# Deployment Guide

## Database Migrations

The application uses EF Core migrations for database schema management.

### Creating Migrations

```bash
cd src/life-games-api
dotnet ef migrations add MigrationName   --project Infrastructure/LifeGames.Infrastructure   --startup-project Api/LifeGames.Api   --context LifeGamesDbContext
```

### Applying Migrations

**Automatic** (on application startup):
- Application applies pending migrations automatically
- Production requires migrations (throws if missing)
- Development falls back to EnsureCreated() without migrations

**Manual** (recommended for production):
```bash
dotnet ef database update   --project Infrastructure/LifeGames.Infrastructure   --startup-project Api/LifeGames.Api
```

## Production Configuration

See:
- `docs/SECRETS.md` - Secrets management
- `docs/LOGGING.md` - CloudWatch logging
- `appsettings.Production.json` - Production settings

## Health Checks

- `GET /health` - Basic health check
- `GET /health/ready` - Readiness check (includes DB connectivity)

## Security Checklist

- [ ] HTTPS enforced
- [ ] CORS restricted to specific origins
- [ ] Secrets in Parameter Store
- [ ] Security headers enabled
- [ ] Rate limiting configured
- [ ] Forwarded headers for proxies
- [ ] CloudWatch logging enabled

---

For detailed deployment scenarios, see README.md and PROJECT_SUMMARY.md.
