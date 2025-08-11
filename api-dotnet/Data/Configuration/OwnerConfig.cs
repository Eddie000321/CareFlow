using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OwnerConfig : IEntityTypeConfiguration<Owner> // Connect with CareFlowDb.cs
{
    public void Configure(EntityTypeBuilder<Owner> mb)
    {
        mb.HasKey(o => o.Id);

        // Property : Configures a single scalar property (column) of your entity.
        // ComplexProperty(EF Core8+ Only) :Configures a nested value object inside your entity, not a simple scalar.
        mb.Property(o => o.Name).IsRequired().HasMaxLength(100);
        mb.Property(o => o.Phone).IsRequired().HasMaxLength(30);
        mb.Property(o => o.Email).HasMaxLength(254);

        mb.HasIndex(o => new { o.Name, o.Phone });

        mb.ComplexProperty(o => o.OwnerAddress, a =>
        {
            a.Property(p => p.Country).IsRequired().HasMaxLength(2).HasColumnName("Country");
            a.Property(p => p.Province).IsRequired().HasMaxLength(2).HasColumnName("Province");
            a.Property(p => p.City).IsRequired().HasMaxLength(100).HasColumnName("City");
            a.Property(p => p.Street).IsRequired().HasMaxLength(200).HasColumnName("Street");
            a.Property(p => p.PostalCode).IsRequired().HasMaxLength(10).HasColumnName("Postal Code");
        });

        // For search
        mb.HasIndex(o => EF.Property<String>(o, "City"));
        mb.HasIndex(o => EF.Property<String>(o, "Province"));
        mb.HasIndex(o => EF.Property<String>(o, "PostalCode"));

        mb.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}