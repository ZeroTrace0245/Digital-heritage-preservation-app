using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly HeritageDbContext _context;

    public ArtifactsController(HeritageDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Artifact>>> GetArtifacts([FromQuery] string? category = null, [FromQuery] string? culture = null)
    {
        var query = _context.Artifacts.Include(a => a.Contributor).AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(a => a.Category == category);
        if (!string.IsNullOrEmpty(culture))
            query = query.Where(a => a.Culture == culture);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Artifact>> GetArtifact(int id)
    {
        var artifact = await _context.Artifacts.Include(a => a.Contributor).FirstOrDefaultAsync(a => a.Id == id);
        return artifact == null ? NotFound() : artifact;
    }

    [HttpPost]
    public async Task<ActionResult<Artifact>> CreateArtifact(Artifact artifact)
    {
        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetArtifact), new { id = artifact.Id }, artifact);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtifact(int id, Artifact artifact)
    {
        if (id != artifact.Id) return BadRequest();

        var existing = await _context.Artifacts.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = artifact.Name;
        existing.Description = artifact.Description;
        existing.Category = artifact.Category;
        existing.HistoricalContext = artifact.HistoricalContext;
        existing.Culture = artifact.Culture;
        existing.EstimatedAge = artifact.EstimatedAge;
        existing.Materials = artifact.Materials;
        existing.Location = artifact.Location;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtifact(int id)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null) return NotFound();

        _context.Artifacts.Remove(artifact);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
