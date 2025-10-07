# CareFlow API - .NET Backend

## Overview
This is the ASP.NET Core backend API for the CareFlow Animal Hospital EMR System. It provides REST endpoints for managing pet medical records, owner information, clinical notes, and lab reports.

## Tech Stack
- **Framework:** ASP.NET Core (.NET 9.0)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core with Fluent API
- **API Documentation:** Swagger/OpenAPI
- **Logging:** Serilog
- **Container:** Docker support

## Project Structure

```
api-dotnet/
├── Controllers/                    # API Controllers
│   ├── OwnersController.cs        # Owner management endpoints
│   ├── PetsController.cs          # Pet management endpoints
│   ├── LabReportsController.cs    # Lab report endpoints
│   ├── LabResultsController.cs    # Lab result endpoints
│   └── ClinicalNotesController.cs # Clinical note endpoints
│
├── Data/                          # Data layer
│   ├── CareFlowDb.cs             # DbContext
│   ├── Seed.cs                   # Database seeding
│   ├── Configuration/            # Entity configurations (Fluent API)
│   │   ├── ClinicalNoteConfig.cs
│   │   ├── LabReportConfig.cs
│   │   ├── LabResultConfig.cs
│   │   ├── OwnerConfig.cs
│   │   └── PetConfig.cs
│   └── expressions/              # Query expressions
│       └── DateDiffExpressions.cs
│
├── Domain/                       # Domain entities
│   ├── Pet.cs                   # Pet entity
│   ├── Owner.cs                 # Owner entity with address
│   ├── ClinicalNote.cs          # Clinical notes
│   ├── LabReport.cs             # Lab reports
│   ├── LabResult.cs             # Individual lab results
│   └── AgoLabel.cs              # Utility for date formatting
│
├── Program.cs                   # Application entry point
├── api-dotnet.csproj           # Project file with dependencies
├── appsettings.json            # Application configuration
├── appsettings.Development.json # Development settings
├── Dockerfile                  # API container definition
└── docker-compose.yml          # Docker composition
```

## Key Dependencies
- `Microsoft.AspNetCore.OpenApi` - OpenAPI support
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `Serilog.AspNetCore` - Structured logging
- `Swashbuckle.AspNetCore` - Swagger UI generation

## Database Schema

### Core Entities
- **Owner**: Pet owners with contact info and address
- **Pet**: Animals with breed, birth date, and calculated age
- **ClinicalNote**: Medical notes for pets
- **LabReport**: Lab test reports with metadata
- **LabResult**: Individual test results within reports

### Key Features
- Complex type mapping for Owner addresses
- Automatic age calculation for pets
- Cascading deletes for data integrity
- Indexed fields for performance

## API Endpoints

### Owners
- `GET /api/owners` - List owners
- `GET /api/owners/{id}` - Get owner with pets
- `POST /api/owners` - Create owner
- `PUT /api/owners/{id}` - Update owner
- `DELETE /api/owners/{id}` - Delete owner

### Pets
- `GET /api/pets` - List pets with calculated ages
- `GET /api/pets/{id}` - Get pet by ID
- `POST /api/pets` - Create pet
- `PUT /api/pets/{id}` - Update pet
- `DELETE /api/pets/{id}` - Delete pet

### Lab Reports
- `GET /api/labreports` - List lab reports
- `GET /api/labreports/{id}` - Get lab report summary
- `GET /api/labreports/{id}/with-results` - Get lab report with results
- `POST /api/labreports` - Create lab report
- `PUT /api/labreports/{id}` - Update lab report
- `DELETE /api/labreports/{id}` - Delete lab report

### Lab Results
- `GET /api/labresults` - List lab results with report metadata
- `GET /api/labresults/{id}` - Get lab result
- `GET /api/labresults/report/{reportId}` - List results for a report
- `GET /api/labresults/abnormal` - List abnormal results
- `POST /api/labresults` - Create lab result
- `PUT /api/labresults/{id}` - Update lab result
- `DELETE /api/labresults/{id}` - Delete lab result

### Clinical Notes
- `GET /api/clinicalnotes` - List clinical notes
- `GET /api/clinicalnotes/{id}` - Get clinical note
- `GET /api/clinicalnotes/pet/{petId}` - List clinical notes for a pet
- `POST /api/clinicalnotes` - Create clinical note
- `PUT /api/clinicalnotes/{id}` - Update clinical note
- `DELETE /api/clinicalnotes/{id}` - Delete clinical note

## Configuration

### Database Connection
Configure PostgreSQL connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5499;Database=careflow;Username=postgres;Password=postgres"
  }
}
```

### Logging
Serilog is configured for structured logging with console output.

## Running the Application

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL database
- Docker (optional)

### Local Development
1. **Clone and navigate:**
   ```bash
   cd api-dotnet
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Update database connection** in `appsettings.Development.json`

4. **Run migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Start the application:**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI:**
   - Local run: `https://localhost:7117/swagger` (HTTPS profile)
   - Docker Compose: `http://localhost:5000/swagger`

### Docker Compose
```bash
docker-compose up --build
```

This builds the API container, starts PostgreSQL, and exposes the API at `http://localhost:5000`.

## Development Workflow

### Adding New Entities
1. Create entity class in `Domain/`
2. Add configuration in `Data/Configuration/`
3. Create migration: `dotnet ef migrations add MigrationName`
4. Update database: `dotnet ef database update`
5. Add controller in `Controllers/`

### Entity Framework Commands
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Database Seeding
The `Seed.cs` class provides bulk data generation for testing:
- 20,000 owners
- ~24,000 pets
- Clinical notes and lab reports with realistic distributions
- Configurable years of historical data

## Architecture Notes
- Uses Entity Framework Code First approach
- Fluent API for precise entity mapping
- Automatic migrations in development mode
- Repository pattern through DbContext
- Structured logging with Serilog

## API Documentation
When running in development mode, comprehensive API documentation is available at `/swagger` with:
- Interactive endpoint testing
- Request/response schemas
- Authentication requirements (when implemented)

## Future Enhancements
- Authentication and authorization
- API versioning
- Performance monitoring
- Caching strategies
- Additional endpoints for clinical workflows
