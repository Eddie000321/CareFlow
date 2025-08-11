using api_dotnet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_dotnet.Data.Configuration;

public class Petconfig : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> mb)
    {
        mb.HasKey(p => p.Id);
        mb.Property(p => p.Name).IsRequired().HasMaxLength(100);
        mb.Property(p => p.Species).IsRequired().HasMaxLength(30);
        mb.Property(p => p.Breed).HasMaxLength(30);
        mb.Property(p => p.BirthDate).HasColumnType("date");

        mb.HasIndex(p => new { p.OwnerId, p.Name });

        mb.HasOne(p => p.Owner)
          .WithMany(o => o.Pets)
          .HasForeignKey(p => p.OwnerId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}