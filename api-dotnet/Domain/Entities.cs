using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareFlow.Domain;

public class Owner // Table Owner
{
    // Properties Table Column
    public int Id { get; set; }
    [MaxLength(100)] public required string Name { get; set; } 
    [MaxLength(30)] public required string Phone { get; set; }
    [MaxLength(254)] public string? Email { get; set; }
    [MaxLength(200)] public required string Address { get; set; } 
    // 1:N relationship with Pet
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}

public class Pet // Table Pet
{
    public int OwnerId { get; set; } // FK
    public Owner Owner { get; set; } = null!;    

    public int Id { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(30)] public required string Species { get; set; }
    [MaxLength(30)] public string? Breed { get; set; }

    [Column(TypeName = "date")] public required DateTime BirthDate { get; set; }

    /*
      Pet → MedicalRecord is a 1:N relationship.
      - Use ICollection<T> so EF can accurately track Add/Remove (change tracking).
      - IEnumerable<T> is read-only and cannot express collection mutations.
      - Expose an interface so the internal implementation (e.g., List or HashSet) stays swappable.
    */
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    

 
    [NotMapped] public int AgeYears => CalcAge(BirthDate).Years;
    [NotMapped] public int AgeMonths => CalcAge(BirthDate).Months;
    [NotMapped] public string AgeLabel => $"{AgeYears}y {AgeMonths}m";

    private static (int Years, int Months) CalcAge(DateTime birthDate)
    {
        var birth = birthDate.Date;
        var today = DateTime.UtcNow.Date; // using UtcNow or DateOnly type

        if (today < birth) return (0, 0);

        int years = today.Year - birth.Year;
        int months = today.Month - birth.Month;
        int days = today.Day - birth.Day;

        if (days < 0)
        {
            months--;
        }
        if (months < 0)
        {
            years--;
            months += 12;
        }

        return (years, months);
    }

}

public class MedicalRecord // Table MedicalRecord
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;    // EF가 주입

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]  public string? Diagnosis  { get; set; }
    [MaxLength(200)]  public string? Treatment  { get; set; }
    [MaxLength(1000)] public string? Notes      { get; set; }
    public required decimal WeightKg { get; set; }
}