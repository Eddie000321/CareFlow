# CareFlow Implementation Audit - 2026-05-19

## Resume-Safe Positioning

CareFlow is a .NET 9/PostgreSQL backend API for a veterinary EMR slice. It is strongest for backend, .NET, EF Core, PostgreSQL, migrations, Swagger/OpenAPI, Serilog, and domain modeling roles.

Do not describe it as a full-stack product, production EMR, compliance-ready system, or mature RBAC/audit platform.

## Confirmed Implementation Scope

- ASP.NET Core controllers for owners, pets, clinical notes, lab reports, and lab results.
- EF Core Code First model with Fluent API configuration and PostgreSQL provider.
- Swagger/OpenAPI and Serilog request logging.
- Docker Compose support for API + PostgreSQL local execution.
- Development seed support, including a large seed path for owners, pets, clinical notes, lab reports, and lab results.

## Improvements Applied

- Extracted pet age formatting into `PetAgeCalculator` so API age labels are testable outside controllers.
- Added a `CreatePetRequest` boundary so pet creation accepts a verified `ownerId` instead of requiring clients to submit an EF navigation object.
- Added API/domain validation for migration-bounded `LabName`, `Status`, `Units`, and `Flag` fields so oversized requests fail as field-level 400 responses before PostgreSQL persistence.
- Added xUnit coverage for day/month/year-month age labels and future-date clamping.
- Changed owner/pet/clinical/lab parent relations from cascade to restrict in EF configuration to better protect clinical-history retention.
- Added route-level delete guards:
  - Owners with pets cannot be deleted.
  - Pets with clinical notes or lab reports cannot be deleted.
- Added controller-level xUnit coverage for not-found, blocked-delete, and successful-delete paths.
- Added production-model tests for the three restrictive clinical-history relationships and the report/result cascade.
- Added `AlignRuntimeModelForRetention` to synchronize the EF snapshot, foreign-key actions, query indexes, and bounded lab metadata; preflight checks reject over-length existing values with explicit errors.
- Added a no-database schema-drift contract and a `WebApplicationFactory` workflow covering owner/pet creation, retrieval, guarded deletion, and cleanup through HTTP.
- Limited automatic migration and sample seeding to the Development environment.
- Added .NET build/test/runtime ignore rules so new local output is not added to Git.
- Added an ephemeral PostgreSQL 16 CI service that applies the complete migration chain twice to verify clean-schema migration and repeat-run idempotency.

## Remaining Gaps

- No authentication or RBAC yet.
- No soft delete or audit trail yet.
- Migration `AlignRuntimeModelForRetention` now aligns restrictive delete behavior, query indexes, and bounded lab metadata with the runtime model. CI validates it against a clean PostgreSQL database, but representative existing data still needs pre-production validation.
- The HTTP workflow is covered through `WebApplicationFactory` with an isolated EF Core in-memory model, but no PostgreSQL-backed HTTP integration test exists yet.
- The migration's `Down` method intentionally restores the legacy cascade-delete actions. A downgrade therefore requires an explicit retention-impact review before execution.
- The pet-create contract change is not API-versioned, so existing clients need a coordinated rollout.
- Restrictive foreign keys preserve retained data if a dependency appears during a delete race, but the resulting database exception is not yet translated into a stable conflict response.
- Lab-report multi-navigation queries have not been profiled against realistic result cardinality or captured SQL plans.
- Large seed is available, but query/performance validation results should be recorded before claiming benchmark improvements.

## Follow-up Verification - 2026-07-09

- `dotnet build CareFlow.sln -c Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test CareFlow.sln -c Release --no-build`: 23 passed, 0 failed, 0 skipped.
- `dotnet list CareFlow.sln package --vulnerable --include-transitive`: no known vulnerable packages reported by the configured NuGet source.
- Draft PR #1 is pushed from `codex/public-evidence-2026-07-09`; as of 2026-07-09 its GitHub Actions push and pull-request checks complete successfully. The default `master` branch remains unchanged until the PR is reviewed and merged.
- The public repository still tracks 151 `bin/` and `obj/` files (about 36 MB in Git objects) plus `api-dotnet/server.log`. Ignore rules now prevent new additions, but existing tracked files still need a deliberate history-safe cleanup.
- The checked-in EF snapshot matches the runtime model, a schema-contract test guards against future drift, and CI applies the migrations to a clean ephemeral PostgreSQL schema. This is not evidence that the migration is safe for every existing production dataset.
- Repository-wide `dotnet format --verify-no-changes` still reports pre-existing whitespace issues in `CareFlowDb`, `DateDiffExpressions`, `Seed`, and several domain model files.
