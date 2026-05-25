AccountingProject - Ubuntu Deployment

Contents:
- backend/ : ASP.NET Core publish output
- db/create-db.sql : SQL Server schema script

Requirements:
- Ubuntu Server
- ASP.NET Core Runtime 8.x
- Nginx
- SQL Server reachable from the app server

Run DB script:
Run db/create-db.sql on the target SQL Server.

Backend run example:
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://127.0.0.1:5000 dotnet AccountingProject.dll

Important:
- Set real ConnectionStrings__DefaultConnection
- Set real Jwt__Key
- Set real AllowedOrigins
- Do not expose SQL Server port 1433 to the internet.