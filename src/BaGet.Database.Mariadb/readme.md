# BaGet's MariaDB Database Provider

This project contains BaGet's MariaDB database provider.

## Migrations

Add a migration with:

```
dotnet ef migrations add MigrationName --context MariadbContext --output-dir Migrations --startup-project ..\BaGet\BaGet.csproj

dotnet ef database update --context MariadbContext --startup-project ..\BaGet\BaGet.csproj
```
