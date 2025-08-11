using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_dotnet.Domain;

public class Owner
{
    public int Id { get; set; }

    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(30)]  public required string Phone { get; set; }
    [MaxLength(254)] public string? Email { get; set; }

    public Address OwnerAddress { get; set; } = new();

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();

    [ComplexType] // EF Core 8+
    public class Address
{
    [MaxLength(2)]   public string Country { get; set; } = "CA"; // ISO-2
    [MaxLength(2)]   public string Province { get; set; } = "ON"; // e.g., ON
    [MaxLength(100)] public string City { get; set; } = "";
    [MaxLength(200)] public string Street { get; set; } = "";
    [MaxLength(10)]  public string PostalCode { get; set; } = "";
}
}