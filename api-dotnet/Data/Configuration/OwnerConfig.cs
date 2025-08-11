using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OwnerConfig : IEntityTypeConfiguration<Owner> // Connect with CareFlowDb.cs
{
    public void Configure(EntityTypeBuilder<Owner> mb)
    {
        mb.HasKey(o => o.Id);

        mb.HasIndex(o => new { o.Name, o.Phone });

        // Property : Configures a single scalar property (column) of your entity.
        // ComplexProperty(EF Core8+ Only) :Configures a nested value object inside your entity, not a simple scalar.
        mb.Property();


    }
}