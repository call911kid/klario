# Manual Deployment

Instructions for publishing and hosting Klario directly on a local server, VM, or self-hosted host without automated CI/CD.

## Prerequisites

- .NET 10 ASP.NET Core Hosting Bundle (or SDK)
- Microsoft SQL Server instance reachable from the host

## Publish Application

Generate the published binaries in Release configuration:

```powershell
dotnet publish src/Klario/Klario.PL/Klario.PL.csproj -c Release -o ./publish
```

## Configure Production Settings

Create `./publish/appsettings.json` (or copy `appsettings.example` into `./publish/`):

```powershell
Copy-Item appsettings.example ./publish/appsettings.json
```

Edit `./publish/appsettings.json` with production values:
- `ConnectionStrings:DefaultConnection`: Production SQL Server connection string
- `Authentication:ApiKey`: Secret API key for endpoint authorization
- `Console:Enabled`: `true` to serve the web dashboard, `false` to disable it

## Run Service

Run directly using the .NET runtime:

```powershell
cd ./publish
dotnet Klario.PL.dll
```

> Note: EF Core database migrations apply automatically on application startup.
