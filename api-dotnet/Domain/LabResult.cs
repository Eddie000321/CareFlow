using System.ComponentModel.DataAnnotations;

namespace CareFlow.Domain;

public class LabResult
{
    public int Id { get; set; }
    public int LabReportId { get; set; }
    public LabReport Report { get; set; } = null!;

    [MaxLength(64)]  public string AnalyteCode { get; set; } = ""; // 표준코드 있으면
    [MaxLength(128)] public string AnalyteName { get; set; } = ""; // 표시명

    public decimal? ValueNumeric { get; set; }   // 수치
    public string?  ValueText    { get; set; }   // 정성(ex: "trace")
    public string?  Units        { get; set; }   // ex: mg/dL

    public decimal? RefLow  { get; set; }
    public decimal? RefHigh { get; set; }
    public string?  Flag    { get; set; }        // "L","H","A" 등
}