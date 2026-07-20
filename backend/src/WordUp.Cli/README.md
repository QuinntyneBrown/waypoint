# Word Up operational CLI

Install the packed .NET tool and run `wu --help` to inspect the complete command surface.

Production commands use the same configuration keys and application handlers as the API. Set these
through the host environment or another standard .NET configuration provider; never commit their
values:

- `DOTNET_ENVIRONMENT=Production`
- `ConnectionStrings__Default` — durable SQL Server connection string
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SigningKey`, and `Jwt__AccessTokenMinutes`
- `AuditIntegrity__SigningKey`
- `SensitiveData__EncryptionKey`

All three signing/encryption keys must contain at least 32 characters and must not use the checked-in
placeholder prefix. Run `wu db migrate` before normal operations, then bootstrap the first operator with
`wu db seed --email <email> --password <password> --display-name <name>`. Normal commands select the
oldest active Program Administrator for audit attribution; pass `--actor <user-id>` to choose one
deterministically. `wu db reset` is destructive and requires the exact option `--confirm RESET`.

The command groups cover database lifecycle, users, invitations, cohorts, youth CSV import, youth and
cohort report exports, manual badge awards, and audited access-token issuance. Use `wu <group> <command>
--help` for typed arguments and options.
