Run `dotnet ef migrations add InitialCreate` after restoring packages to generate the SQL Server migration for the configured `ApplicationDbContext`.

The backend is migration-ready: Identity tables and Carevionix domain tables are mapped through Entity Framework Core.

Fresh databases also receive the demo admin, doctor, patient, role, and availability seed data from `20260531143000_SeedDemoAccounts` when `Update-Database` is run.
