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

    [NotMapped] public string AgeLabel
    {
        get
        {
            var b = BirthDate.Date; var t = DateTime.UtcNow.Date;
            var y = t.Year - b.Year; var m = t.Month - b.Month; if (t.Day < b.Day) m--;
            if (m < 0) { y--; m += 12; }
            return $"{y}y {m}m";
        }
    }
}