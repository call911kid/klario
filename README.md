# Klario

Klario is a self-hosted job monitoring platform designed to run on your own server. It continuously tracks openings across multiple hiring sources, filters postings based on your exact career preferences, and delivers real-time alerts the moment matching opportunities are published. It began as a learning project and evolved into a personal monitoring service.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft SQL Server (local instance, LocalDB, Docker, or hosted)

## Local Setup

### 1. Configure Settings

Copy `appsettings.example` to `src/Klario/Klario.PL/appsettings.json`:

```powershell
Copy-Item appsettings.example src/Klario/Klario.PL/appsettings.json
```

Update `DefaultConnection` in `appsettings.json`:
- LocalDB: `Server=(localdb)\\mssqllocaldb;Database=KlarioDb;Trusted_Connection=True;MultipleActiveResultSets=true;`
- Local instance: `Server=localhost;Database=KlarioDb;Trusted_Connection=True;TrustServerCertificate=True;`

Set `Authentication:ApiKey` to your preferred key.

> Note: Database migrations apply automatically on application startup.

### 2. Run Application

```powershell
dotnet run --project src/Klario/Klario.PL/Klario.PL.csproj
```

Available endpoints:
- Swagger: `https://localhost:7220/swagger`
- Hangfire Dashboard: `https://localhost:7220/hangfire`
- Management Console: `https://localhost:7220/console`
- Health Check: `https://localhost:7220/api/health`

## Deployment

Detailed deployment guides are available in `docs/deployment/`:

- [GitHub Actions (MonsterASP)](docs/deployment/github-actions-monsterasp.md) - Automated CI/CD deployment and required GitHub secrets.
- [Manual Deployment](docs/deployment/manual-deployment.md) - Publish and run directly on a local or remote server.
