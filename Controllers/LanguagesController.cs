using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly HeritageDbContext _context;

    public LanguagesController(HeritageDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LanguageEntry>>> GetLanguageEntries([FromQuery] string? language = null, [FromQuery] string? dialect = null)
    {
        var query = _context.LanguageEntries.Include(l => l.Contributor).AsQueryable();

        if (!string.IsNullOrEmpty(language))
            query = query.Where(l => l.Language == language);
        if (!string.IsNullOrEmpty(dialect))
            query = query.Where(l => l.Dialect == dialect);

        return await query.OrderBy(l => l.Word).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LanguageEntry>> GetLanguageEntry(int id)
    {
        var entry = await _context.LanguageEntries.Include(l => l.Contributor).FirstOrDefaultAsync(l => l.Id == id);
        return entry == null ? NotFound() : entry;
    }

    [HttpGet("languages")]
    public async Task<ActionResult<IEnumerable<string>>> GetLanguages()
    {
        var languages = await _context.LanguageEntries.Select(l => l.Language).Distinct().ToListAsync();
        return Ok(languages);
    }

    [HttpPost]
    public async Task<ActionResult<LanguageEntry>> CreateLanguageEntry(LanguageEntry entry)
    {
        _context.LanguageEntries.Add(entry);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLanguageEntry), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLanguageEntry(int id, LanguageEntry entry)
    {
        if (id != entry.Id) return BadRequest();

        var existing = await _context.LanguageEntries.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Word = entry.Word;
        existing.Translation = entry.Translation;
        existing.PartOfSpeech = entry.PartOfSpeech;
        existing.Example = entry.Example;
        existing.ExampleTranslation = entry.ExampleTranslation;
        existing.IPA = entry.IPA;
        existing.Notes = entry.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLanguageEntry(int id)
    {
        var entry = await _context.LanguageEntries.FindAsync(id);
        if (entry == null) return NotFound();

        _context.LanguageEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
