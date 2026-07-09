using System.ComponentModel.DataAnnotations;

namespace api_dotnet.Domain;

public enum LabTestType { CBC, Chemistry, Urinalysis, Thyroid, Unknown }

public class LabReport
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public LabTestType TestType { get; set; } = LabTestType.Unknown;
    public DateTimeOffset CollectedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ReportedAt  { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(128)] public string? LabName { get; set; }
    [MaxLength(32)]  public string? Status  { get; set; } // e.g., "Final", "Prelim"

    public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
}
