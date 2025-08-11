using System.ComponentModel.DataAnnotations;

namespace api_dotnet.Domain;

public class ClinicalNote
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(100)] public string? Author { get; set; }
    [MaxLength(4000)] public required string Content { get; set; }
}