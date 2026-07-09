using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_dotnet.Data;
using api_dotnet.Domain;
using api_dotnet.Domain.Dtos;

namespace api_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly CareflowDb _context;

    public PetsController(CareflowDb context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetDto>>> GetPets()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
        var asOf = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;

        var rows = await _context.Pets.AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Species,
                p.Breed,
                p.BirthDate,
                TotalMonths =
                    ((asOf.Year - p.BirthDate.Year) * 12)
                    + (asOf.Month - p.BirthDate.Month)
                    - (asOf.Day < p.BirthDate.Day ? 1 : 0)
            })
            .ToListAsync();

        var result = rows.Select(x =>
        {
            var age = PetAgeCalculator.FromBirthDate(x.BirthDate, asOf);

            return new PetDto
            {
                Id = x.Id,
                Name = x.Name,
                Species = x.Species,
                Breed = x.Breed,
                BirthDate = x.BirthDate,
                AgeYears = age.Years,
                AgeMonths = age.Months,
                AgeTotalMonths = age.TotalMonths,
                AgeAsOf = age.AsOf,
                AgeLabel = age.Label
            };
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetPet(int id)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
        var asOf = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;

        var pet = await _context.Pets.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Species,
                p.Breed,
                p.BirthDate,
                TotalMonths =
                    ((asOf.Year - p.BirthDate.Year) * 12)
                    + (asOf.Month - p.BirthDate.Month)
                    - (asOf.Day < p.BirthDate.Day ? 1 : 0)
            })
            .FirstOrDefaultAsync();

        if (pet == null)
        {
            return NotFound();
        }

        var age = PetAgeCalculator.FromBirthDate(pet.BirthDate, asOf);

        var result = new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            AgeYears = age.Years,
            AgeMonths = age.Months,
            AgeTotalMonths = age.TotalMonths,
            AgeAsOf = age.AsOf,
            AgeLabel = age.Label
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Pet>> PostPet(CreatePetRequest request)
    {
        if (!await _context.Owners.AnyAsync(owner => owner.Id == request.OwnerId))
        {
            return BadRequest("Cannot create a pet for an owner that does not exist.");
        }

        var pet = new Pet
        {
            OwnerId = request.OwnerId,
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            BirthDate = request.BirthDate!.Value
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPet), new { id = pet.Id }, pet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPet(int id, Pet pet)
    {
        if (id != pet.Id)
        {
            return BadRequest();
        }

        _context.Entry(pet).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PetExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.ClinicalNotes)
            .Include(p => p.LabReports)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null)
        {
            return NotFound();
        }

        if (pet.ClinicalNotes.Count > 0 || pet.LabReports.Count > 0)
        {
            return BadRequest("Cannot delete a pet with existing clinical notes or lab reports. Archive records first.");
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PetExists(int id)
    {
        return _context.Pets.Any(e => e.Id == id);
    }
}
