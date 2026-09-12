using DigitalHeritageApp.Data;
using DigitalHeritageApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DigitalHeritageApp.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly HeritageDbContext _context;
    public UsersController(HeritageDbContext context) => _context = context;

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(int id) => await _context.Users.FindAsync(id) is { } user ? user : NotFound();

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> Update(int id, User profile)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.Name = profile.Name; user.Email = profile.Email; user.Community = profile.Community; user.Bio = profile.Bio;
        await _context.SaveChangesAsync();
        return user;
    }
}
