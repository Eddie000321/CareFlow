using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_dotnet.Data;
using api_dotnet.Domain;
using api_dotnet.Domain.Dtos;

namespace api_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicalNotesController : ControllerBase
{
    private readonly CareflowDb _context;

    public ClinicalNotesController(CareflowDb context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClinicalNoteDto>>> GetClinicalNotes()
    {
        var notes = await _context.ClinicalNotes
            .Include(cn => cn.Pet)
            .AsNoTracking()
            .Select(cn => new ClinicalNoteDto
            {
                Id = cn.Id,
                PetId = cn.PetId,
                PetName = cn.Pet.Name,
                RecordedAt = cn.RecordedAt,
                Author = cn.Author,
                Content = cn.Content
            })
            .OrderByDescending(cn => cn.RecordedAt)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClinicalNoteDto>> GetClinicalNote(int id)
    {
        var note = await _context.ClinicalNotes
            .Include(cn => cn.Pet)
            .AsNoTracking()
            .Where(cn => cn.Id == id)
            .Select(cn => new ClinicalNoteDto
            {
                Id = cn.Id,
                PetId = cn.PetId,
                PetName = cn.Pet.Name,
                RecordedAt = cn.RecordedAt,
                Author = cn.Author,
                Content = cn.Content
            })
            .FirstOrDefaultAsync();

        if (note == null)
        {
            return NotFound();
        }

        return Ok(note);
    }

    [HttpGet("pet/{petId}")]
    public async Task<ActionResult<IEnumerable<ClinicalNoteDto>>> GetClinicalNotesByPet(int petId)
    {
        var notes = await _context.ClinicalNotes
            .Include(cn => cn.Pet)
            .AsNoTracking()
            .Where(cn => cn.PetId == petId)
            .Select(cn => new ClinicalNoteDto
            {
                Id = cn.Id,
                PetId = cn.PetId,
                PetName = cn.Pet.Name,
                RecordedAt = cn.RecordedAt,
                Author = cn.Author,
                Content = cn.Content
            })
            .OrderByDescending(cn => cn.RecordedAt)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<ClinicalNote>> PostClinicalNote(ClinicalNote note)
    {
        _context.ClinicalNotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClinicalNote), new { id = note.Id }, note);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutClinicalNote(int id, ClinicalNote note)
    {
        if (id != note.Id)
        {
            return BadRequest();
        }

        _context.Entry(note).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClinicalNoteExists(id))
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
    public async Task<IActionResult> DeleteClinicalNote(int id)
    {
        var note = await _context.ClinicalNotes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        _context.ClinicalNotes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ClinicalNoteExists(int id)
    {
        return _context.ClinicalNotes.Any(e => e.Id == id);
    }
}