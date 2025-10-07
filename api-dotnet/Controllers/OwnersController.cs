using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_dotnet.Data;
using api_dotnet.Domain;

namespace api_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly CareflowDb _context;

    public OwnersController(CareflowDb context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Owner>>> GetOwners()
    {
        var owners = await _context.Owners
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync();

        return Ok(owners);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Owner>> GetOwner(int id)
    {
        var owner = await _context.Owners
            .Include(o => o.Pets)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (owner is null)
        {
            return NotFound();
        }

        return Ok(owner);
    }

    [HttpPost]
    public async Task<ActionResult<Owner>> PostOwner(Owner owner)
    {
        _context.Owners.Add(owner);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOwner), new { id = owner.Id }, owner);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutOwner(int id, Owner owner)
    {
        if (id != owner.Id)
        {
            return BadRequest();
        }

        _context.Entry(owner).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OwnerExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOwner(int id)
    {
        var owner = await _context.Owners.FindAsync(id);
        if (owner is null)
        {
            return NotFound();
        }

        _context.Owners.Remove(owner);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OwnerExists(int id) => _context.Owners.Any(o => o.Id == id);
}
