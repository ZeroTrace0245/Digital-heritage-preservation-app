using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly HeritageDbContext _context;

    public EventsController(HeritageDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CulturalEvent>>> GetEvents([FromQuery] string? culture = null, [FromQuery] string? type = null)
    {
        var query = _context.CulturalEvents.Include(e => e.Contributor).AsQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(e => e.Culture == culture);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(e => e.Type == type);

        return await query.OrderBy(e => e.StartDate).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CulturalEvent>> GetEvent(int id)
    {
        var @event = await _context.CulturalEvents.Include(e => e.Contributor).FirstOrDefaultAsync(e => e.Id == id);
        return @event == null ? NotFound() : @event;
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<CulturalEvent>>> GetUpcomingEvents()
    {
        var now = DateTime.UtcNow;
        return await _context.CulturalEvents
            .Include(e => e.Contributor)
            .Where(e => e.StartDate >= now)
            .OrderBy(e => e.StartDate)
            .Take(10)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CulturalEvent>> CreateEvent(CulturalEvent @event)
    {
        _context.CulturalEvents.Add(@event);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, CulturalEvent @event)
    {
        if (id != @event.Id) return BadRequest();

        var existing = await _context.CulturalEvents.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = @event.Title;
        existing.Description = @event.Description;
        existing.Culture = @event.Culture;
        existing.Type = @event.Type;
        existing.StartDate = @event.StartDate;
        existing.EndDate = @event.EndDate;
        existing.Location = @event.Location;
        existing.IsRecurring = @event.IsRecurring;
        existing.RecurrencePattern = @event.RecurrencePattern;
        existing.Attendees = @event.Attendees;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var @event = await _context.CulturalEvents.FindAsync(id);
        if (@event == null) return NotFound();

        _context.CulturalEvents.Remove(@event);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
