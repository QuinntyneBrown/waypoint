#!/usr/bin/env node
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const manifest = JSON.parse(readFileSync(join(ROOT, 'scripts', 'route-manifest.json'), 'utf8'));

function parseSource(relativePath) {
  const path = join(ROOT, relativePath);
  return ts.createSourceFile(
    path,
    readFileSync(path, 'utf8'),
    ts.ScriptTarget.Latest,
    true,
    ts.ScriptKind.TS,
  );
}

function property(object, name) {
  return object.properties.find(
    (candidate) =>
      ts.isPropertyAssignment(candidate) &&
      ((ts.isIdentifier(candidate.name) && candidate.name.text === name) ||
        (ts.isStringLiteral(candidate.name) && candidate.name.text === name)),
  )?.initializer;
}

function stringValue(node) {
  return node && (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node))
    ? node.text
    : undefined;
}

function variableInitializer(source, name) {
  let result;
  function visit(node) {
    if (ts.isVariableDeclaration(node) && ts.isIdentifier(node.name) && node.name.text === name) {
      result = node.initializer;
      return;
    }
    ts.forEachChild(node, visit);
  }
  visit(source);
  if (!result) throw new Error(`Could not find exported variable ${name} in ${source.fileName}`);
  return result;
}

function routePath(parent, child) {
  if (!child) return parent || '/';
  if (!parent || parent === '/') return `/${child}`;
  return `${parent}/${child}`;
}

function collectRoutes(array, parent = '') {
  const routes = [];
  for (const element of array.elements) {
    if (!ts.isObjectLiteralExpression(element)) continue;
    const path = stringValue(property(element, 'path'));
    if (path === undefined) continue;
    const fullPath = routePath(parent, path);
    routes.push(fullPath);
    const children = property(element, 'children');
    if (children && ts.isArrayLiteralExpression(children)) {
      routes.push(...collectRoutes(children, fullPath === '/' ? '' : fullPath));
    }
  }
  return routes;
}

function collectNavigation(source) {
  const initializer = variableInitializer(source, 'W_NAV');
  if (!ts.isObjectLiteralExpression(initializer))
    throw new Error('W_NAV must be an object literal');

  const navigation = [];
  const errors = [];
  for (const roleEntry of initializer.properties) {
    if (!ts.isPropertyAssignment(roleEntry) || !ts.isObjectLiteralExpression(roleEntry.initializer))
      continue;
    const role = roleEntry.name.getText(source).replaceAll(/["']/g, '');
    const roleConfig = roleEntry.initializer;
    const brandPath = stringValue(property(roleConfig, 'brandPath'));
    if (brandPath) navigation.push({ role, key: 'brand', path: brandPath });

    const items = property(roleConfig, 'items');
    const keys = new Set();
    if (items && ts.isArrayLiteralExpression(items)) {
      for (const item of items.elements) {
        if (!ts.isObjectLiteralExpression(item)) continue;
        const key = stringValue(property(item, 'key'));
        const path = stringValue(property(item, 'path'));
        if (!key || !path) continue;
        if (keys.has(key)) errors.push(`${role} navigation repeats key ${key}`);
        keys.add(key);
        navigation.push({ role, key, path });
      }
    }

    const botnav = property(roleConfig, 'botnav');
    if (botnav && ts.isArrayLiteralExpression(botnav)) {
      for (const keyNode of botnav.elements) {
        const key = stringValue(keyNode);
        if (key && !keys.has(key))
          errors.push(`${role} bottom navigation references missing key ${key}`);
      }
    }
  }
  return { navigation, errors };
}

const portalSource = parseSource(manifest.portalRoutesSource);
const publicSource = parseSource(manifest.publicRoutesSource);
const portalInitializer = variableInitializer(portalSource, 'routes');
const publicInitializer = variableInitializer(publicSource, 'routes');
if (
  !ts.isArrayLiteralExpression(portalInitializer) ||
  !ts.isArrayLiteralExpression(publicInitializer)
)
  throw new Error('Application route manifests must be array literals');

const portalRoutes = new Set(collectRoutes(portalInitializer));
const publicRoutes = new Set(collectRoutes(publicInitializer));
const { navigation, errors } = collectNavigation(parseSource(manifest.navigationSource));
const allowlist = new Map(
  manifest.allowedUnresolvedNavigation.map((entry) => [
    `${entry.role}:${entry.path}`,
    entry.reason,
  ]),
);

for (const path of manifest.requiredPortalRoutes) {
  if (!portalRoutes.has(path)) errors.push(`required portal route is missing: ${path}`);
}
for (const path of manifest.requiredPublicRoutes) {
  if (!publicRoutes.has(path)) errors.push(`required public route is missing: ${path}`);
}

for (const entry of navigation) {
  const allowKey = `${entry.role}:${entry.path}`;
  if (!portalRoutes.has(entry.path) && !allowlist.has(allowKey)) {
    errors.push(`${entry.role} navigation ${entry.key} points to an unknown route: ${entry.path}`);
  }
}

for (const [allowKey] of allowlist) {
  const [role, path] = allowKey.split(':');
  if (!navigation.some((entry) => entry.role === role && entry.path === path))
    errors.push(`stale unresolved-navigation entry no longer exists in W_NAV: ${allowKey}`);
  if (portalRoutes.has(path))
    errors.push(
      `stale unresolved-navigation entry now has a real route and must be removed: ${allowKey}`,
    );
}

if (errors.length > 0) {
  console.error(`ROUTE MANIFEST GATE: FAIL\n- ${errors.join('\n- ')}`);
  process.exit(1);
}

console.log(
  `ROUTE MANIFEST GATE: PASS (${portalRoutes.size} portal routes, ${publicRoutes.size} public routes, ` +
    `${navigation.length} navigation links, ${allowlist.size} documented unresolved links)`,
);
