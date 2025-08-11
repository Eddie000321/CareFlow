namespace api_dotnet.Domain.Dtos;

public sealed class LabResultDto
{
    public int Id { get; set; }
    public int LabReportId { get; set; }
    public string AnalyteCode { get; set; } = "";
    public string AnalyteName { get; set; } = "";
    public decimal? ValueNumeric { get; set; }
    public string? ValueText { get; set; }
    public string? Units { get; set; }
    public decimal? RefLow { get; set; }
    public decimal? RefHigh { get; set; }
    public string? Flag { get; set; }
    public string? Status { get; set; } // Normal, High, Low, Critical, etc.
}

public sealed class LabResultWithReportDto
{
    public int Id { get; set; }
    public int LabReportId { get; set; }
    public string AnalyteCode { get; set; } = "";
    public string AnalyteName { get; set; } = "";
    public decimal? ValueNumeric { get; set; }
    public string? ValueText { get; set; }
    public string? Units { get; set; }
    public decimal? RefLow { get; set; }
    public decimal? RefHigh { get; set; }
    public string? Flag { get; set; }
    public string? Status { get; set; }
    public string PetName { get; set; } = "";
    public string TestType { get; set; } = "";
    public DateTimeOffset ReportedAt { get; set; }
}