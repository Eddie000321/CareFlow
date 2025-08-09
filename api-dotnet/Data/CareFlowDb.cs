using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Data;

/// <summary>
/// EF Core DbContext – registers entities and configures DB-specific details.
/// </summary>
public class CareflowDb : DbContext
{
    /// <summary>
    /// Constructs the application's database context.
    /// The <paramref name="options"/> are provided by dependency injection and include
    /// configuration such as the database provider and connection string. The options are
    /// passed to the base <see cref="DbContext"/> so Entity Framework Core can initialize properly.
    /// </summary>
    public CareflowDb(DbContextOptions<CareflowDb> options) : base(options) { }

    //    
    /// <summary>
    /// Represents the Owners table. Use this set to query and save <see cref="Owner"/> entities.
    /// Exposes a queryable, change-tracked collection mapped to the database.
    /// </summary>
    public DbSet<Owner> Owners => Set<Owner>();
    /// <summary>
    /// Represents the Pets table. Each <see cref="Pet"/> belongs to an Owner and can have many medical records.
    /// Query this set to read/write pet rows in the database.
    /// </summary>
    public DbSet<Pet> Pets => Set<Pet>();
    /// <summary>
    /// Represents the MedicalRecords table. Each <see cref="MedicalRecord"/> is linked to a Pet.
    /// Use this set for CRUD operations on medical records (often queried by PetId and RecordedAt).
    /// </summary>
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    protected override void OnModelCreating(ModelBuilder db)
    {
        db.Entity<Owner>()
        .HasMany(o => o.Pets)
        .WithOne(p => p.Owner)
        .HasForeignKey(p => p.OwnerId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
