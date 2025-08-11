namespace api_dotnet.Domain.Dtos;

public sealed class ClinicalNoteDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
    public string? Author { get; set; }
    public string Content { get; set; } = "";
}