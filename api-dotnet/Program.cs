using api_dotnet.Data; // CareflowDb, Configurations
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// 1. Add DbContext (PostgreSQL)
builder.Services.AddDbContext<CareflowDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add controllers (if you have them)
builder.Services.AddControllers();

// 3. Add Swagger (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// 4. Keep schema mutation and sample data strictly inside local development.
// Production deployments should apply reviewed migrations as a separate release step.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CareflowDb>();
    await db.Database.MigrateAsync();

    if (!await db.Pets.AnyAsync())
    {
        var owner = new api_dotnet.Domain.Owner
        {
            Name = "Test Owner",
            Phone = "555-0123",
            Email = "test@example.com",
            OwnerAddress = new api_dotnet.Domain.Owner.Address
            {
                Street = "123 Main St",
                City = "Toronto",
                Province = "ON",
                PostalCode = "M1M1M1",
                Country = "CA"
            }
        };
        db.Owners.Add(owner);
        await db.SaveChangesAsync();

        var pet = new api_dotnet.Domain.Pet
        {
            Name = "Test Pet",
            Species = "Canine",
            Breed = "Golden Retriever",
            BirthDate = DateTime.Now.AddYears(-3).AddMonths(-6),
            OwnerId = owner.Id
        };
        db.Pets.Add(pet);
        await db.SaveChangesAsync();
    }
}

// 5. Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers(); // If using attribute routing

app.Run();

// Expose the top-level entry point to WebApplicationFactory without changing
// the application's production startup path.
public partial class Program;
