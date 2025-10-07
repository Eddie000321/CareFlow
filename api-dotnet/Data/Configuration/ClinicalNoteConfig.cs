using api_dotnet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_dotnet.Data.Configuration;

public sealed class ClinicalNoteConfig : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> mb)
    {
        mb.HasKey(note => note.Id);

        mb.Property(note => note.Author)
            .HasMaxLength(100);

        mb.Property(note => note.Content)
            .IsRequired()
            .HasMaxLength(4000);

        mb.Property(note => note.RecordedAt)
            .IsRequired();

        mb.HasIndex(note => new { note.PetId, note.RecordedAt });

        mb.HasOne(note => note.Pet)
            .WithMany(pet => pet.ClinicalNotes)
            .HasForeignKey(note => note.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
