#!/usr/bin/env node
// Acceptance Test
// Traces to: L2-NFR-008 AC1/AC2/AC4
//
// Dependency-free fixed-arrival-rate API load gate. Fifty independently
// authenticated asynchronous workers maintain 20 requests/second by default;
// latency is measured through complete response-body consumption against a
// loopback server (network excluded).

import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { performance } from "node:perf_hooks";

if (process.argv.includes("--help")) {
  console.log(`Usage: node scripts/performance/api-load.mjs

Environment variables:
  PERF_BASE_URL            API origin (default http://127.0.0.1:5211)
  PERF_PATHS               Comma-separated read paths
  PERF_BEARER_TOKEN        Optional single token for authenticated reads
  PERF_BEARER_TOKENS       Optional comma-separated token pool (one identity per token)
  PERF_SEED_AUTH           Seed/sign in one Testing admin per virtual user (default true)
  PERF_SEED_COHORTS        Seed one consented Youth in a distinct cohort per virtual user (default false)
  PERF_REQUIRE_UNIQUE_USERS Require one authenticated identity per virtual user (default true with seeded auth)
  PERF_REQUIRE_COHORTS     Require cohort coverage for every virtual user (default false)
  PERF_VIRTUAL_USERS       Concurrent workers (default 50)
  PERF_REQUESTS_PER_SECOND Fixed arrival rate (default 20)
  PERF_SEED_CONCURRENCY    Maximum parallel identity setup requests (default 5)
  PERF_SETUP_TIMEOUT_MS    Timeout for each setup/sign-in request (default 120000)
  PERF_WARMUP_SECONDS      Unmeasured warmup (default 5)
  PERF_DURATION_SECONDS    Measured duration (default 30)
  PERF_P50_MS              Maximum median latency (default 300)
  PERF_P95_MS              Maximum p95 latency (default 800)
  PERF_MAX_ERROR_RATE      Exclusive maximum error fraction (default 0.01)
  PERF_MAX_RESPONSE_BYTES  Maximum response body size (default 262144)
  PERF_REPORT_PATH         Optional JSON report path`);
  process.exit(0);
}

function positiveNumber(name, fallback) {
  const value = Number(process.env[name] ?? fallback);
  if (!Number.isFinite(value) || value <= 0)
    throw new Error(`${name} must be a positive number`);
  return value;
}

function positiveInteger(name, fallback) {
  const value = positiveNumber(name, fallback);
  if (!Number.isInteger(value)) throw new Error(`${name} must be an integer`);
  return value;
}

const config = {
  baseUrl: new URL(process.env.PERF_BASE_URL ?? "http://127.0.0.1:5211"),
  paths: (
    process.env.PERF_PATHS ??
    "/api/public/content,/api/people?page=1&pageSize=25,/health/ready"
  )
    .split(",")
    .map((path) => path.trim())
    .filter(Boolean),
  virtualUsers: positiveInteger("PERF_VIRTUAL_USERS", 50),
  requestsPerSecond: positiveNumber("PERF_REQUESTS_PER_SECOND", 20),
  seedConcurrency: positiveInteger("PERF_SEED_CONCURRENCY", 5),
  setupTimeoutMs: positiveInteger("PERF_SETUP_TIMEOUT_MS", 120_000),
  warmupSeconds: positiveNumber("PERF_WARMUP_SECONDS", 5),
  durationSeconds: positiveNumber("PERF_DURATION_SECONDS", 30),
  requestTimeoutMs: positiveInteger("PERF_REQUEST_TIMEOUT_MS", 5_000),
  p50LimitMs: positiveNumber("PERF_P50_MS", 300),
  p95LimitMs: positiveNumber("PERF_P95_MS", 800),
  maxErrorRate: positiveNumber("PERF_MAX_ERROR_RATE", 0.01),
  maxResponseBytes: positiveInteger("PERF_MAX_RESPONSE_BYTES", 262_144),
  reportPath: process.env.PERF_REPORT_PATH,
  seedAuth: process.env.PERF_SEED_AUTH !== "false",
  seedCohorts: process.env.PERF_SEED_COHORTS === "true",
  requireUniqueUsers:
    process.env.PERF_REQUIRE_UNIQUE_USERS === "true" ||
    (process.env.PERF_REQUIRE_UNIQUE_USERS !== "false" &&
      process.env.PERF_SEED_AUTH !== "false"),
  requireCohorts: process.env.PERF_REQUIRE_COHORTS === "true",
};

if (config.paths.length === 0)
  throw new Error("PERF_PATHS must contain at least one path");

const sleep = (milliseconds) =>
  milliseconds > 0
    ? new Promise((resolvePromise) => setTimeout(resolvePromise, milliseconds))
    : undefined;

async function seedAuthorization(workerId) {
  const password = "PerformanceOnly123!";
  let credentials = {
    email: `performance-${workerId}-${crypto.randomUUID()}@wordup.test`,
    password,
  };
  let cohortId = null;
  const seed = config.seedCohorts
    ? await fetch(new URL("/api/_test/youth-curriculum", config.baseUrl), {
        method: "POST",
        signal: AbortSignal.timeout(config.setupTimeoutMs),
      })
    : await fetch(new URL("/api/_test/users", config.baseUrl), {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({
          email: credentials.email,
          password,
          displayName: `Performance test administrator ${workerId + 1}`,
          role: "ProgramAdministrator",
        }),
        signal: AbortSignal.timeout(config.setupTimeoutMs),
      });
  if (!seed.ok) {
    throw new Error(
      `Could not seed performance identity ${workerId + 1} (HTTP ${seed.status}). ` +
        "Run the API in Testing, set PERF_BEARER_TOKEN, or set PERF_SEED_AUTH=false with anonymous PERF_PATHS.",
    );
  }
  if (config.seedCohorts) {
    const cohort = await seed.json();
    credentials = { email: cohort.youthEmail, password: cohort.youthPassword };
    cohortId = cohort.cohortId;
  }

  const signIn = await fetch(new URL("/api/auth/sign-in", config.baseUrl), {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(credentials),
    signal: AbortSignal.timeout(config.setupTimeoutMs),
  });
  if (!signIn.ok)
    throw new Error(
      `Could not sign in performance identity ${workerId + 1} (HTTP ${signIn.status})`,
    );
  const auth = await signIn.json();
  if (typeof auth.accessToken !== "string")
    throw new Error("Sign-in response did not include accessToken");
  return { authorization: `Bearer ${auth.accessToken}`, cohortId };
}

async function authorizationHeaders() {
  const configuredTokens = (
    process.env.PERF_BEARER_TOKENS ??
    process.env.PERF_BEARER_TOKEN ??
    ""
  )
    .split(",")
    .map((token) => token.trim())
    .filter(Boolean);
  if (configuredTokens.length > 0) {
    return configuredTokens.map((token) => ({
      authorization: `Bearer ${token}`,
      cohortId: null,
    }));
  }
  if (!config.seedAuth) return [];

  const identities = new Array(config.virtualUsers);
  let nextWorkerId = 0;

  async function seedWorker() {
    while (nextWorkerId < config.virtualUsers) {
      const workerId = nextWorkerId;
      nextWorkerId += 1;
      identities[workerId] = await seedAuthorization(workerId);
    }
  }

  await Promise.all(
    Array.from(
      { length: Math.min(config.seedConcurrency, config.virtualUsers) },
      () => seedWorker(),
    ),
  );
  return identities;
}

const authorizations = await authorizationHeaders();
const cohortCount = new Set(
  authorizations.map((identity) => identity.cohortId).filter(Boolean),
).size;
if (config.requireUniqueUsers && authorizations.length < config.virtualUsers) {
  throw new Error(
    `Performance gate requires ${config.virtualUsers} unique authenticated users, ` +
      `but only ${authorizations.length} token(s) were provided`,
  );
}
if (config.requireCohorts && cohortCount < config.virtualUsers) {
  throw new Error(
    `Performance gate requires ${config.virtualUsers} cohort-scoped users, ` +
      `but only ${cohortCount} distinct cohort(s) were seeded`,
  );
}

async function runPhase(durationSeconds, recordMetrics) {
  const requestCount = Math.ceil(durationSeconds * config.requestsPerSecond);
  const startedAt = performance.now();
  let nextRequest = 0;
  const latencies = [];
  const failures = [];
  let completed = 0;

  async function worker(workerId) {
    const identity = authorizations.length
      ? authorizations[workerId % authorizations.length]
      : undefined;
    while (true) {
      const requestIndex = nextRequest++;
      if (requestIndex >= requestCount) return;

      const scheduledAt =
        startedAt + (requestIndex * 1_000) / config.requestsPerSecond;
      await sleep(scheduledAt - performance.now());

      const path =
        config.paths[(requestIndex + workerId) % config.paths.length];
      const requestStartedAt = performance.now();
      try {
        const response = await fetch(new URL(path, config.baseUrl), {
          headers: {
            accept: "application/json",
            ...(identity ? { authorization: identity.authorization } : {}),
          },
          signal: AbortSignal.timeout(config.requestTimeoutMs),
        });
        const body = await response.arrayBuffer();
        const latency = performance.now() - requestStartedAt;

        if (recordMetrics) latencies.push(latency);
        if (!response.ok || body.byteLength > config.maxResponseBytes) {
          failures.push({
            path,
            status: response.status,
            bytes: body.byteLength,
            reason: !response.ok
              ? `HTTP ${response.status}`
              : "response exceeded byte budget",
          });
        }
      } catch (error) {
        if (recordMetrics) latencies.push(performance.now() - requestStartedAt);
        failures.push({
          path,
          reason: error instanceof Error ? error.message : String(error),
        });
      } finally {
        completed += 1;
      }
    }
  }

  await Promise.all(
    Array.from({ length: config.virtualUsers }, (_, workerId) =>
      worker(workerId),
    ),
  );
  return {
    requestCount,
    completed,
    elapsedSeconds: (performance.now() - startedAt) / 1_000,
    latencies,
    failures,
  };
}

function percentile(values, quantile) {
  if (values.length === 0) return Number.POSITIVE_INFINITY;
  const sorted = [...values].sort((left, right) => left - right);
  const position = (sorted.length - 1) * quantile;
  const lower = Math.floor(position);
  const upper = Math.ceil(position);
  if (lower === upper) return sorted[lower];
  return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
}

console.log(
  `Performance gate: ${config.virtualUsers} virtual users, ${config.requestsPerSecond} RPS, ` +
    `${config.durationSeconds}s, ${config.paths.join(", ")}`,
);

await runPhase(config.warmupSeconds, false);
const result = await runPhase(config.durationSeconds, true);

const report = {
  config: {
    ...config,
    baseUrl: config.baseUrl.toString(),
    reportPath: config.reportPath ?? null,
    authenticatedUsers: authorizations.length,
    representedCohorts: cohortCount,
  },
  metrics: {
    requests: result.completed,
    elapsedSeconds: result.elapsedSeconds,
    achievedRequestsPerSecond: result.completed / result.elapsedSeconds,
    p50Ms: percentile(result.latencies, 0.5),
    p95Ms: percentile(result.latencies, 0.95),
    errorRate: result.failures.length / result.completed,
    errors: result.failures.length,
  },
  sampleFailures: result.failures.slice(0, 10),
};

const failures = [];
if (report.metrics.p50Ms > config.p50LimitMs)
  failures.push(
    `p50 ${report.metrics.p50Ms.toFixed(1)}ms > ${config.p50LimitMs}ms`,
  );
if (report.metrics.p95Ms > config.p95LimitMs)
  failures.push(
    `p95 ${report.metrics.p95Ms.toFixed(1)}ms > ${config.p95LimitMs}ms`,
  );
if (report.metrics.errorRate >= config.maxErrorRate)
  failures.push(
    `error rate ${(report.metrics.errorRate * 100).toFixed(2)}% >= ${config.maxErrorRate * 100}%`,
  );
if (report.metrics.achievedRequestsPerSecond < config.requestsPerSecond * 0.95)
  failures.push(
    `throughput ${report.metrics.achievedRequestsPerSecond.toFixed(2)} RPS < 95% of target`,
  );

console.table({
  requests: report.metrics.requests,
  rps: report.metrics.achievedRequestsPerSecond.toFixed(2),
  p50_ms: report.metrics.p50Ms.toFixed(1),
  p95_ms: report.metrics.p95Ms.toFixed(1),
  errors: report.metrics.errors,
  error_rate: `${(report.metrics.errorRate * 100).toFixed(2)}%`,
});

if (config.reportPath) {
  const outputPath = resolve(config.reportPath);
  mkdirSync(dirname(outputPath), { recursive: true });
  writeFileSync(outputPath, `${JSON.stringify(report, null, 2)}\n`);
}

if (failures.length > 0) {
  console.error(`PERFORMANCE GATE: FAIL\n- ${failures.join("\n- ")}`);
  if (report.sampleFailures.length > 0)
    console.error(
      `Sample request failures:\n${JSON.stringify(report.sampleFailures, null, 2)}`,
    );
  process.exitCode = 1;
} else {
  console.log("PERFORMANCE GATE: PASS");
}
