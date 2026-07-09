using System.ComponentModel.DataAnnotations;

namespace api_dotnet.Domain.Dtos;

public sealed class CreatePetRequest
{
    [Range(1, int.MaxValue)]
    public int OwnerId { get; init; }

    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Species { get; init; } = string.Empty;

    [MaxLength(30)]
    public string? Breed { get; init; }

    [Required]
    public DateTime? BirthDate { get; init; }
}
