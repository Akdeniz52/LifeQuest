using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LevelingSystem.API.Data;
using LevelingSystem.API.Models.DTOs;
using LevelingSystem.API.Utilities;

namespace LevelingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CharacterController> _logger;

    public CharacterController(
        ApplicationDbContext context,
        ILogger<CharacterController> logger)
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
    public async Task<ActionResult<CharacterProfileResponse>> GetProfile()
    {
        try
        {
            var characterId = GetCharacterId();

            var character = await _context.Characters
                .Include(c => c.Stats)
                    .ThenInclude(s => s.StatDefinition)
                .Include(c => c.SystemMessages)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            var xpForNextLevel = ProgressionFormulas.CalculateXPForLevel(character.Level + 1);
            var unreadMessages = character.SystemMessages.Count(m => !m.IsRead);

            var stats = character.Stats
                .Select(s => new CharacterStatDto
                {
                    Id = s.Id,
                    StatName = s.StatDefinition.Name,
                    Category = s.StatDefinition.Category,
                    CurrentValue = Math.Round(s.CurrentValue, 2),
                    MaxValue = s.StatDefinition.MaxValue,
                    LastUsedAt = s.LastUsedAt,
                    IsLocked = s.StatDefinition.UnlockLevel > character.Level
                })
                .OrderBy(s => s.Category)
                .ThenBy(s => s.StatName)
                .ToList();

            var response = new CharacterProfileResponse
            {
                Id = character.Id,
                Name = character.Name,
                Level = character.Level,
                CurrentXP = character.CurrentXP,
                TotalXP = character.TotalXP,
                XPForNextLevel = xpForNextLevel,
                AvailableStatPoints = character.AvailableStatPoints,
                Stats = stats,
                UnreadMessages = unreadMessages
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving character profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<List<StatDetailResponse>>> GetStats()
    {
        try
        {
            var characterId = GetCharacterId();

            var character = await _context.Characters
                .Include(c => c.Stats)
                    .ThenInclude(s => s.StatDefinition)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            var stats = character.Stats
                .Select(s => new StatDetailResponse
                {
                    Id = s.StatDefinition.Id,
                    Name = s.StatDefinition.Name,
                    Category = s.StatDefinition.Category,
                    Description = s.StatDefinition.Description,
                    CurrentValue = Math.Round(s.CurrentValue, 2),
                    MinValue = s.StatDefinition.MinValue,
                    MaxValue = s.StatDefinition.MaxValue,
                    DecayRate = s.StatDefinition.DecayRate,
                    LastUsedAt = s.LastUsedAt,
                    UnlockLevel = s.StatDefinition.UnlockLevel,
                    IsLocked = s.StatDefinition.UnlockLevel > character.Level
                })
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToList();

            return Ok(stats);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving character stats");
            return StatusCode(500, new { message = "An error occurred while retrieving stats" });
        }
    }

    [HttpPost("distribute-stat")]
    public async Task<ActionResult<CharacterProfileResponse>> DistributeStat(DistributeStatPointRequest request)
    {
        try
        {
            var characterId = GetCharacterId();

            var character = await _context.Characters
                .Include(c => c.Stats)
                    .ThenInclude(s => s.StatDefinition)
                .Include(c => c.SystemMessages)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            if (character.AvailableStatPoints < request.Amount)
            {
                return BadRequest(new { message = "Not enough stat points available" });
            }

            var characterStat = character.Stats.FirstOrDefault(s => s.StatDefinitionId == request.StatId);
            if (characterStat == null)
            {
                return NotFound(new { message = "Stat not found" });
            }

            // Check if stat is locked
            if (characterStat.StatDefinition.UnlockLevel > character.Level)
            {
                return BadRequest(new { message = "This stat is still locked" });
            }

            // Apply stat points
            var newValue = Math.Min(
                characterStat.CurrentValue + request.Amount,
                characterStat.StatDefinition.MaxValue
            );
            
            var actualIncrease = newValue - characterStat.CurrentValue;
            characterStat.CurrentValue = newValue;
            characterStat.UpdatedAt = DateTime.UtcNow;

            character.AvailableStatPoints -= request.Amount;
            character.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Stat point distributed: {StatName} +{Amount} for character {CharacterId}",
                characterStat.StatDefinition.Name, actualIncrease, characterId
            );

            // Return updated profile
            var xpForNextLevel = ProgressionFormulas.CalculateXPForLevel(character.Level + 1);
            var unreadMessages = character.SystemMessages.Count(m => !m.IsRead);

            var stats = character.Stats
                .Select(s => new CharacterStatDto
                {
                    Id = s.Id,
                    StatName = s.StatDefinition.Name,
                    Category = s.StatDefinition.Category,
                    CurrentValue = Math.Round(s.CurrentValue, 2),
                    MaxValue = s.StatDefinition.MaxValue,
                    LastUsedAt = s.LastUsedAt,
                    IsLocked = s.StatDefinition.UnlockLevel > character.Level
                })
                .OrderBy(s => s.Category)
                .ThenBy(s => s.StatName)
                .ToList();

            var response = new CharacterProfileResponse
            {
                Id = character.Id,
                Name = character.Name,
                Level = character.Level,
                CurrentXP = character.CurrentXP,
                TotalXP = character.TotalXP,
                XPForNextLevel = xpForNextLevel,
                AvailableStatPoints = character.AvailableStatPoints,
                Stats = stats,
                UnreadMessages = unreadMessages
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing stat point");
            return StatusCode(500, new { message = "An error occurred while distributing stat point" });
        }
    }
}
