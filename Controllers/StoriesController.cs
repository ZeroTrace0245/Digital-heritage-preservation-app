using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly HeritageDbContext _context;

    public StoriesController(HeritageDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Story>>> GetStories([FromQuery] string? culture = null, [FromQuery] string? language = null)
    {
        var query = _context.Stories.Include(s => s.Contributor).AsQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(s => s.Culture == culture);
        if (!string.IsNullOrEmpty(language))
            query = query.Where(s => s.Language == language);

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Story>> GetStory(int id)
    {
        var story = await _context.Stories.Include(s => s.Contributor).FirstOrDefaultAsync(s => s.Id == id);
        if (story == null) return NotFound();

        story.ViewCount++;
        await _context.SaveChangesAsync();
        return story;
    }

    [HttpPost]
    public async Task<ActionResult<Story>> CreateStory(Story story)
    {
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStory), new { id = story.Id }, story);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStory(int id, Story story)
    {
        if (id != story.Id) return BadRequest();

        var existing = await _context.Stories.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = story.Title;
        existing.Description = story.Description;
        existing.TranscriptionText = story.TranscriptionText;
        existing.Culture = story.Culture;
        existing.Language = story.Language;
        existing.Location = story.Location;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var story = await _context.Stories.FindAsync(id);
        if (story == null) return NotFound();

        _context.Stories.Remove(story);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
