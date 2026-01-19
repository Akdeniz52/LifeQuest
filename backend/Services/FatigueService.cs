using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelingSystem.API.Services;

public class FatigueService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FatigueService> _logger;

    public FatigueService(
        ApplicationDbContext context,
        ILogger<FatigueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<double> CalculateFatigue(Guid characterId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var fatigueLog = await _context.FatigueLogs
            .FirstOrDefaultAsync(f => f.CharacterId == characterId && f.Date == today);

        if (fatigueLog == null)
        {
            return 0;
        }

        // Fatigue formula: completed / (assigned * 1.5), capped at 0.8
        if (fatigueLog.QuestsAssigned == 0)
        {
            return 0;
        }

        var fatigue = (double)fatigueLog.QuestsCompleted / (fatigueLog.QuestsAssigned * 1.5);
        return Math.Min(0.8, fatigue);
    }

    public async Task LogDailyFatigue(Guid characterId, int questsCompleted, int questsAssigned)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var fatigueLog = await _context.FatigueLogs
            .FirstOrDefaultAsync(f => f.CharacterId == characterId && f.Date == today);

        if (fatigueLog == null)
        {
            fatigueLog = new FatigueLog
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                Date = today,
                QuestsCompleted = questsCompleted,
                QuestsAssigned = questsAssigned,
                CreatedAt = DateTime.UtcNow
            };
            _context.FatigueLogs.Add(fatigueLog);
        }
        else
        {
            fatigueLog.QuestsCompleted = questsCompleted;
            fatigueLog.QuestsAssigned = questsAssigned;
        }

        // Calculate fatigue level
        fatigueLog.FatigueLevel = questsAssigned > 0 
            ? Math.Min(0.8, (double)questsCompleted / (questsAssigned * 1.5))
            : 0;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Logged fatigue for character {CharacterId}: {Fatigue:P0} ({Completed}/{Assigned})", 
            characterId, fatigueLog.FatigueLevel, questsCompleted, questsAssigned);
    }

    public async Task UpdateFatigueAfterQuestCompletion(Guid characterId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Count today's quests
        var todayStart = DateTime.UtcNow.Date;
        var questsCompleted = await _context.QuestInstances
            .CountAsync(qi => qi.CharacterId == characterId && 
                             qi.CompletedAt >= todayStart &&
                             qi.Status == QuestStatus.Completed);

        var questsAssigned = await _context.QuestInstances
            .CountAsync(qi => qi.CharacterId == characterId && 
                             qi.AssignedAt >= todayStart);

        await LogDailyFatigue(characterId, questsCompleted, questsAssigned);
    }
}
