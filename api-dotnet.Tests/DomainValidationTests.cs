using System.ComponentModel.DataAnnotations;
using api_dotnet.Domain;
using Xunit;

namespace api_dotnet.Tests;

public sealed class DomainValidationTests
{
    [Fact]
    public void LabReport_BoundedMetadata_IsRejectedBeforePersistence()
    {
        var report = new LabReport
        {
            LabName = new string('L', 129),
            Status = new string('S', 33)
        };

        var memberNames = Validate(report);

        Assert.Contains(nameof(LabReport.LabName), memberNames);
        Assert.Contains(nameof(LabReport.Status), memberNames);
    }

    [Fact]
    public void LabResult_BoundedMetadata_IsRejectedBeforePersistence()
    {
        var result = new LabResult
        {
            AnalyteCode = "CBC",
            AnalyteName = "Complete blood count",
            Units = new string('U', 33),
            Flag = new string('F', 9)
        };

        var memberNames = Validate(result);

        Assert.Contains(nameof(LabResult.Units), memberNames);
        Assert.Contains(nameof(LabResult.Flag), memberNames);
    }

    private static HashSet<string> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results.SelectMany(result => result.MemberNames).ToHashSet(StringComparer.Ordinal);
    }
}
