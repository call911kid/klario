# Deployment via GitHub Actions (MonsterASP)

Automated deployment is configured using `.github/workflows/deploy.yml` on push to the `main` branch.

## Hosting Overview

MonsterASP provides free ASP.NET Core website hosting and a free Microsoft SQL Server database suitable for running Klario.

## Required GitHub Secrets

Configure the following repository secrets under **Settings > Secrets and variables > Actions**:

| Secret | Description | Source in MonsterASP |
|---|---|---|
| `FTP_SERVER` | FTP server host address | Web hosting panel > FTP Accounts |
| `FTP_USERNAME` | FTP account username | Web hosting panel > FTP Accounts |
| `FTP_PASSWORD` | FTP account password | Web hosting panel > FTP Accounts |
| `CONNECTION_STRING` | Remote SQL Server connection string | Database panel > Connection details |
| `API_KEY` | Secret key expected in `X-Api-Key` header | User-defined custom secret |
| `APP_URL` | Optional. Deployed site URL for health check | Web hosting panel > Website domain |
| `CONSOLE_ENABLED` | Optional. `true` or `false` (default: `true`) | User-defined flag |

## Workflow Execution

1. Build & publish artifacts with Release configuration targeting .NET 10.
2. Generate production `appsettings.json` from GitHub secrets.
3. Place `app_offline.htm` via FTP to gracefully halt running instances.
4. Upload application payload via FTP to `/wwwroot/`.
5. Remove `app_offline.htm` to bring the service back online.
6. Verify deployment by probing the `/api/health` endpoint.
