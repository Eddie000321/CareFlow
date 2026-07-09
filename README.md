# CareFlow - Animal Hospital EMR System

## Project Overview
CareFlow is the backend API for an **Animal Hospital Electronic Medical Record (EMR)** system.
It efficiently stores and manages medical data such as patient (pet) information, owner information, clinical notes, test results, and lab reports, providing them via a REST API.

---

## Project Goals
1.  **Centralized Medical Data Management**
    -   Safely and systematically store owner, pet, test, and medical record data.
    -   Design a stable data storage structure based on PostgreSQL.

2.  **Scalable and Maintainable Architecture**
    -   Modular structure based on ASP.NET Core + Entity Framework Core.
    -   Clear entity mapping using the Fluent API.

3.  **Developer-Friendly API**
    -   Implementation of CRUD-focused REST APIs.
    -   Automated API documentation through Swagger UI.

4.  **Automated Development Environment**
    -   Automatic schema updates via EF Core Migrations.
    -   Support for running PostgreSQL + API containers with Docker Compose.

---

## Tech Stack
-   **Backend Framework:** ASP.NET Core (.NET 9)
-   **Database:** PostgreSQL
-   **ORM:** Entity Framework Core (with Fluent API mapping)
-   **API Docs:** Swagger / OpenAPI
-   **Container:** Docker, Docker Compose

---

## Project Structure

```
api-dotnet/
│── Controllers/         # Defines API endpoints
│    ├── OwnersController.cs
│    ├── PetsController.cs
│    ├── LabReportsController.cs
│    ├── LabResultsController.cs
│    ├── ClinicalNotesController.cs
│
│── Data/
│    ├── Configuration/  # Entity mapping configurations (Fluent API)
│    ├── CareFlowDb.cs   # DbContext (Manages DB connection)
│    ├── Seed.cs         # Initial data seeding
│    ├── expressions/    # Expressions for data comparison/analysis
│
│── Domain/              # Entity classes (Mapped to DB tables)
│    ├── Pet.cs
│    ├── Owner.cs
│    ├── LabReport.cs
│    ├── LabResult.cs
│    ├── ClinicalNote.cs
│
│── appsettings.json     # Default settings (including DB connection string)
│── appsettings.Development.json # Development-specific settings
│── docker-compose.yml   # Configuration for PostgreSQL + API containers
│── Dockerfile           # Container build instructions for the API
│── Program.cs           # App entry point and service registration
```

---

## Key Features
1.  **Pet Management**
    -   Create / Read / Update / Delete APIs for pets.

2.  **Owner Management**
    -   Create / Read / Update / Delete APIs for owners.

3.  **Lab Report Management**
    -   Create / Read / Update / Delete APIs for lab reports.

4.  **Clinical Note Management**
    -   API for creating and viewing medical records.

5.  **Data Comparison/Change Detection**
    -   Provides Expressions to extract only modified data (`DateDiffExpressions.cs`).

6.  **Structured Logging**
    -   Configured Serilog pipeline with console sink for development visibility.

7.  **Swagger-based API Documentation**
    -   API testing available at the `/swagger` path.

8.  **Automatic DB Migration (in Development mode)**
    -   Executes `db.Database.Migrate()` in `Program.cs`.

9.  **Retention-Oriented Delete Guards**
    -   Owner and pet deletes are blocked when dependent pets, clinical notes, or lab reports exist.
    -   EF Core relationship configuration uses restrictive delete behavior for retained clinical history.

10. **Testable Age Labels**
    -   Pet age-label calculation is extracted into a reusable domain helper with xUnit coverage.

---

## How to Run (Development Environment)
1.  **Clone the repository**
    ```bash
    git clone https://github.com/Eddie000321/CareFlow.git
    cd CareFlow/api-dotnet
    ```

2.  **Run with Docker**
    ```bash
    docker-compose up --build
    ```

3.  **(Optional) Apply migrations manually**
    ```bash
    dotnet ef database update
    ```

4.  **Access Swagger UI**
    -   Local run (HTTPS profile): `https://localhost:7117/swagger`
    -   Docker Compose: `http://localhost:5000/swagger`

---

## Development Guidelines
1.  **Changing the DB Schema**
    -   Modify entity mappings in `Data/Configuration/`.
    -   Create a migration and update the database.
    ```bash
    dotnet ef migrations add MigrationName
    dotnet ef database update
    ```

2.  **Adding a New Endpoint**
    -   Create an entity class in `Domain/`.
    -   Add a Fluent API configuration in `Data/Configuration/`.
    -   Create a controller in `Controllers/`.
    -   If necessary, add dummy data in `Seed.cs`.

3.  **Deployment Considerations**
    -   Keep automatic migration and sample seeding limited to development mode.
    -   Apply reviewed migrations as a separate production release step.
    -   Manage DB connection strings with environment variables.
    -   Add authentication/authorization logic and enforce HTTPS.

## Build and Test

From the repository root:

```bash
dotnet restore CareFlow.sln
dotnet build CareFlow.sln -c Release --no-restore
dotnet test CareFlow.sln -c Release --no-build
```

The same restore/build/test sequence runs for pushes and pull requests through
`.github/workflows/dotnet.yml`.

The xUnit suite covers pet age-label boundaries, owner/pet retention guards,
runtime-to-migration schema consistency, and an in-memory HTTP workflow through
the real ASP.NET Core middleware and controller pipeline. A PostgreSQL-backed
integration suite remains future work.

## Case Study: Retention-Safe Schema Evolution

- **Problem:** Controller guards protected clinical history, but the checked-in EF snapshot still described cascading deletes and the pet-create endpoint exposed an EF navigation property as part of its request contract.
- **Decision:** Add a reviewed migration that aligns foreign keys, query indexes, and bounded lab metadata with the runtime model, plus a focused `CreatePetRequest` DTO that accepts `ownerId` without requiring a nested owner entity. Length preflight checks stop the migration instead of silently trimming existing data.
- **Verification:** A schema-contract test fails when the runtime model and latest snapshot diverge, while a `WebApplicationFactory` test exercises owner and pet creation, pet retrieval, guarded owner deletion, and ordered cleanup across the HTTP pipeline without a live production database.
- **Limits:** The HTTP test uses EF's in-memory provider, the migration has not been applied to an existing PostgreSQL dataset, and authentication, soft delete, and audit history are not implemented.
- **Learning:** Retention rules are stronger when API behavior, database constraints, migration artifacts, and automated evidence all express the same policy.

---

## Future Plans
-   User authentication and role-based access control.
-   Soft delete and audit trail for retained medical records.
-   PostgreSQL-backed HTTP integration tests for main API workflows.
-   Real-time clinical note synchronization (SignalR).
-   Cloud deployment (Azure or AWS).
