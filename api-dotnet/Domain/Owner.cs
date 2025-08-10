using System.ComponentModel.DataAnnotations;

namespace CareFlow.Domain;

public class Owner
{
    public int Id { get; set; }

    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(30)]  public required string Phone { get; set; }
    [MaxLength(254)] public string? Email { get; set; }
    [MaxLength(200)] public required string Address { get; set; }

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}