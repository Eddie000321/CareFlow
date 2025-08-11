using api_dotnet.Data; // CareflowDb, Configurations
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext (PostgreSQL)
builder.Services.AddDbContext<CareflowDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add controllers (if you have them)
builder.Services.AddControllers();

// 3. Add Swagger (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Apply migrations and seed data automatically (optional for dev only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CareflowDb>();
    db.Database.Migrate();
    
    // Add seed data in development environment
    if (app.Environment.IsDevelopment())
    {
        // Create minimal seed data for testing
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
}

// 5. Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers(); // If using attribute routing

app.Run();