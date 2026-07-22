# StudyMgt – Study Centre Management System

A Blazor Server application (.NET 10) with a PostgreSQL database backend.

## Onboarding Access Overview

- Student onboarding is an administrator-managed intake flow.
- The `/student-onboarding` route requires an authenticated centre administrator session.
- Public users should use `/centre-administrators` to access student intake through the secure admin dashboard.
- Tutor and carer onboarding remain separate onboarding flows.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (running locally or remotely)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

Install the EF CLI tool if not already installed:

```bash
dotnet tool install --global dotnet-ef
```

---

## Configuration

Update the connection string in `appsettings.Development.json` (for local development) to match your PostgreSQL instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=StudyMgtDb01;Username=postgres;Password=YourPassword"
  }
}
```

---

## Backend – Database Setup

1. **Apply migrations** to create/update the database schema:

   ```bash
   cd StudyMgt
   dotnet ef database update
   ```

2. **Add a new migration** (after model changes):

   ```bash
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   ```

---

## Running the Application

The project is a Blazor Server app — the backend and frontend run together in a single process.

1. **Restore packages:**

   ```bash
   dotnet restore
   ```

2. **Build:**

   ```bash
   dotnet build
   ```

3. **Run:**

   ```bash
   dotnet run --project StudyMgt.csproj
   ```

   The app will start and listen on:
   - HTTP:  `http://localhost:5058`
   - HTTPS: `https://localhost:7247`

   Open your browser at `http://localhost:5058`.

---

## Running the Tests

Integration tests run against a temporary PostgreSQL database. Ensure your local PostgreSQL instance is running before executing tests.

```bash
cd tests/StudyMgt.IntegrationTests
dotnet test
```

---

## Project Structure

```
StudyMgt/
├── Components/         # Blazor pages and layouts (frontend)
├── Data/
│   ├── Entities/       # EF Core entity models
│   └── StudyMgtDbContext.cs
├── Migrations/         # EF Core migration files
├── Services/           # Business logic services
├── wwwroot/            # Static assets (CSS, JS, images)
├── appsettings.json            # Production config
├── appsettings.Development.json # Local dev config
└── Program.cs          # App startup and DI configuration
```
