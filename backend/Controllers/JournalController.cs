using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Models.DTOs;

namespace LevelingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JournalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JournalController> _logger;

    public JournalController(ApplicationDbContext context, ILogger<JournalController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetCharacterId()
    {
        var characterIdClaim = User.FindFirst("CharacterId")?.Value;
        if (string.IsNullOrEmpty(characterIdClaim))
        {
            throw new UnauthorizedAccessException("Character ID not found in token");
        }
        return Guid.Parse(characterIdClaim);
    }

    [HttpGet]
    public async Task<ActionResult<List<JournalEntryDto>>> GetEntries()
    {
        try
        {
            var characterId = GetCharacterId();

            var entries = await _context.JournalEntries
                .Where(j => j.CharacterId == characterId)
                .OrderByDescending(j => j.Date)
                .Take(30) // Last 30 entries
                .ToListAsync();

            var response = entries.Select(e => new JournalEntryDto
            {
                Id = e.Id,
                Date = e.Date,
                Content = e.Content,
                CreatedAt = e.CreatedAt
            }).ToList();

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries");
            return StatusCode(500, new { message = "An error occurred while retrieving journal entries" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> CreateEntry(CreateJournalEntryRequest request)
    {
        try
        {
            var characterId = GetCharacterId();

            var entry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                Date = DateTime.UtcNow.Date,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            var response = new JournalEntryDto
            {
                Id = entry.Id,
                Date = entry.Date,
                Content = entry.Content,
                CreatedAt = entry.CreatedAt
            };

            return CreatedAtAction(nameof(GetEntries), new { id = entry.Id }, response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry");
            return StatusCode(500, new { message = "An error occurred while creating journal entry" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JournalEntryDto>> UpdateEntry(Guid id, CreateJournalEntryRequest request)
    {
        try
        {
            var characterId = GetCharacterId();

            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == id && j.CharacterId == characterId);

            if (entry == null)
            {
                return NotFound(new { message = "Journal entry not found" });
            }

            entry.Content = request.Content;

            await _context.SaveChangesAsync();

            var response = new JournalEntryDto
            {
                Id = entry.Id,
                Date = entry.Date,
                Content = entry.Content,
                CreatedAt = entry.CreatedAt
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating journal entry");
            return StatusCode(500, new { message = "An error occurred while updating journal entry" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(Guid id)
    {
        try
        {
            var characterId = GetCharacterId();

            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == id && j.CharacterId == characterId);

            if (entry == null)
            {
                return NotFound(new { message = "Journal entry not found" });
            }

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journal entry");
            return StatusCode(500, new { message = "An error occurred while deleting journal entry" });
        }
    }
}
