using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Data;

public class CareflowDb : DbContext
{
    public CareflowDb(DbContextOptions<CareflowDb> options) : base(options) { }

    public DbSet<Owner>        Owners        => Set<Owner>();
    public DbSet<Pet>          Pets          => Set<Pet>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
    public DbSet<LabReport>    LabReports    => Set<LabReport>();
    public DbSet<LabResult>    LabResults    => Set<LabResult>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(CareflowDb).Assembly);
    }
}