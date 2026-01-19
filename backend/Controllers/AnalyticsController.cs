using LevelingSystem.API.Data;
using LevelingSystem.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LevelingSystem.API.Models.Entities;

namespace LevelingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        ApplicationDbContext context,
        ILogger<AnalyticsController> logger)
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

    [HttpGet("summary")]
    public async Task<ActionResult<ProgressSummaryResponse>> GetSummary([FromQuery] int days = 30)
    {
        try
        {
            var characterId = GetCharacterId();
            var character = await _context.Characters
                .Include(c => c.Streaks)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound();
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var quests = await _context.QuestInstances
                .Where(qi => qi.CharacterId == characterId && qi.AssignedAt >= cutoffDate)
                .ToListAsync();

            var totalQuests = quests.Count;
            var completedQuests = quests.Count(q => q.Status == QuestStatus.Completed);
            var failedQuests = quests.Count(q => q.Status == QuestStatus.Failed);
            var successRate = totalQuests > 0 ? (double)completedQuests / totalQuests : 0;

            var dailyStreak = character.Streaks.FirstOrDefault(s => s.StreakType == StreakType.Daily);

            return Ok(new ProgressSummaryResponse
            {
                TotalQuests = totalQuests,
                CompletedQuests = completedQuests,
                FailedQuests = failedQuests,
                SuccessRate = Math.Round(successRate, 2),
                TotalXP = character.TotalXP,
                AverageXPPerDay = days > 0 ? (int)(character.TotalXP / days) : 0,
                CurrentLevel = character.Level,
                CurrentStreak = dailyStreak?.CurrentStreak ?? 0,
                LongestStreak = dailyStreak?.LongestStreak ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics summary");
            return StatusCode(500, new { message = "An error occurred while retrieving summary" });
        }
    }

    [HttpGet("daily")]
    public async Task<ActionResult<List<DailyStatsResponse>>> GetDailyStats([FromQuery] int days = 7)
    {
        try
        {
            var characterId = GetCharacterId();
            var cutoffDate = DateTime.UtcNow.AddDays(-days).Date;

            var progressLogs = await _context.ProgressLogs
                .Where(pl => pl.CharacterId == characterId && 
                            pl.CreatedAt >= cutoffDate &&
                            pl.EventType == EventType.QuestComplete)
                .ToListAsync();

            var dailyStats = progressLogs
                .GroupBy(pl => DateOnly.FromDateTime(pl.CreatedAt))
                .Select(g => new DailyStatsResponse
                {
                    Date = g.Key,
                    QuestsCompleted = g.Count(),
                    XPGained = g.Sum(pl => pl.XPChange),
                    StatChanges = new Dictionary<string, double>()
                })
                .OrderBy(ds => ds.Date)
                .ToList();

            return Ok(dailyStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily stats");
            return StatusCode(500, new { message = "An error occurred while retrieving daily stats" });
        }
    }

    [HttpGet("stat-trends")]
    public async Task<ActionResult<List<StatTrendResponse>>> GetStatTrends()
    {
        try
        {
            var characterId = GetCharacterId();

            var stats = await _context.CharacterStats
                .Include(cs => cs.StatDefinition)
                .Where(cs => cs.CharacterId == characterId)
                .ToListAsync();

            // For now, return current values (trend calculation would require historical data)
            var trends = stats.Select(s => new StatTrendResponse
            {
                StatName = s.StatDefinition.Name,
                CurrentValue = Math.Round(s.CurrentValue, 2),
                Change7Days = 0, // TODO: Calculate from historical data
                Change30Days = 0 // TODO: Calculate from historical data
            }).ToList();

            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stat trends");
            return StatusCode(500, new { message = "An error occurred while retrieving stat trends" });
        }
    }
}
