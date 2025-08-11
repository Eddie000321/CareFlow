namespace api_dotnet.Domain.Dtos;

public sealed class PetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public DateTime BirthDate { get; set; }
    public int AgeYears { get; set; }
    public int AgeMonths { get; set; }
    public int AgeTotalMonths { get; set; }
    public DateTime AgeAsOf { get; set; }
    public string AgeLabel { get; set; } = "";
}