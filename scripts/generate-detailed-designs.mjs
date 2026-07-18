import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "..");
const detailedRoot = path.join(repoRoot, "docs", "detailed-designs");
const specsRoot = path.join(repoRoot, "docs", "specs");
const manifestPath = path.join(detailedRoot, "feature-slices.json");

const subsystemTypes = {
  "application-intake": "ApplicationIntake",
  authentication: "Authentication",
  "consent-and-safeguarding": "ConsentAndSafeguarding",
  "curriculum-authoring": "CurriculumAuthoring",
  engagement: "Engagement",
  "guardian-portal": "GuardianPortal",
  "learning-experience": "LearningExperience",
  mentoring: "Mentoring",
  "non-functional": "NonFunctional",
  "platform-services": "PlatformServices",
  "program-administration": "ProgramAdministration",
  "public-site": "PublicSite",
  "reporting-and-data": "ReportingAndData",
  "volunteer-portal": "VolunteerPortal",
};

const subsystemNames = {
  "application-intake": "Application Intake",
  authentication: "Authentication and Access",
  "consent-and-safeguarding": "Consent and Safeguarding",
  "curriculum-authoring": "Curriculum Authoring",
  engagement: "Engagement and Operations",
  "guardian-portal": "Guardian Portal",
  "learning-experience": "Learning Experience",
  mentoring: "Mentoring and Coaching",
  "non-functional": "Non-Functional and Quality",
  "platform-services": "Platform Services",
  "program-administration": "Program Administration",
  "public-site": "Public Site",
  "reporting-and-data": "Reporting and Data",
  "volunteer-portal": "Volunteer Portal",
};

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
if (manifest.version !== 1 || !Array.isArray(manifest.slices)) {
  throw new Error("Unsupported feature-slice manifest.");
}

function toPosix(value) {
  return value.split(path.sep).join("/");
}

function walk(directory) {
  if (!fs.existsSync(directory)) return [];
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(directory, entry.name);
    return entry.isDirectory() ? walk(fullPath) : [fullPath];
  });
}

function normalizeNormativeText(value) {
  return value
    .replace(/\bmust not\b/gi, "shall not")
    .replace(/\bmust\b/gi, "shall")
    .replace(/\bneeds to\b/gi, "shall")
    .replace(/\bneed to\b/gi, "shall")
    .replace(/\bhas to\b/gi, "shall")
    .replace(/\bhave to\b/gi, "shall")
    .replace(/\bis required to\b/gi, "shall")
    .replace(/\bare required to\b/gi, "shall");
}

function parseSpecs() {
  const specs = new Map();
  for (const subsystem of Object.keys(subsystemTypes)) {
    const file = path.join(specsRoot, subsystem, "L2.md");
    const text = fs.readFileSync(file, "utf8");
    const headings = [...text.matchAll(/^## (L2-[A-Z]+-[0-9]+): (.+)$/gm)];
    headings.forEach((heading, index) => {
      const start = heading.index;
      const end = index + 1 < headings.length ? headings[index + 1].index : text.length;
      const section = text.slice(start, end).trim();
      const trace = section.match(/^\*\*Traces to:\*\*\s+`?(L1-[A-Z]+-[0-9]+)`?\s*$/m);
      if (!trace) throw new Error(`Missing L1 trace for ${heading[1]}.`);
      const traceEnd = trace.index + trace[0].length;
      const acceptanceIndex = section.indexOf("**Acceptance Criteria:**", traceEnd);
      const body = section
        .slice(traceEnd, acceptanceIndex >= 0 ? acceptanceIndex : section.length)
        .trim()
        .replace(/^---\s*$/gm, "")
        .trim();
      specs.set(heading[1], {
        id: heading[1],
        title: heading[2].trim(),
        l1: trace[1],
        text: normalizeNormativeText(body),
        subsystem,
      });
    });
  }
  return specs;
}

const specs = parseSpecs();
if (specs.size !== 88) throw new Error(`Expected 88 L2 specifications, found ${specs.size}.`);

const primaryIds = new Set(
  manifest.slices.flatMap((slice) =>
    slice.requirements.filter((item) => item.relationship === "primary").map((item) => item.id),
  ),
);
const uncoveredSpecs = [...specs.keys()].filter((id) => !primaryIds.has(id));
if (uncoveredSpecs.length > 0) {
  throw new Error(`Feature slices do not cover these L2 specifications: ${uncoveredSpecs.join(", ")}`);
}

const stopWords = new Set([
  "about", "allow", "backend", "feature", "flow", "frontend", "information", "operation",
  "provide", "software", "system", "word", "with", "within", "from", "into", "through",
  "and", "the", "for", "that", "this", "their", "they", "when", "where", "which", "who",
]);

const subsystemAliases = {
  "application-intake": ["application", "intake"],
  authentication: ["auth", "authentication"],
  "consent-and-safeguarding": ["audit", "consent", "safeguarding"],
  "curriculum-authoring": ["curriculum"],
  engagement: ["engagement"],
  "guardian-portal": ["guardian"],
  "learning-experience": ["journey", "learning", "youth"],
  mentoring: ["mentor", "mentoring"],
  "non-functional": ["health", "middleware", "security"],
  "platform-services": ["access", "platform", "preferences", "profile", "support"],
  "program-administration": ["admin", "cohort", "people", "settings"],
  "public-site": ["public"],
  "reporting-and-data": ["data", "report", "reporting"],
  "volunteer-portal": ["volunteer"],
};

function tokenise(value) {
  return [...new Set(
    String(value)
      .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, " ")
      .trim()
      .split(/\s+/)
      .filter((token) => token.length >= 3 && !stopWords.has(token)),
  )];
}

function pascalCase(value) {
  return String(value)
    .replace(/[^a-zA-Z0-9]+/g, " ")
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .map((part) => part[0].toUpperCase() + part.slice(1))
    .join("");
}

function extractDeclarations(file) {
  const text = fs.readFileSync(file, "utf8");
  const declarations = [];
  if (file.endsWith(".cs")) {
    for (const match of text.matchAll(/\b(?:public|internal)\s+(?:(?:sealed|static|abstract|partial)\s+)*(class|interface|enum|record(?:\s+(?:class|struct))?)\s+(\w+)/g)) {
      declarations.push({ kind: match[1].split(/\s+/)[0], name: match[2] });
    }
  } else if (file.endsWith(".ts")) {
    for (const match of text.matchAll(/\bexport\s+(?:(?:default|abstract)\s+)*(class|interface|type|function|const)\s+(\w+)/g)) {
      declarations.push({ kind: match[1], name: match[2] });
    }
  }
  return [...new Map(declarations.map((item) => [`${item.kind}:${item.name}`, item])).values()];
}

const sourceFiles = [
  ...walk(path.join(repoRoot, "backend", "src")).filter((file) => file.endsWith(".cs")),
  ...walk(path.join(repoRoot, "frontend", "projects")).filter(
    (file) => file.endsWith(".ts") && !file.endsWith(".spec.ts"),
  ),
];

const sourceElements = sourceFiles.flatMap((file) => {
  const relativePath = toPosix(path.relative(repoRoot, file));
  return extractDeclarations(file).map((declaration) => ({
    ...declaration,
    path: relativePath,
    nameTokens: tokenise(declaration.name),
    pathTokens: tokenise(relativePath),
  }));
});

function featureTokens(slice) {
  const requirementVocabulary = slice.requirements
    .map((item) => {
      const specification = specs.get(item.id);
      return specification ? `${specification.title} ${specification.text}` : "";
    })
    .join(" ");
  return {
    primary: tokenise(`${slice.slug} ${slice.title}`),
    secondary: tokenise(`${slice.overview} ${slice.processing} ${requirementVocabulary}`),
    subsystem: subsystemAliases[slice.subsystem] ?? tokenise(slice.subsystem),
  };
}

function matchScore(element, tokens) {
  let score = 0;
  const addMatches = (wantedTokens, actualTokens, exactWeight, prefixWeight) => {
    for (const wanted of wantedTokens) {
      for (const actual of actualTokens) {
        if (wanted === actual) score += exactWeight;
        else if (wanted.length >= 5 && actual.length >= 5 && (wanted.startsWith(actual) || actual.startsWith(wanted))) score += prefixWeight;
      }
    }
  };
  addMatches(tokens.primary, element.nameTokens, 12, 4);
  addMatches(tokens.primary, element.pathTokens, 6, 2);
  addMatches(tokens.secondary, element.nameTokens, 3, 1);
  addMatches(tokens.secondary, element.pathTokens, 1, 0);
  addMatches(tokens.subsystem, element.nameTokens, 6, 2);
  return score;
}

function topMatches(candidates, tokens, limit, minimum = 2) {
  return candidates
    .map((element) => ({ ...element, score: matchScore(element, tokens) }))
    .filter((element) => element.score >= minimum)
    .sort((left, right) =>
      right.score - left.score ||
      Number(/Handler$/.test(right.name)) - Number(/Handler$/.test(left.name)) ||
      Number(right.kind === "class") - Number(left.kind === "class") ||
      left.name.localeCompare(right.name))
    .slice(0, limit);
}

function designElements(slice) {
  const typeFolder = subsystemTypes[slice.subsystem];
  const tokens = featureTokens(slice);
  const backendPrefix = "backend/src/";
  const controllerCandidates = sourceElements.filter((element) =>
    element.kind === "class" && element.path.startsWith(`${backendPrefix}WordUp.Api/Controllers/${typeFolder}/`),
  );
  const applicationCandidates = sourceElements.filter((element) =>
    element.path.startsWith(`${backendPrefix}WordUp.Application/${typeFolder}/`),
  );
  const domainCandidates = sourceElements.filter((element) =>
    ["class", "record", "enum"].includes(element.kind) && element.path.startsWith(`${backendPrefix}WordUp.Domain/${typeFolder}/`),
  );
  const broadBackendCandidates = sourceElements.filter((element) =>
    element.path.startsWith(backendPrefix) &&
    (element.path.includes("/Infrastructure/") ||
      element.path.includes("/Middleware/") ||
      element.path.includes("/Realtime/") ||
      element.path.includes("/Shared/")),
  );
  const frontendCandidates = sourceElements.filter((element) =>
    element.kind === "class" && element.path.startsWith("frontend/projects/") && !element.path.includes("/wu-ui/") && !element.path.includes("/wu-api/"),
  );
  const apiClientCandidates = sourceElements.filter((element) =>
    element.kind === "class" && element.path.includes("/wu-api/") && /(?:Api|Client|Service)$/.test(element.name),
  );

  const controllers = topMatches(controllerCandidates, tokens, 1, 1);
  let application = topMatches(applicationCandidates, tokens, 8, 2);
  let domain = topMatches(domainCandidates, tokens, 4, 2);
  const infrastructure = topMatches(broadBackendCandidates, tokens, 3, 3);
  const frontend = topMatches(frontendCandidates, tokens, 2, 3);
  const apiClients = topMatches(apiClientCandidates, tokens, 1, 2);

  const readOnly = /^(browse|explore|inspect|navigate|read|show|view|await)-/.test(slice.slug);
  const baseName = pascalCase(slice.slug);
  const request = application.find((element) => /(?:Command|Query)$/.test(element.name)) ?? {
    name: `${baseName}${readOnly ? "Query" : "Command"}`,
    planned: true,
  };
  const requestBase = request.name.replace(/(?:Command|Query)$/, "");
  const handler = application.find((element) => element.name === `${request.name}Handler`) ??
    application.find((element) => element.name.startsWith(requestBase) && /Handler$/.test(element.name)) ?? {
      name: `${request.name}Handler`,
      planned: true,
    };
  const requestFamily = application.filter((element) => element.name.startsWith(requestBase));
  application = [...new Map([
    request,
    handler,
    ...requestFamily,
  ].map((element) => [element.name, element])).values()].slice(0, 6);
  if (handler.path) {
    const handlerSource = fs.readFileSync(path.join(repoRoot, handler.path), "utf8");
    const referencedDomain = domainCandidates.filter((element) => {
      const singular = new RegExp(`\\b${element.name}\\b`);
      const plural = new RegExp(`\\b${element.name}(?:s|es)\\b`);
      return singular.test(handlerSource) || plural.test(handlerSource);
    }).sort((left, right) => {
      const leftClass = left.kind === "class" ? 1 : 0;
      const rightClass = right.kind === "class" ? 1 : 0;
      return rightClass - leftClass || left.name.localeCompare(right.name);
    });
    domain = [...new Map([...referencedDomain, ...domain].map((element) => [element.name, element])).values()].slice(0, 4);
  }
  const controller = controllers[0] ?? { name: `${typeFolder}Controller`, planned: true };
  const entity = domain[0] ?? { name: `${baseName}State`, planned: true };
  const page = frontend[0] ?? { name: `${baseName}Page`, planned: true };
  const apiClient = apiClients[0] ?? { name: `${typeFolder}Api`, planned: true };

  return {
    tokens,
    readOnly,
    controllers,
    application,
    domain,
    infrastructure,
    frontend,
    apiClients,
    controller,
    request,
    handler,
    entity,
    page,
    apiClient,
  };
}

function actorFor(slice) {
  const text = `${slice.slug} ${slice.title} ${slice.overview}`.toLowerCase();
  if (slice.subsystem === "public-site") return "Anonymous visitor";
  if (slice.subsystem === "volunteer-portal") return "Volunteer";
  if (slice.subsystem === "guardian-portal") return "Guardian";
  if (slice.subsystem === "learning-experience") return "Youth participant";
  if (slice.subsystem === "mentoring") return "Mentor";
  if (slice.subsystem === "curriculum-authoring" || slice.subsystem === "program-administration" || slice.subsystem === "reporting-and-data") return "Program administrator";
  if (slice.subsystem === "application-intake") return /review|decide|enroll/.test(text) ? "Program administrator" : "Applicant or referrer";
  if (slice.subsystem === "authentication") return /invitation|authorize|scope/.test(text) ? "Program administrator or account holder" : "Account holder";
  if (slice.subsystem === "consent-and-safeguarding") return /audit/.test(text) ? "Program administrator" : "Guardian or youth participant";
  if (slice.subsystem === "engagement") {
    if (/attendance|session|calendar|announcement/.test(text)) return "Mentor, volunteer, or program administrator";
    return "Authenticated participant";
  }
  if (slice.subsystem === "non-functional") return /operate|observe/.test(text) ? "Platform operator" : "Word Up participant";
  return "Authenticated participant";
}

function actorSubject(slice) {
  const actor = actorFor(slice);
  const subjects = {
    "Anonymous visitor": "An anonymous visitor",
    Volunteer: "A volunteer",
    Guardian: "A guardian",
    "Youth participant": "A youth participant",
    Mentor: "A mentor",
    "Program administrator": "A program administrator",
    "Applicant or referrer": "An applicant or referrer",
    "Program administrator or account holder": "A program administrator or account holder",
    "Account holder": "An account holder",
    "Guardian or youth participant": "A guardian or youth participant",
    "Mentor, volunteer, or program administrator": "A mentor, volunteer, or program administrator",
    "Authenticated participant": "An authenticated participant",
    "Platform operator": "A platform operator",
    "Word Up participant": "A Word Up participant",
  };
  return subjects[actor] ?? `A ${actor.toLowerCase()}`;
}

function primarySpecifications(slice) {
  return slice.requirements
    .filter((item) => item.relationship === "primary")
    .map((item) => specs.get(item.id))
    .filter(Boolean);
}

function overviewText(slice) {
  if (slice.overview) return normalizeNormativeText(slice.overview);
  const primary = primarySpecifications(slice);
  const trace = primary.map((specification) => `\`${specification.id}\``).join(", ");
  const sourceText = primary.map((specification) => specification.text).join(" ");
  return `The ${slice.title} feature slice refines ${trace} into one independently deliverable interaction. ${sourceText}`;
}

function entryText(slice) {
  if (slice.entry) return normalizeNormativeText(slice.entry);
  return `${actorSubject(slice)} starts this feature interaction through the ${primaryFrontendApplication(slice)}.`;
}

function processingText(slice) {
  if (slice.processing) return normalizeNormativeText(slice.processing);
  const requirementIds = slice.requirements.map((item) => `\`${item.id}\``).join(", ");
  return `The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies ${requirementIds}, scopes the operation to the current actor, and queries or changes domain state through the persistence port.`;
}

function resultText(slice, readOnly = /^(browse|explore|inspect|navigate|read|show|view|await)-/.test(slice.slug)) {
  if (slice.result) return normalizeNormativeText(slice.result);
  if (readOnly) return "The frontend application presents an authorized and bounded feature result.";
  return "The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.";
}

function scopeBoundaryText(slice) {
  if (slice.scopeBoundary) return normalizeNormativeText(slice.scopeBoundary);
  const primaryIdsForSlice = new Set(slice.requirements.filter((item) => item.relationship === "primary").map((item) => item.id));
  const siblings = manifest.slices.filter((candidate) =>
    candidate.subsystem === slice.subsystem &&
    candidate.slug !== slice.slug &&
    candidate.requirements.some((item) => item.relationship === "primary" && primaryIdsForSlice.has(item.id)),
  );
  if (siblings.length === 0) return "Adjacent subsystem feature slices own behavior outside this end-to-end outcome.";
  return `${siblings.map((candidate) => candidate.title).join("; ")} ${siblings.length === 1 ? "is an adjacent feature slice that owns" : "are adjacent feature slices that own"} other outcomes within the same primary L2 capability.`;
}

function participatingApplications(slice) {
  const values = Array.isArray(slice.applications) ? [...slice.applications] : [];
  if (!values.some((value) => /WordUp API/i.test(value))) values.push("WordUp API application");
  return [...new Set(values)];
}

function primaryFrontendApplication(slice) {
  const applications = participatingApplications(slice);
  return applications.find((value) => /Portal|Public Site/i.test(value)) ?? "Portal application";
}

function externalSystemsFor(slice) {
  const text = `${slice.slug} ${slice.title} ${slice.overview} ${slice.processing}`.toLowerCase();
  const systems = [];
  if (/email|invitation|password|notification|contact|application/.test(text)) systems.push("Email delivery service");
  if (/donation/.test(text)) systems.push("External donation provider");
  return systems;
}

function markdownEscape(value) {
  return String(value).replace(/\|/g, "\\|").replace(/\r?\n/g, "<br>");
}

function linkedElement(element) {
  if (!element || element.planned || !element.path) return `\`${element?.name ?? "<TO SUPPLY>"}\` *(planned boundary)*`;
  return `[\`${element.name}\`](../../../../${element.path})`;
}

function linkedElements(elements, fallback) {
  if (!elements || elements.length === 0) return linkedElement(fallback);
  return elements.map(linkedElement).join("<br>");
}

function shortText(value, fallback, maximum = 96) {
  const normalized = String(value || fallback).replace(/\s+/g, " ").trim();
  if (normalized.length <= maximum) return normalized;
  return `${normalized.slice(0, maximum - 1).trim()}…`;
}

function pumlText(value) {
  return String(value)
    .replace(/\\/g, "\\\\")
    .replace(/"/g, "'")
    .replace(/\r?\n/g, "\\n");
}

function alias(value) {
  const normalized = String(value).replace(/[^a-zA-Z0-9_]/g, "_");
  return /^[0-9]/.test(normalized) ? `n_${normalized}` : normalized;
}

function diagramHeader(title, direction = "left to right direction") {
  return `@startuml\n${direction}\ntitle ${pumlText(title)}\nskinparam backgroundColor #FFFFFF\nskinparam shadowing false\nskinparam roundcorner 12\nskinparam defaultFontName Arial\nskinparam ArrowColor #344054\nskinparam rectangle {\n  BorderColor #344054\n  FontColor #101828\n}\nskinparam package {\n  BorderColor #667085\n  FontColor #101828\n  BackgroundColor #F9FAFB\n}\n`;
}

function c4DiagramHeader(title) {
  return `@startuml\n!include <C4/C4_Component>\nLAYOUT_LEFT_RIGHT()\ntitle ${pumlText(title)}\n`;
}

function systemContextDiagram(slice) {
  const actor = actorFor(slice);
  const external = externalSystemsFor(slice);
  const externalNodes = external.map((name, index) => `System_Ext(ext${index + 1}, "${pumlText(name)}", "Independently operated dependency")`).join("\n");
  const externalLinks = external.map((name, index) => `Rel(wordup, ext${index + 1}, "Exchanges ${pumlText(name.toLowerCase().replace(" service", ""))} data")`).join("\n");
  return `${c4DiagramHeader(`C4 level 1 — ${slice.title} — system context`)}Person(primary, "${pumlText(actor)}", "Human role outside the software boundary")\nPerson(staff, "Program staff", "Administers authorized program activity")\nSystem(wordup, "Word Up software system", "Youth leadership, learning, mentoring, and program operations")\n${externalNodes}\nRel(primary, wordup, "${pumlText(shortText(entryText(slice), `Starts ${slice.title.toLowerCase()}`, 72))}")\nRel(staff, wordup, "Administers authorized program activity")\n${externalLinks}\nSHOW_LEGEND()\n@enduml\n`;
}

function containerDiagram(slice) {
  const actor = actorFor(slice);
  const apps = participatingApplications(slice).filter((value) => !/WordUp API/i.test(value));
  const appNodes = apps.map((name, index) => `    Container(app${index + 1}, "${pumlText(name)}", "Angular", "${pumlText(slice.title)} interaction")`).join("\n");
  const appLinks = apps.map((_name, index) => `Rel(app${index + 1}, api, "Calls", "HTTPS JSON")`).join("\n");
  return `${c4DiagramHeader(`C4 level 2 — ${slice.title} — containers`)}Person(primary, "${pumlText(actor)}", "Uses the feature")\nSystem_Boundary(wordup, "Word Up software system") {\n  Container_Boundary(frontend, "Frontend") {\n${appNodes}\n  }\n  Container_Boundary(backend, "Backend") {\n    Container(api, "WordUp API application", "ASP.NET Core", "Authentication, policy, and feature endpoints")\n    Container(realtime, "Realtime gateway", "SignalR", "Feature change delivery")\n  }\n  ContainerDb(db, "Word Up database", "SQL Server", "Authoritative feature state")\n}\nRel(primary, app1, "Uses")\n${appLinks}\nRel(api, db, "Reads and writes", "EF Core")\nRel(api, realtime, "Publishes authorized changes")\nRel(realtime, app1, "Pushes subscribed events", "SignalR")\nSHOW_LEGEND()\n@enduml\n`;
}

function componentDiagram(slice, elements) {
  const frontendApp = primaryFrontendApplication(slice);
  return `${c4DiagramHeader(`C4 level 3 — ${slice.title} — components`)}System_Boundary(wordup, "Word Up software system") {\n  Container_Boundary(frontend, "Frontend") {\n    Container_Boundary(frontend_app, "${pumlText(frontendApp)}") {\n      Component(page, "${pumlText(elements.page.name)}", "Angular page component", "Owns feature interaction state and presentation")\n      Component(client, "${pumlText(elements.apiClient.name)}", "Typed API client", "Calls the feature API with typed requests and results")\n    }\n  }\n  Container_Boundary(backend, "Backend") {\n    Container_Boundary(api_app, "WordUp API application") {\n      Component(controller, "${pumlText(elements.controller.name)}", "API controller", "Applies transport policy and maps requests")\n    }\n    Container_Boundary(application, "WordUp Application") {\n      Component(request, "${pumlText(elements.request.name)}", "MediatR request", "Represents the application use case")\n      Component(handler, "${pumlText(elements.handler.name)}", "MediatR handler", "Coordinates authorized feature behavior")\n    }\n    Container_Boundary(domain, "WordUp Domain") {\n      Component(entity, "${pumlText(elements.entity.name)}", "Domain type", "Holds feature state and invariants")\n    }\n    Container_Boundary(infrastructure, "WordUp Infrastructure") {\n      Component(persistence, "AppDbContext", "EF Core adapter", "Queries and persists authoritative state")\n    }\n  }\n}\nRel(page, client, "Calls")\nRel(client, controller, "Calls", "HTTPS JSON")\nRel(controller, request, "Constructs")\nRel(controller, handler, "Sends through MediatR")\nRel(handler, entity, "Reads or changes")\nRel(handler, persistence, "Queries or persists")\nRel(persistence, entity, "Maps")\nSHOW_LEGEND()\n@enduml\n`;
}

function codeDiagram(slice, elements) {
  const frontendApp = primaryFrontendApplication(slice);
  return `${c4DiagramHeader(`C4 level 4 — ${slice.title} — code elements`)}System_Boundary(wordup, "Word Up software system") {\n  Container_Boundary(frontend, "Frontend") {\n    Container_Boundary(frontend_app, "${pumlText(frontendApp)}") {\n      Component(page, "${pumlText(elements.page.name)}", "Angular component", "Feature page code element")\n      Component(client, "${pumlText(elements.apiClient.name)}", "TypeScript API client", "Typed API boundary code element")\n    }\n  }\n  Container_Boundary(backend, "Backend") {\n    Container_Boundary(api_app, "WordUp API application") {\n      Component(controller, "${pumlText(elements.controller.name)}", "C# controller", "HTTP endpoint code element")\n    }\n    Container_Boundary(application, "WordUp Application") {\n      Component(request, "${pumlText(elements.request.name)}", "C# request record", "Application request code element")\n      Component(handler, "${pumlText(elements.handler.name)}", "C# handler class", "Application handler code element")\n      Component(dbport, "IAppDbContext", "C# interface", "Persistence port")\n      Component(current, "ICurrentUser", "C# interface", "Current-user port")\n    }\n    Container_Boundary(domain, "WordUp Domain") {\n      Component(entity, "${pumlText(elements.entity.name)}", "C# domain type", "Feature domain code element")\n    }\n  }\n}\nRel(page, client, "Invokes")\nRel(client, controller, "Calls")\nRel(controller, request, "Constructs")\nRel(request, handler, "Dispatched to")\nRel(handler, dbport, "Depends on")\nRel(handler, current, "Scopes by")\nRel(handler, entity, "Reads or changes")\nSHOW_LEGEND()\n@enduml\n`;
}

function classDiagram(slice, elements) {
  const operation = elements.readOnly ? "load" : "submit";
  const apiOperation = elements.readOnly ? "get" : "execute";
  const controllerOperation = elements.readOnly ? "GetAsync" : "PostAsync";
  const pageAlias = alias(`page_${elements.page.name}`);
  const clientAlias = alias(`client_${elements.apiClient.name}`);
  const controllerAlias = alias(`controller_${elements.controller.name}`);
  const requestAlias = alias(`request_${elements.request.name}`);
  const handlerAlias = alias(`handler_${elements.handler.name}`);
  const entityAlias = alias(`entity_${elements.entity.name}`);
  return `${diagramHeader(`UML class structure — ${slice.title}`)}package "Frontend — ${pumlText(primaryFrontendApplication(slice))}" {\n  class "${pumlText(elements.page.name)}" as ${pageAlias} <<Angular component>> {\n    +${operation}(): void\n    +renderResult(): void\n  }\n  class "${pumlText(elements.apiClient.name)}" as ${clientAlias} <<API client>> {\n    +${apiOperation}(request): Observable\n  }\n}\npackage "Backend — WordUp API application" {\n  class "${pumlText(elements.controller.name)}" as ${controllerAlias} <<controller>> {\n    +${controllerOperation}(request, cancellationToken): ActionResult\n  }\n}\npackage "Backend — WordUp Application" {\n  class "${pumlText(elements.request.name)}" as ${requestAlias} <<record>>\n  class "${pumlText(elements.handler.name)}" as ${handlerAlias} <<handler>> {\n    +Handle(request, cancellationToken): Result\n  }\n  interface IAppDbContext {\n    +SaveChangesAsync(cancellationToken): int\n  }\n  interface ICurrentUser {\n    +Id: Guid\n    +Role: string\n  }\n}\npackage "Backend — WordUp Domain" {\n  class "${pumlText(elements.entity.name)}" as ${entityAlias} <<domain type>>\n}\n${pageAlias} --> ${clientAlias} : invokes\n${clientAlias} --> ${controllerAlias} : calls\n${controllerAlias} ..> ${requestAlias} : creates\n${requestAlias} --> ${handlerAlias} : dispatched to\n${handlerAlias} --> IAppDbContext : depends on\n${handlerAlias} --> ICurrentUser : scopes by\n${handlerAlias} --> ${entityAlias} : reads or changes\nIAppDbContext o-- ${entityAlias} : persists\nlegend right\n  Solid arrows show runtime collaboration.\n  Dashed arrows show construction.\n  Aggregation shows persistence ownership.\nendlegend\n@enduml\n`;
}

function sequenceDiagram(slice, elements) {
  const actor = actorFor(slice);
  const entry = shortText(`Start ${slice.title.toLowerCase()}`, "Start feature interaction", 52);
  const requirementIds = slice.requirements.map((item) => item.id).join(", ");
  const processing = shortText(`Apply ${requirementIds} feature rules`, "Apply feature rules", 68);
  const result = shortText(`Present ${slice.title.toLowerCase()} result`, "Present feature result", 56);
  const persistenceVerb = elements.readOnly ? "Query authorized state" : "Load and persist authorized state";
  const completionVerb = elements.readOnly ? "Complete bounded projection" : "Save changes in one unit of work";
  return `${diagramHeader(`UML sequence behaviour — ${slice.title}`, "")}actor "${pumlText(actor)}" as actor\nbox "Frontend — ${pumlText(primaryFrontendApplication(slice))}" #E0F2FE\n  participant "${pumlText(elements.page.name)}" as page\n  participant "${pumlText(elements.apiClient.name)}" as client\nend box\nbox "Backend — WordUp API application" #D1FADF\n  participant "${pumlText(elements.controller.name)}" as controller\nend box\nbox "Backend — WordUp Application and Domain" #ECFDF3\n  participant "${pumlText(elements.handler.name)}" as handler\n  participant "${pumlText(elements.entity.name)}" as entity\nend box\nbox "Backend — WordUp Infrastructure" #FFF4E5\n  database "Word Up database" as db\nend box\nactor -> page : ${pumlText(entry)}\npage -> client : Build typed request\nclient -> controller : HTTPS request\ncontroller -> controller : Authenticate and apply endpoint policy\ncontroller -> handler : Send ${pumlText(elements.request.name)}\nhandler -> db : ${persistenceVerb}\ndb --> handler : Current state\nhandler -> entity : ${pumlText(processing)}\nentity --> handler : Valid state transition or projection\nhandler -> db : ${completionVerb}\ndb --> handler : Commit or query result\nhandler --> controller : Typed result\ncontroller --> client : HTTP response or ProblemDetails\nclient --> page : Feature result\npage --> actor : ${pumlText(result)}\n@enduml\n`;
}

function renderRequirements(slice) {
  return slice.requirements.map((reference) => {
    const specification = specs.get(reference.id);
    if (!specification) throw new Error(`Unknown requirement ${reference.id} in ${slice.subsystem}/${slice.slug}.`);
    const relationship = reference.relationship === "primary" ? "Primary" : "Dependency";
    return `### ${specification.id} — ${specification.title}\n\n| Field | Value |\n| --- | --- |\n| Relationship | ${relationship} |\n| L1 specification ID | \`${specification.l1}\` |\n\n${specification.text}`;
  }).join("\n\n");
}

function readmeFor(slice, elements) {
  const subsystemName = subsystemNames[slice.subsystem];
  const applications = participatingApplications(slice).join("; ");
  const sourceNote = "A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.";
  const processing = processingText(slice);
  const scopeBoundary = scopeBoundaryText(slice);
  return `# ${slice.title}\n\n## Overview\n\nWord Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The ${subsystemName} subsystem owns the part of that software concerned with ${slice.title.toLowerCase()}.\n\n${overviewText(slice)}\n\n${entryText(slice)} ${resultText(slice, elements.readOnly)}\n\nThe Word Up software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.\n\n## Description\n\n### Vertical slice boundary\n\nThe feature spans ${applications}. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. ${scopeBoundary}\n\n### Components and types\n\n${sourceNote}\n\n| Layer | Components, types, and classes | Responsibility |\n| --- | --- | --- |\n| Frontend page | ${linkedElements(elements.frontend, elements.page)} | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |\n| Frontend API boundary | ${linkedElements(elements.apiClients, elements.apiClient)} | Serializes requests, deserializes typed results, and maps safe API errors for the page. |\n| API transport | ${linkedElements(elements.controllers, elements.controller)} | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |\n| Application slice | ${linkedElements(elements.application, elements.request)} | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |\n| Domain state | ${linkedElements(elements.domain, elements.entity)} | Holds persistent state and lifecycle invariants used by the feature. |\n| Infrastructure and shared ports | ${linkedElements(elements.infrastructure, { name: "IAppDbContext", path: "backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs" })} | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |\n\n### Functions and data flow\n\n- \`${elements.page.name}.${elements.readOnly ? "load" : "submit"}()\` starts the browser interaction and maintains presentation state.\n- \`${elements.apiClient.name}.${elements.readOnly ? "get" : "execute"}()\` crosses the frontend-to-backend boundary with a typed HTTP request.\n- \`${elements.controller.name}.${elements.readOnly ? "GetAsync" : "PostAsync"}()\` applies transport policy and constructs \`${elements.request.name}\`.\n- \`${elements.handler.name}.Handle()\` loads authorized state through \`IAppDbContext\`, applies feature rules through \`${elements.entity.name}\`, and returns a typed result.\n- \`IAppDbContext.SaveChangesAsync()\` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.\n\n### Behavior\n\n1. **Entry.** ${entryText(slice)}\n2. **Processing.** ${processing}\n3. **Result.** ${resultText(slice, elements.readOnly)}\n\n## Requirements\n\nThis section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses \`shall\` in accordance with the architecture-description voice.\n\n${renderRequirements(slice)}\n\n## Diagrams\n\nThe six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.\n\n### C4 diagrams\n\n#### System context\n\nFigure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Word Up software system for this feature.\n\n![Figure 1 — C4 system context for ${slice.title}.](diagrams/c4-system-context.png)\n\n*Figure 1 — C4 system context: ${slice.title} in the Word Up environment.*\n\n#### Container\n\nFigure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.\n\n![Figure 2 — C4 container view for ${slice.title}.](diagrams/c4-container.png)\n\n*Figure 2 — C4 container view: application and data boundaries for ${slice.title}.*\n\n#### Component\n\nFigure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.\n\n![Figure 3 — C4 component view for ${slice.title}.](diagrams/c4-component.png)\n\n*Figure 3 — C4 component view: collaborating components for ${slice.title}.*\n\n#### Code\n\nFigure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.\n\n![Figure 4 — C4 code view for ${slice.title}.](diagrams/c4-code.png)\n\n*Figure 4 — C4 code view: feature-owned code elements for ${slice.title}.*\n\n### Class diagram\n\nFigure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.\n\n![Figure 5 — Class structure for ${slice.title}.](diagrams/class-structure.png)\n\n*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for ${slice.title}.*\n\n### Sequence diagram\n\nFigure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.\n\n![Figure 6 — Sequence behavior for ${slice.title}.](diagrams/sequence-behaviour.png)\n\n*Figure 6 — Sequence behavior: the end-to-end ${slice.title.toLowerCase()} interaction.*\n`;
}

const resolvedRoot = fs.realpathSync(detailedRoot);
const expectedRoot = path.join(fs.realpathSync(repoRoot), "docs", "detailed-designs");
if (resolvedRoot.toLowerCase() !== expectedRoot.toLowerCase()) {
  throw new Error(`Refusing to replace unexpected detailed-design root: ${resolvedRoot}`);
}

const existingDirectories = fs.readdirSync(resolvedRoot, { withFileTypes: true })
  .filter((entry) => entry.isDirectory());
for (const entry of existingDirectories) {
  const target = path.resolve(resolvedRoot, entry.name);
  if (path.dirname(target).toLowerCase() !== resolvedRoot.toLowerCase()) {
    throw new Error(`Refusing to remove directory outside the detailed-design root: ${target}`);
  }
}
for (const entry of existingDirectories) {
  fs.rmSync(path.join(resolvedRoot, entry.name), { recursive: true, force: false });
}

let readmeCount = 0;
let pumlCount = 0;
for (const slice of manifest.slices) {
  if (!subsystemTypes[slice.subsystem]) throw new Error(`Unknown subsystem ${slice.subsystem}.`);
  const featureDirectory = path.join(detailedRoot, slice.subsystem, slice.slug);
  const diagramDirectory = path.join(featureDirectory, "diagrams");
  fs.mkdirSync(diagramDirectory, { recursive: true });
  const elements = designElements(slice);
  const readme = readmeFor(slice, elements).replace(
    `The ${subsystemNames[slice.subsystem]} subsystem owns the part of that software concerned with ${slice.title.toLowerCase()}.`,
    `The ${subsystemNames[slice.subsystem]} subsystem owns this feature slice.`,
  );
  fs.writeFileSync(path.join(featureDirectory, "README.md"), readme, "utf8");
  readmeCount += 1;
  const diagrams = {
    "c4-system-context.puml": systemContextDiagram(slice),
    "c4-container.puml": containerDiagram(slice),
    "c4-component.puml": componentDiagram(slice, elements),
    "c4-code.puml": codeDiagram(slice, elements),
    "class-structure.puml": classDiagram(slice, elements),
    "sequence-behaviour.puml": sequenceDiagram(slice, elements),
  };
  for (const [fileName, content] of Object.entries(diagrams)) {
    fs.writeFileSync(path.join(diagramDirectory, fileName), content, "utf8");
    pumlCount += 1;
  }
}

console.log(`subsystems=${new Set(manifest.slices.map((slice) => slice.subsystem)).size}`);
console.log(`feature_readmes=${readmeCount}`);
console.log(`plantuml_sources=${pumlCount}`);
