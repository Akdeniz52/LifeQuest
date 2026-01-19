using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Models.DTOs;

namespace LevelingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestsController> _logger;
    private readonly Services.QuestCompletionService _questCompletionService;
    private readonly Services.QuestAssignmentService _questAssignmentService;
    private readonly Services.StreakService _streakService;
    private readonly Services.FatigueService _fatigueService;

    public QuestsController(
        ApplicationDbContext context,
        ILogger<QuestsController> logger,
        Services.QuestCompletionService questCompletionService,
        Services.QuestAssignmentService questAssignmentService,
        Services.StreakService streakService,
        Services.FatigueService fatigueService)
    {
        _context = context;
        _logger = logger;
        _questCompletionService = questCompletionService;
        _questAssignmentService = questAssignmentService;
        _streakService = streakService;
        _fatigueService = fatigueService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return Guid.Parse(userIdClaim);
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

    [HttpGet("definitions")]
    public async Task<ActionResult<List<QuestDefinitionResponse>>> GetDefinitions()
    {
        try
        {
            var userId = GetUserId();

            var quests = await _context.QuestDefinitions
                .Include(q => q.StatEffects)
                .Where(q => q.CreatedByUserId == userId && q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            var response = quests.Select(q => new QuestDefinitionResponse
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                QuestType = q.QuestType.ToString(),
                IsMandatory = q.IsMandatory,
                BaseXP = q.BaseXP,
                DifficultyMultiplier = q.DifficultyMultiplier,
                IsActive = q.IsActive,
                CreatedAt = q.CreatedAt,
                StatEffects = q.StatEffects.Select(e => new QuestStatEffectDto
                {
                    StatDefinitionId = e.StatDefinitionId,
                    EffectMultiplier = e.EffectMultiplier
                }).ToList()
            }).ToList();

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quest definitions");
            return StatusCode(500, new { message = "An error occurred while retrieving quests" });
        }
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<QuestDefinitionResponse>> CreateDefinition(CreateQuestRequest request)
    {
        try
        {
            var userId = GetUserId();

            // Validate quest type
            if (!Enum.TryParse<QuestType>(request.QuestType, out var questType))
            {
                return BadRequest(new { message = "Invalid quest type. Use: Daily, Weekly, Custom, or Penalty" });
            }

            // Validate stat effects
            if (request.StatEffects.Any())
            {
                var statIds = request.StatEffects.Select(e => e.StatDefinitionId).ToList();
                var validStats = await _context.StatDefinitions
                    .Where(s => statIds.Contains(s.Id) && s.IsActive)
                    .CountAsync();

                if (validStats != statIds.Count)
                {
                    return BadRequest(new { message = "One or more stat IDs are invalid" });
                }
            }

            var quest = new QuestDefinition
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = userId,
                Title = request.Title,
                Description = request.Description,
                QuestType = questType,
                IsMandatory = request.IsMandatory,
                BaseXP = request.BaseXP,
                DifficultyMultiplier = request.DifficultyMultiplier,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuestDefinitions.Add(quest);

            // Add stat effects
            foreach (var effect in request.StatEffects)
            {
                var statEffect = new QuestStatEffect
                {
                    Id = Guid.NewGuid(),
                    QuestDefinitionId = quest.Id,
                    StatDefinitionId = effect.StatDefinitionId,
                    EffectMultiplier = effect.EffectMultiplier
                };
                _context.QuestStatEffects.Add(statEffect);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Quest created: {QuestId} by user {UserId}", quest.Id, userId);

            var response = new QuestDefinitionResponse
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = quest.Description,
                QuestType = quest.QuestType.ToString(),
                IsMandatory = quest.IsMandatory,
                BaseXP = quest.BaseXP,
                DifficultyMultiplier = quest.DifficultyMultiplier,
                IsActive = quest.IsActive,
                CreatedAt = quest.CreatedAt,
                StatEffects = request.StatEffects
            };

            return CreatedAtAction(nameof(GetDefinitions), new { id = quest.Id }, response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quest definition");
            return StatusCode(500, new { message = "An error occurred while creating quest" });
        }
    }

    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<QuestDefinitionResponse>> UpdateDefinition(Guid id, UpdateQuestRequest request)
    {
        try
        {
            var userId = GetUserId();

            var quest = await _context.QuestDefinitions
                .Include(q => q.StatEffects)
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedByUserId == userId);

            if (quest == null)
            {
                return NotFound(new { message = "Quest not found" });
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Title))
                quest.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.Description))
                quest.Description = request.Description;

            if (request.IsMandatory.HasValue)
                quest.IsMandatory = request.IsMandatory.Value;

            if (request.BaseXP.HasValue)
                quest.BaseXP = request.BaseXP.Value;

            if (request.DifficultyMultiplier.HasValue)
                quest.DifficultyMultiplier = request.DifficultyMultiplier.Value;

            if (request.IsActive.HasValue)
                quest.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Quest updated: {QuestId}", quest.Id);

            var response = new QuestDefinitionResponse
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = quest.Description,
                QuestType = quest.QuestType.ToString(),
                IsMandatory = quest.IsMandatory,
                BaseXP = quest.BaseXP,
                DifficultyMultiplier = quest.DifficultyMultiplier,
                IsActive = quest.IsActive,
                CreatedAt = quest.CreatedAt,
                StatEffects = quest.StatEffects.Select(e => new QuestStatEffectDto
                {
                    StatDefinitionId = e.StatDefinitionId,
                    EffectMultiplier = e.EffectMultiplier
                }).ToList()
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quest definition");
            return StatusCode(500, new { message = "An error occurred while updating quest" });
        }
    }

    [HttpDelete("definitions/{id}")]
    public async Task<IActionResult> DeleteDefinition(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var quest = await _context.QuestDefinitions
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedByUserId == userId);

            if (quest == null)
            {
                return NotFound(new { message = "Quest not found" });
            }

            // Soft delete
            quest.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Quest soft-deleted: {QuestId}", quest.Id);

            return Ok(new { message = "Quest deactivated successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quest definition");
            return StatusCode(500, new { message = "An error occurred while deleting quest" });
        }
    }

    [HttpGet("today")]
    public async Task<ActionResult<List<QuestInstanceResponse>>> GetTodayQuests()
    {
        try
        {
            var characterId = GetCharacterId();
            var today = DateTime.UtcNow.Date;

            // Return both Pending and Completed quests for today
            var instances = await _context.QuestInstances
                .Include(qi => qi.QuestDefinition)
                .Where(qi => qi.CharacterId == characterId &&
                            qi.AssignedAt >= today &&
                            (qi.Status == QuestStatus.Pending || qi.Status == QuestStatus.Completed))
                .OrderBy(qi => qi.Status) // Pending first, then Completed
                .ThenBy(qi => qi.Deadline)
                .ToListAsync();

            _logger.LogInformation("Found {Count} quests for character {CharacterId}", instances.Count, characterId);

            var response = instances.Select(qi => new QuestInstanceResponse
            {
                Id = qi.Id,
                QuestDefinitionId = qi.QuestDefinitionId,
                Title = qi.QuestDefinition.Title,
                Description = qi.QuestDefinition.Description,
                QuestType = qi.QuestDefinition.QuestType.ToString(),
                IsMandatory = qi.QuestDefinition.IsMandatory,
                BaseXP = qi.QuestDefinition.BaseXP,
                Status = qi.Status.ToString(),
                AssignedAt = qi.AssignedAt,
                Deadline = qi.Deadline,
                CompletedAt = qi.CompletedAt
            }).ToList();

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's quests");
            return StatusCode(500, new { message = "An error occurred while retrieving quests" });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<QuestInstanceResponse>>> GetActiveQuests()
    {
        try
        {
            var characterId = GetCharacterId();

            var instances = await _context.QuestInstances
                .Include(qi => qi.QuestDefinition)
                .Where(qi => qi.CharacterId == characterId &&
                            qi.Status == QuestStatus.Pending)
                .OrderBy(qi => qi.Deadline)
                .ToListAsync();

            var response = instances.Select(qi => new QuestInstanceResponse
            {
                Id = qi.Id,
                QuestDefinitionId = qi.QuestDefinitionId,
                Title = qi.QuestDefinition.Title,
                Description = qi.QuestDefinition.Description,
                QuestType = qi.QuestDefinition.QuestType.ToString(),
                IsMandatory = qi.QuestDefinition.IsMandatory,
                BaseXP = qi.QuestDefinition.BaseXP,
                Status = qi.Status.ToString(),
                AssignedAt = qi.AssignedAt,
                Deadline = qi.Deadline,
                CompletedAt = qi.CompletedAt
            }).ToList();

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active quests");
            return StatusCode(500, new { message = "An error occurred while retrieving quests" });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<CompleteQuestResponse>> CompleteQuest(Guid id)
    {
        try
        {
            var characterId = GetCharacterId();
            var result = await _questCompletionService.CompleteQuest(id, characterId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing quest {QuestId}", id);
            return StatusCode(500, new { message = "An error occurred while completing quest" });
        }
    }

    [HttpPost("{id}/fail")]
    public async Task<ActionResult<CompleteQuestResponse>> FailQuest(Guid id)
    {
        try
        {
            var characterId = GetCharacterId();
            var result = await _questCompletionService.FailQuest(id, characterId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing quest {QuestId}", id);
            return StatusCode(500, new { message = "An error occurred while failing quest" });
        }
    }

    [HttpPost("assign/{questDefinitionId}")]
    public async Task<ActionResult<AssignQuestResponse>> AssignQuest(Guid questDefinitionId)
    {
        try
        {
            var characterId = GetCharacterId();
            var instance = await _questAssignmentService.AssignQuest(characterId, questDefinitionId);

            if (instance == null)
            {
                return NotFound(new { message = "Quest definition not found or inactive" });
            }

            var questDef = await _context.QuestDefinitions
                .FirstOrDefaultAsync(q => q.Id == questDefinitionId);

            return Ok(new AssignQuestResponse
            {
                Id = instance.Id,
                QuestDefinitionId = instance.QuestDefinitionId,
                Title = questDef?.Title ?? "",
                Status = instance.Status.ToString(),
                AssignedAt = instance.AssignedAt,
                Deadline = instance.Deadline ?? DateTime.UtcNow.AddDays(1)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning quest {QuestId}", questDefinitionId);
            return StatusCode(500, new { message = "An error occurred while assigning quest" });
        }
    }

    [HttpGet("streaks")]
    public async Task<ActionResult<AllStreaksResponse>> GetStreaks()
    {
        try
        {
            var characterId = GetCharacterId();

            var dailyStreak = await _context.Streaks
                .FirstOrDefaultAsync(s => s.CharacterId == characterId && s.StreakType == StreakType.Daily);

            var weeklyStreak = await _context.Streaks
                .FirstOrDefaultAsync(s => s.CharacterId == characterId && s.StreakType == StreakType.Weekly);

            return Ok(new AllStreaksResponse
            {
                Daily = dailyStreak != null ? new StreakResponse
                {
                    Type = dailyStreak.StreakType,
                    CurrentStreak = dailyStreak.CurrentStreak,
                    LongestStreak = dailyStreak.LongestStreak,
                    LastCompletedDate = dailyStreak.LastCompletedDate
                } : null,
                Weekly = weeklyStreak != null ? new StreakResponse
                {
                    Type = weeklyStreak.StreakType,
                    CurrentStreak = weeklyStreak.CurrentStreak,
                    LongestStreak = weeklyStreak.LongestStreak,
                    LastCompletedDate = weeklyStreak.LastCompletedDate
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving streaks");
            return StatusCode(500, new { message = "An error occurred while retrieving streaks" });
        }
    }

    [HttpGet("fatigue")]
    public async Task<ActionResult<FatigueResponse>> GetFatigue()
    {
        try
        {
            var characterId = GetCharacterId();
            var fatigue = await _fatigueService.CalculateFatigue(characterId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var fatigueLog = await _context.FatigueLogs
                .FirstOrDefaultAsync(f => f.CharacterId == characterId && f.Date == today);

            var recommendation = fatigue < 0.3 ? "You're doing great! Keep it up!" :
                                fatigue < 0.6 ? "You're working hard. Consider taking a break." :
                                "High fatigue detected. Rest is recommended.";

            return Ok(new FatigueResponse
            {
                CurrentFatigue = Math.Round(fatigue, 2),
                QuestsCompletedToday = fatigueLog?.QuestsCompleted ?? 0,
                QuestsAssignedToday = fatigueLog?.QuestsAssigned ?? 0,
                XPPenalty = Math.Round(fatigue, 2),
                Recommendation = recommendation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fatigue");
            return StatusCode(500, new { message = "An error occurred while retrieving fatigue" });
        }
    }

    [HttpGet("messages")]
    public async Task<ActionResult<MessagesResponse>> GetMessages(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 20)
    {
        try
        {
            var characterId = GetCharacterId();

            var query = _context.SystemMessages
                .Where(m => m.CharacterId == characterId);

            if (unreadOnly)
            {
                query = query.Where(m => !m.IsRead);
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();

            var unreadCount = await _context.SystemMessages
                .CountAsync(m => m.CharacterId == characterId && !m.IsRead);

            return Ok(new MessagesResponse
            {
                Messages = messages.Select(m => new SystemMessageResponse
                {
                    Id = m.Id,
                    MessageType = m.MessageType.ToString(),
                    Title = m.Title,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                }).ToList(),
                UnreadCount = unreadCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages");
            return StatusCode(500, new { message = "An error occurred while retrieving messages" });
        }
    }

    [HttpPost("messages/mark-all-read")]
    public async Task<IActionResult> MarkAllMessagesRead()
    {
        try
        {
            var characterId = GetCharacterId();

            var unreadMessages = await _context.SystemMessages
                .Where(m => m.CharacterId == characterId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{unreadMessages.Count} messages marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            return StatusCode(500, new { message = "An error occurred while marking messages as read" });
        }
    }
}
