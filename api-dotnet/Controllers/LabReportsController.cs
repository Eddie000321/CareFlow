using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_dotnet.Data;
using api_dotnet.Domain;
using api_dotnet.Domain.Dtos;

namespace api_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabReportsController : ControllerBase
{
    private readonly CareflowDb _context;

    public LabReportsController(CareflowDb context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabReportDto>>> GetLabReports()
    {
        var reports = await _context.LabReports
            .Include(lr => lr.Pet)
            .Include(lr => lr.Results)
            .AsNoTracking()
            .Select(lr => new LabReportDto
            {
                Id = lr.Id,
                PetId = lr.PetId,
                PetName = lr.Pet.Name,
                TestType = lr.TestType.ToString(),
                CollectedAt = lr.CollectedAt,
                ReportedAt = lr.ReportedAt,
                LabName = lr.LabName,
                Status = lr.Status,
                ResultCount = lr.Results.Count
            })
            .OrderByDescending(lr => lr.ReportedAt)
            .ToListAsync();

        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LabReportDto>> GetLabReport(int id)
    {
        var report = await _context.LabReports
            .Include(lr => lr.Pet)
            .Include(lr => lr.Results)
            .AsNoTracking()
            .Where(lr => lr.Id == id)
            .Select(lr => new LabReportDto
            {
                Id = lr.Id,
                PetId = lr.PetId,
                PetName = lr.Pet.Name,
                TestType = lr.TestType.ToString(),
                CollectedAt = lr.CollectedAt,
                ReportedAt = lr.ReportedAt,
                LabName = lr.LabName,
                Status = lr.Status,
                ResultCount = lr.Results.Count
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpGet("pet/{petId}")]
    public async Task<ActionResult<IEnumerable<LabReportDto>>> GetLabReportsByPet(int petId)
    {
        var reports = await _context.LabReports
            .Include(lr => lr.Pet)
            .Include(lr => lr.Results)
            .AsNoTracking()
            .Where(lr => lr.PetId == petId)
            .Select(lr => new LabReportDto
            {
                Id = lr.Id,
                PetId = lr.PetId,
                PetName = lr.Pet.Name,
                TestType = lr.TestType.ToString(),
                CollectedAt = lr.CollectedAt,
                ReportedAt = lr.ReportedAt,
                LabName = lr.LabName,
                Status = lr.Status,
                ResultCount = lr.Results.Count
            })
            .OrderByDescending(lr => lr.ReportedAt)
            .ToListAsync();

        return Ok(reports);
    }

    [HttpGet("{id}/with-results")]
    public async Task<ActionResult> GetLabReportWithResults(int id)
    {
        var report = await _context.LabReports
            .Include(lr => lr.Pet)
            .Include(lr => lr.Results)
            .AsNoTracking()
            .Where(lr => lr.Id == id)
            .Select(lr => new
            {
                Id = lr.Id,
                PetId = lr.PetId,
                PetName = lr.Pet.Name,
                TestType = lr.TestType.ToString(),
                CollectedAt = lr.CollectedAt,
                ReportedAt = lr.ReportedAt,
                LabName = lr.LabName,
                Status = lr.Status,
                Results = lr.Results.Select(r => new
                {
                    r.Id,
                    r.AnalyteCode,
                    r.AnalyteName,
                    r.ValueNumeric,
                    r.ValueText,
                    r.Units,
                    r.RefLow,
                    r.RefHigh,
                    r.Flag
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<LabReport>> PostLabReport(LabReport report)
    {
        _context.LabReports.Add(report);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLabReport), new { id = report.Id }, report);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutLabReport(int id, LabReport report)
    {
        if (id != report.Id)
        {
            return BadRequest();
        }

        _context.Entry(report).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LabReportExists(id))
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
    public async Task<IActionResult> DeleteLabReport(int id)
    {
        var report = await _context.LabReports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        _context.LabReports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool LabReportExists(int id)
    {
        return _context.LabReports.Any(e => e.Id == id);
    }
}