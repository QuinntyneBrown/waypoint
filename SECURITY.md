# Security Policy

## Supported versions

Waypoint is under active development. Security fixes are applied to the latest commit on the `main` branch; older commits, forks, and deployments are not supported. No release is currently approved for production use or for storing real participant information.

## Reporting a vulnerability

Do not disclose a suspected vulnerability in a public issue, discussion, pull request, or social channel.

Use [GitHub private vulnerability reporting](https://github.com/QuinntyneBrown/waypoint/security/advisories/new) to send the maintainers:

- A description of the vulnerability and its potential impact
- The affected endpoint, handler, dependency, or configuration
- Reproduction steps or a minimal proof of concept
- Any suggested mitigation, if known

Do not include real participant information, credentials, signing keys, connection strings, consent records, or unredacted logs in a report. Use synthetic data and redact sensitive evidence.

Maintainers will acknowledge a report as soon as practical, investigate it, and coordinate remediation and disclosure with the reporter. Response times are best effort because this is a volunteer-maintained project.

## Security and safeguarding considerations

Waypoint is an early-stage project and has not completed an independent production security, privacy, accessibility, or safeguarding assessment. In particular:

- The development `Jwt:SigningKey` in `appsettings.json` is a placeholder and must be replaced with a strong secret supplied through user secrets or environment configuration before any non-local use.
- The default in-memory database is for local development only and provides no durable operational storage.
- The local CORS origin, Swagger UI, logging email sender, and development diagnostics must be reviewed and restricted for deployment.
- Authentication, authorization, invitation tokens, refresh tokens, password-reset tokens, audit logs, and guardian-consent records require threat modelling and operational controls beyond application code.
- Youth and guardian information is sensitive. Deployers must establish lawful collection, least-privilege access, retention, deletion, incident response, and safeguarding procedures before handling real data.

Deployers are responsible for reviewing secret management, dependency risk, transport security, data residency, encryption, backups, monitoring, audit retention, breach response, and applicable privacy and child-safeguarding obligations.
