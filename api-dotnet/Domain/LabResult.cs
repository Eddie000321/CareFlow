using System.ComponentModel.DataAnnotations;

namespace CareFlow.Domain;

public class LabResult
{
    public int Id { get; set; }
    public int LabReportId { get; set; }
    public LabReport Report { get; set; } = null!;

    [MaxLength(64)]  public string AnalyteCode { get; set; } = ""; // Standard code, if available
    [MaxLength(128)] public string AnalyteName { get; set; } = ""; // Display name

    public decimal? ValueNumeric { get; set; }   // Numeric value
    public string?  ValueText    { get; set; }   // Qualitative (e.g., "trace")
    public string?  Units        { get; set; }   // e.g., mg/dL

    public decimal? RefLow  { get; set; }
    public decimal? RefHigh { get; set; }
    public string?  Flag    { get; set; }        // e.g., "L", "H", "A"
}