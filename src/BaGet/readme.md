# BaGet

This is the project that implements [NuGet service APIs](https://docs.microsoft.com/en-us/nuget/api/overview). Most of the core logic is contained within the `BaGet.Core` project.

## Migrations

Add a migration with:

```
dotnet ef migrations add InitialCreate --project ../BaGet.Database.Mariadb -- --Database:Type Mariadb
dotnet ef migrations add InitialCreate --project ../BaGet.Database.Sqlite -- --Database:Type Sqlite
dotnet ef migrations add InitialCreate --project ../BaGet.Database.SqlServer -- --Database:Type SqlServer
dotnet ef migrations add InitialCreate --project ../BaGet.Database.PostgreSql -- --Database:Type PostgreSQL
dotnet ef database update
```
