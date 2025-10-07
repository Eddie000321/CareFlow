using api_dotnet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_dotnet.Data.Configuration;

public sealed class LabResultConfig : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> mb)
    {
        mb.HasKey(result => result.Id);

        mb.Property(result => result.AnalyteCode)
            .IsRequired()
            .HasMaxLength(64);

        mb.Property(result => result.AnalyteName)
            .IsRequired()
            .HasMaxLength(128);

        mb.Property(result => result.Flag)
            .HasMaxLength(8);

        mb.Property(result => result.Units)
            .HasMaxLength(32);

        mb.HasIndex(result => new { result.LabReportId, result.AnalyteName });

        mb.HasOne(result => result.Report)
            .WithMany(report => report.Results)
            .HasForeignKey(result => result.LabReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
