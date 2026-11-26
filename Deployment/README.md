# Run NTChat on Windows Server 2008 R2 SP1

Build on the Visual Studio development PC. The server runs the published application. The runtime target is the x64 full installation of Windows Server 2008 R2 SP1, IIS 7.5 in Integrated mode, .NET Framework 4.8 and SQL Server 2008 R2 Express.

## Development

Open ProjectSunrise.slnx, restore NuGet packages and start ProjectSunrise in IIS Express. Web.config uses Windows authentication to connect to a LocalDB database named ProjectSunriseDev. EF creates a new development database and seeds the roles on first database access. An existing database with a different schema requires a migration.

## Database on the server

1. Install the Database Engine for SQL Server 2008 R2 Express. The Release configuration assumes a local instance named SQLEXPRESS. Create an empty database named ProjectSunrise.
2. In SQL Server Management Studio, select that database and run Initialize.sql from this folder. The script installs the Identity and chat tables, indexes, foreign keys and roles in one transaction. It refuses to run in a populated or system database.
3. Create the IIS application pool named ProjectSunrise using ApplicationPoolIdentity. For a local SQL instance and this pool name, run the following as a database administrator.

```sql
USE master;
CREATE LOGIN [IIS APPPOOL\ProjectSunrise] FROM WINDOWS;
GO
USE ProjectSunrise;
CREATE USER [IIS APPPOOL\ProjectSunrise] FOR LOGIN [IIS APPPOOL\ProjectSunrise];
EXEC sp_addrolemember N'db_datareader', N'IIS APPPOOL\ProjectSunrise';
EXEC sp_addrolemember N'db_datawriter', N'IIS APPPOOL\ProjectSunrise';
GO
```

If using a different pool, database or instance name, adjust the login and published DefaultConnection accordingly. Release publishing disables automatic schema creation.

## Publish to IIS

1. Enable the IIS ASP.NET, .NET Extensibility, ISAPI Extensions, ISAPI Filters and Static Content role services. Install .NET Framework 4.8 and confirm ASP.NET 4 is registered with IIS. Set the pool to **.NET CLR v4.0** and **Integrated**.
2. Use the **PublishedSite** folder in the distribution ZIP, or publish the web project to a folder using **Release** in Visual Studio. Copy the contents of the published folder to the server as an IIS application, including Web.config, Views and bin with its Roslyn compiler files. Give the pool read and execute access to the site. Uploaded pictures live in the database.
3. Bind the application to the intended site, enable Anonymous Authentication in IIS so ASP.NET Identity can handle sign-in, and configure the intended HTTPS binding before using real accounts. Windows Server 2008 R2 and SQL Server 2008 R2 have reached end of support.
4. Register an account. To bootstrap staff administration, substitute the registered username below. Sign out and sign in to refresh its role claims.

```sql
USE ProjectSunrise;
INSERT INTO dbo.AspNetUserRoles(UserId, RoleId)
SELECT u.Id, r.Id
FROM dbo.AspNetUsers AS u
CROSS JOIN dbo.AspNetRoles AS r
WHERE u.UserName = N'REPLACE_WITH_YOUR_USERNAME'
  AND r.Name IN (N'Staff', N'Administrators')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.AspNetUserRoles AS ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
  );
```

## Acceptance check

Use two ordinary accounts and the staff account. Create a room, invite the second account, join it, exchange messages, toggle reactions, edit an authored message and follow its notification. Switch the room to private and verify an unrelated account cannot access its message API. Start a direct conversation and verify only its participants can open it. Try a staff broadcast, avatar upload and a page refresh with JavaScript disabled.

Room refresh uses a small polling request every ten seconds in capable browsers. Manual refresh and ordinary HTML forms work independently. Staff broadcasts and message notifications are saved with the originating change. This installation starts with an empty database. The Django source archive contains no production database dump.

[Microsoft .NET Framework system requirements](https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements) Â· [Windows Server lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2008-r2) Â· [SQL Server lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-sql-server-2008-r2)
