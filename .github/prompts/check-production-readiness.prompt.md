---
description: "Analyze project configuration for production readiness including database, telemetry, and security settings"
agent: "agent"
tools: [read, search]
---

Analyze the project configuration for production readiness:

1. **Database**: Connection strings, pooling, timeouts, retry policies
2. **Telemetry**: OpenTelemetry exporters, sampling rates
3. **Authentication**: JWT key strength, token expiration
4. **Rate Limiting**: Strategy and limits configured
5. **Health Checks**: Endpoints and dependencies monitored
6. **Logging**: Structured logging, log levels, sensitive data filtering
7. **Security Headers**: HSTS, CSP, CORS configuration

Compare `appsettings.json` vs `appsettings.Production.json` and flag missing production overrides.
