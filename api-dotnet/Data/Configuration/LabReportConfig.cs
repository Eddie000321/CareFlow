using api_dotnet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_dotnet.Data.Configuration;

public sealed class LabReportConfig : IEntityTypeConfiguration<LabReport>
{
    public void Configure(EntityTypeBuilder<LabReport> mb)
    {
        mb.HasKey(report => report.Id);

        mb.Property(report => report.TestType)
            .HasConversion<int>();

        mb.Property(report => report.CollectedAt)
            .IsRequired();

        mb.Property(report => report.ReportedAt)
            .IsRequired();

        mb.Property(report => report.LabName)
            .HasMaxLength(128);

        mb.Property(report => report.Status)
            .HasMaxLength(32);

        mb.HasIndex(report => new { report.PetId, report.ReportedAt });

        mb.HasOne(report => report.Pet)
            .WithMany(pet => pet.LabReports)
            .HasForeignKey(report => report.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.HasMany(report => report.Results)
            .WithOne(result => result.Report)
            .HasForeignKey(result => result.LabReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
