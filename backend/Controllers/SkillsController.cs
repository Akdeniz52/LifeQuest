using LevelingSystem.API.Data;
using LevelingSystem.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LevelingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly Services.SkillService _skillService;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(
        ApplicationDbContext context,
        Services.SkillService skillService,
        ILogger<SkillsController> logger)
    {
        _context = context;
        _skillService = skillService;
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

    [HttpGet("available")]
    public async Task<ActionResult<List<SkillDefinitionResponse>>> GetAvailableSkills()
    {
        try
        {
            var characterId = GetCharacterId();
            var skills = await _skillService.GetAvailableSkills(characterId);

            var response = skills.Select(s => new SkillDefinitionResponse
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                SkillType = s.SkillType.ToString(),
                Category = s.Category,
                CanUnlock = true,
                UnlockConditions = s.UnlockConditions,
                Effects = s.Effects,
                CooldownHours = s.CooldownHours
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available skills");
            return StatusCode(500, new { message = "An error occurred while retrieving skills" });
        }
    }

    [HttpGet("unlocked")]
    public async Task<ActionResult<List<CharacterSkillResponse>>> GetUnlockedSkills()
    {
        try
        {
            var characterId = GetCharacterId();

            var skills = await _context.CharacterSkills
                .Include(cs => cs.SkillDefinition)
                .Where(cs => cs.CharacterId == characterId)
                .ToListAsync();

            var response = skills.Select(cs =>
            {
                var onCooldown = false;
                DateTime? cooldownEndsAt = null;

                if (cs.SkillDefinition.SkillType == Models.Entities.SkillType.Active &&
                    cs.SkillDefinition.CooldownHours.HasValue &&
                    cs.LastUsedAt.HasValue)
                {
                    cooldownEndsAt = cs.LastUsedAt.Value.AddHours(cs.SkillDefinition.CooldownHours.Value);
                    onCooldown = DateTime.UtcNow < cooldownEndsAt;
                }

                return new CharacterSkillResponse
                {
                    Id = cs.Id,
                    SkillId = cs.SkillDefinitionId,
                    SkillName = cs.SkillDefinition.Name,
                    SkillType = cs.SkillDefinition.SkillType.ToString(),
                    UnlockedAt = cs.UnlockedAt,
                    LastUsedAt = cs.LastUsedAt,
                    TimesUsed = cs.TimesUsed,
                    OnCooldown = onCooldown,
                    CooldownEndsAt = cooldownEndsAt
                };
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unlocked skills");
            return StatusCode(500, new { message = "An error occurred while retrieving skills" });
        }
    }

    [HttpPost("{skillId}/unlock")]
    public async Task<ActionResult<CharacterSkillResponse>> UnlockSkill(Guid skillId)
    {
        try
        {
            var characterId = GetCharacterId();
            var characterSkill = await _skillService.UnlockSkill(characterId, skillId);

            if (characterSkill == null)
            {
                return BadRequest(new { message = "Cannot unlock skill. Requirements not met or already unlocked." });
            }

            var skill = await _context.SkillDefinitions.FindAsync(skillId);

            return Ok(new CharacterSkillResponse
            {
                Id = characterSkill.Id,
                SkillId = characterSkill.SkillDefinitionId,
                SkillName = skill?.Name ?? "",
                SkillType = skill?.SkillType.ToString() ?? "",
                UnlockedAt = characterSkill.UnlockedAt,
                TimesUsed = characterSkill.TimesUsed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking skill {SkillId}", skillId);
            return StatusCode(500, new { message = "An error occurred while unlocking skill" });
        }
    }

    [HttpPost("{skillId}/use")]
    public async Task<IActionResult> UseSkill(Guid skillId)
    {
        try
        {
            var characterId = GetCharacterId();
            var success = await _skillService.UseActiveSkill(characterId, skillId);

            if (!success)
            {
                return BadRequest(new { message = "Cannot use skill. Not unlocked, not active, or on cooldown." });
            }

            return Ok(new { message = "Skill used successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using skill {SkillId}", skillId);
            return StatusCode(500, new { message = "An error occurred while using skill" });
        }
    }
}
