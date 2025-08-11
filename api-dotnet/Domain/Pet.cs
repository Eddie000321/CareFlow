using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_dotnet.Domain;

public class Pet
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(30)]  public required string Species { get; set; }
    [MaxLength(30)]  public string? Breed { get; set; }

    [Column(TypeName = "date")] public required DateTime BirthDate { get; set; }

    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public ICollection<LabReport>    LabReports    { get; set; } = new List<LabReport>();

}