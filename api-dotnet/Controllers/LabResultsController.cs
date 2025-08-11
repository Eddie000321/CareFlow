using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_dotnet.Data;
using api_dotnet.Domain;
using api_dotnet.Domain.Dtos;

namespace api_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabResultsController : ControllerBase
{
    private readonly CareflowDb _context;

    public LabResultsController(CareflowDb context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabResultWithReportDto>>> GetLabResults()
    {
        var results = await _context.LabResults
            .Include(lr => lr.Report)
            .ThenInclude(r => r.Pet)
            .AsNoTracking()
            .Select(lr => new LabResultWithReportDto
            {
                Id = lr.Id,
                LabReportId = lr.LabReportId,
                AnalyteCode = lr.AnalyteCode,
                AnalyteName = lr.AnalyteName,
                ValueNumeric = lr.ValueNumeric,
                ValueText = lr.ValueText,
                Units = lr.Units,
                RefLow = lr.RefLow,
                RefHigh = lr.RefHigh,
                Flag = lr.Flag,
                Status = DetermineStatus(lr.ValueNumeric, lr.RefLow, lr.RefHigh, lr.Flag),
                PetName = lr.Report.Pet.Name,
                TestType = lr.Report.TestType.ToString(),
                ReportedAt = lr.Report.ReportedAt
            })
            .OrderByDescending(lr => lr.ReportedAt)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LabResultWithReportDto>> GetLabResult(int id)
    {
        var result = await _context.LabResults
            .Include(lr => lr.Report)
            .ThenInclude(r => r.Pet)
            .AsNoTracking()
            .Where(lr => lr.Id == id)
            .Select(lr => new LabResultWithReportDto
            {
                Id = lr.Id,
                LabReportId = lr.LabReportId,
                AnalyteCode = lr.AnalyteCode,
                AnalyteName = lr.AnalyteName,
                ValueNumeric = lr.ValueNumeric,
                ValueText = lr.ValueText,
                Units = lr.Units,
                RefLow = lr.RefLow,
                RefHigh = lr.RefHigh,
                Flag = lr.Flag,
                Status = DetermineStatus(lr.ValueNumeric, lr.RefLow, lr.RefHigh, lr.Flag),
                PetName = lr.Report.Pet.Name,
                TestType = lr.Report.TestType.ToString(),
                ReportedAt = lr.Report.ReportedAt
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("report/{reportId}")]
    public async Task<ActionResult<IEnumerable<LabResultDto>>> GetLabResultsByReport(int reportId)
    {
        var results = await _context.LabResults
            .AsNoTracking()
            .Where(lr => lr.LabReportId == reportId)
            .Select(lr => new LabResultDto
            {
                Id = lr.Id,
                LabReportId = lr.LabReportId,
                AnalyteCode = lr.AnalyteCode,
                AnalyteName = lr.AnalyteName,
                ValueNumeric = lr.ValueNumeric,
                ValueText = lr.ValueText,
                Units = lr.Units,
                RefLow = lr.RefLow,
                RefHigh = lr.RefHigh,
                Flag = lr.Flag,
                Status = DetermineStatus(lr.ValueNumeric, lr.RefLow, lr.RefHigh, lr.Flag)
            })
            .OrderBy(lr => lr.AnalyteName)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("abnormal")]
    public async Task<ActionResult<IEnumerable<LabResultWithReportDto>>> GetAbnormalResults()
    {
        var results = await _context.LabResults
            .Include(lr => lr.Report)
            .ThenInclude(r => r.Pet)
            .AsNoTracking()
            .Where(lr => lr.Flag != null && lr.Flag != "")
            .Select(lr => new LabResultWithReportDto
            {
                Id = lr.Id,
                LabReportId = lr.LabReportId,
                AnalyteCode = lr.AnalyteCode,
                AnalyteName = lr.AnalyteName,
                ValueNumeric = lr.ValueNumeric,
                ValueText = lr.ValueText,
                Units = lr.Units,
                RefLow = lr.RefLow,
                RefHigh = lr.RefHigh,
                Flag = lr.Flag,
                Status = DetermineStatus(lr.ValueNumeric, lr.RefLow, lr.RefHigh, lr.Flag),
                PetName = lr.Report.Pet.Name,
                TestType = lr.Report.TestType.ToString(),
                ReportedAt = lr.Report.ReportedAt
            })
            .OrderByDescending(lr => lr.ReportedAt)
            .ToListAsync();

        return Ok(results);
    }

    [HttpPost]
    public async Task<ActionResult<LabResult>> PostLabResult(LabResult result)
    {
        _context.LabResults.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLabResult), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutLabResult(int id, LabResult result)
    {
        if (id != result.Id)
        {
            return BadRequest();
        }

        _context.Entry(result).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LabResultExists(id))
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
    public async Task<IActionResult> DeleteLabResult(int id)
    {
        var result = await _context.LabResults.FindAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        _context.LabResults.Remove(result);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool LabResultExists(int id)
    {
        return _context.LabResults.Any(e => e.Id == id);
    }

    private static string DetermineStatus(decimal? value, decimal? refLow, decimal? refHigh, string? flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            return flag switch
            {
                "H" => "High",
                "L" => "Low",
                "A" => "Abnormal",
                "C" => "Critical",
                _ => "Flagged"
            };
        }

        if (value.HasValue && refLow.HasValue && refHigh.HasValue)
        {
            if (value < refLow) return "Low";
            if (value > refHigh) return "High";
            return "Normal";
        }

        return "Unknown";
    }
}