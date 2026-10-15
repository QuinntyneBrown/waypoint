import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const frontendRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const packageDefinitions = [
  { directory: 'api', name: '@waypoint/api' },
  { directory: 'components', name: '@waypoint/components' },
  {
    directory: 'auth',
    name: '@waypoint/auth',
    expectedPeers: { '@waypoint/api': '0.0.1', '@waypoint/components': '0.0.1' },
  },
  {
    directory: 'core',
    name: '@waypoint/core',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
    },
  },
  {
    directory: 'account',
    name: '@waypoint/account',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
      '@waypoint/core': '0.0.1',
    },
  },
  {
    directory: 'youth',
    name: '@waypoint/youth',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
      '@waypoint/core': '0.0.1',
    },
  },
  {
    directory: 'mentor',
    name: '@waypoint/mentor',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
      '@waypoint/core': '0.0.1',
    },
  },
  {
    directory: 'admin',
    name: '@waypoint/admin',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
      '@waypoint/core': '0.0.1',
    },
  },
  {
    directory: 'guardian',
    name: '@waypoint/guardian',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
    },
  },
  {
    directory: 'volunteer',
    name: '@waypoint/volunteer',
    expectedPeers: {
      '@waypoint/api': '0.0.1',
      '@waypoint/auth': '0.0.1',
      '@waypoint/components': '0.0.1',
      '@waypoint/core': '0.0.1',
    },
  },
];

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

function readJson(path) {
  return JSON.parse(readFileSync(path, 'utf8'));
}

function artifactPath(directory, path) {
  return join(directory, path.replace(/^\.\//, ''));
}

function packageNameFromSpecifier(specifier) {
  return specifier.split('/').slice(0, 2).join('/');
}

function sourceFiles(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) {
      return sourceFiles(path);
    }

    return /\.(?:d\.ts|mjs|js)$/.test(entry.name) ? [path] : [];
  });
}

function packageDryRun(directory) {
  const arguments_ = ['pack', '--dry-run', '--json'];
  const options = {
    cwd: directory,
    encoding: 'utf8',
    stdio: ['ignore', 'pipe', 'pipe'],
    windowsHide: true,
  };

  const npmExecPath = process.env.npm_execpath;
  const output = npmExecPath
    ? execFileSync(process.execPath, [npmExecPath, ...arguments_], options)
    : execFileSync(process.platform === 'win32' ? 'npm.cmd' : 'npm', arguments_, {
        ...options,
        shell: process.platform === 'win32',
      });

  const results = JSON.parse(output);
  assert(results.length === 1, `Expected one npm pack result for ${directory}.`);
  return results[0];
}

const builtPackages = new Map();

for (const definition of packageDefinitions) {
  const directory = join(frontendRoot, 'dist', definition.directory);
  const manifestPath = join(directory, 'package.json');
  assert(
    existsSync(manifestPath),
    `Missing ${relative(frontendRoot, manifestPath)}. Run npm run packages:build first.`,
  );

  const manifest = readJson(manifestPath);
  assert(
    manifest.name === definition.name,
    `${definition.directory} was packed as ${manifest.name}; expected ${definition.name}.`,
  );
  assert(typeof manifest.version === 'string', `${definition.name} has no version.`);
  assert(manifest.private !== true, `${definition.name} is marked private.`);
  assert(manifest.sideEffects === false, `${definition.name} must remain side-effect free.`);
  assert(typeof manifest.module === 'string', `${definition.name} has no module entry.`);
  assert(typeof manifest.typings === 'string', `${definition.name} has no typings entry.`);
  assert(manifest.exports?.['.'], `${definition.name} has no root export.`);

  for (const [dependency, version] of Object.entries(definition.expectedPeers ?? {})) {
    assert(
      manifest.peerDependencies?.[dependency] === version,
      `${definition.name} must declare ${dependency}@${version} as a peer dependency.`,
    );
  }

  for (const entry of [manifest.module, manifest.typings]) {
    assert(
      existsSync(artifactPath(directory, entry)),
      `${definition.name} points to missing artifact ${entry}.`,
    );
  }

  const dryRun = packageDryRun(directory);
  assert(dryRun.name === definition.name, `npm pack renamed ${definition.name}.`);
  assert(
    dryRun.version === manifest.version,
    `npm pack reported the wrong version for ${definition.name}.`,
  );

  const packedFiles = new Set(dryRun.files.map(({ path }) => path));
  for (const entry of ['package.json', manifest.module, manifest.typings]) {
    assert(
      packedFiles.has(entry.replace(/^\.\//, '')),
      `${definition.name} would omit ${entry} from its tarball.`,
    );
  }

  builtPackages.set(definition.name, { directory, manifest });
}

const packageImports = new Map();
const importPatterns = [
  /\b(?:from|import\s*\()\s*["'](@waypoint\/[^"']+)["']/g,
  /\bimport\s*["'](@waypoint\/[^"']+)["']/g,
];

for (const [name, { directory, manifest }] of builtPackages) {
  const imports = new Set();
  for (const file of sourceFiles(directory)) {
    const contents = readFileSync(file, 'utf8');
    for (const pattern of importPatterns) {
      for (const match of contents.matchAll(pattern)) {
        imports.add(packageNameFromSpecifier(match[1]));
      }
    }
  }

  for (const importedName of imports) {
    assert(
      builtPackages.has(importedName),
      `${name} imports unknown workspace package ${importedName}.`,
    );
    if (importedName === name) {
      continue;
    }

    const declaredVersion =
      manifest.peerDependencies?.[importedName] ??
      manifest.dependencies?.[importedName] ??
      manifest.optionalDependencies?.[importedName];
    assert(
      declaredVersion,
      `${name} imports ${importedName} without declaring it as a dependency.`,
    );
  }

  packageImports.set(
    name,
    [...imports].filter((entry) => entry !== name),
  );
}

const allowedImports = {
  '@waypoint/api': [],
  '@waypoint/components': [],
};
for (const [name, imports] of packageImports) {
  const allowed = allowedImports[name];
  if (!allowed) continue;
  for (const importedName of imports) {
    assert(allowed.includes(importedName), `${name} must not depend on ${importedName}.`);
  }
}
for (const [name, imports] of packageImports) {
  for (const importedName of imports) {
    assert(
      ![
        '@waypoint/youth',
        '@waypoint/mentor',
        '@waypoint/admin',
        '@waypoint/guardian',
        '@waypoint/volunteer',
        '@waypoint/account',
      ].includes(importedName),
      `${name} must not depend on the role feature library ${importedName}.`,
    );
  }
}

const importSummary = [...packageImports]
  .filter(([, imports]) => imports.length > 0)
  .map(([name, imports]) => `${name} -> ${imports.join(', ')}`)
  .join('; ');

console.log(
  `Package artifact check passed for ${packageDefinitions.map(({ name }) => name).join(', ')} (${packageDefinitions.length} npm pack dry runs${importSummary ? `; ${importSummary}` : ''}).`,
);
