using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/community")]
public class CommunityController : ControllerBase
{
    private readonly HeritageDbContext _context;
    public CommunityController(HeritageDbContext context) => _context = context;

    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<CommunityItem>>> GetItems([FromQuery] string? type = null)
    {
        var query = _context.CommunityItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.Type == type);
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    [HttpPost("items")]
    public async Task<ActionResult<CommunityItem>> CreateItem(CommunityItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.Description))
            return BadRequest("A title and description are required.");
        _context.CommunityItems.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
    }

    [HttpPost("items/{id}/verify")]
    public async Task<ActionResult<CommunityItem>> Verify(int id, [FromQuery] int verifierId = 1)
    {
        var item = await _context.CommunityItems.FindAsync(id);
        if (item is null) return NotFound();
        item.VerifiedById = verifierId;
        item.VerifiedAt = DateTime.UtcNow;
        item.Status = "Community verified";
        await _context.SaveChangesAsync();
        return item;
    }

    [HttpPost("items/{id}/close")]
    public async Task<ActionResult<CommunityItem>> Close(int id)
    {
        var item = await _context.CommunityItems.FindAsync(id);
        if (item is null) return NotFound();
        item.Status = "Completed";
        await _context.SaveChangesAsync();
        return item;
    }

    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<CommunityGroup>>> GetGroups() =>
        await _context.CommunityGroups.OrderBy(x => x.Name).ToListAsync();

    [HttpPost("groups")]
    public async Task<ActionResult<CommunityGroup>> CreateGroup(CommunityGroup group)
    {
        if (string.IsNullOrWhiteSpace(group.Name)) return BadRequest("A group name is required.");
        _context.CommunityGroups.Add(group);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGroups), new { id = group.Id }, group);
    }
}
