namespace api_dotnet.Domain.Dtos;

public sealed class LabReportDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = "";
    public string TestType { get; set; } = "";
    public DateTimeOffset CollectedAt { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public string? LabName { get; set; }
    public string? Status { get; set; }
    public int ResultCount { get; set; }
}