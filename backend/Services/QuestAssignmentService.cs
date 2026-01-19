using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelingSystem.API.Services;

public class QuestAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestAssignmentService> _logger;

    public QuestAssignmentService(
        ApplicationDbContext context,
        ILogger<QuestAssignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AssignDailyQuests(Guid characterId)
    {
        var dailyQuests = await _context.QuestDefinitions
            .Where(q => q.AutoAssign && 
                       q.RecurrenceType == Models.Entities.RecurrenceType.Daily && 
                       q.IsActive)
            .ToListAsync();

        foreach (var quest in dailyQuests)
        {
            await AssignQuest(characterId, quest.Id);
        }

        _logger.LogInformation("Assigned {Count} daily quests to character {CharacterId}", 
            dailyQuests.Count, characterId);
    }

    public async Task AssignWeeklyQuests(Guid characterId)
    {
        var weeklyQuests = await _context.QuestDefinitions
            .Where(q => q.AutoAssign && 
                       q.RecurrenceType == Models.Entities.RecurrenceType.Weekly && 
                       q.IsActive)
            .ToListAsync();

        foreach (var quest in weeklyQuests)
        {
            await AssignQuest(characterId, quest.Id);
        }

        _logger.LogInformation("Assigned {Count} weekly quests to character {CharacterId}", 
            weeklyQuests.Count, characterId);
    }

    public async Task<QuestInstance?> AssignQuest(Guid characterId, Guid questDefinitionId)
    {
        var questDef = await _context.QuestDefinitions
            .FirstOrDefaultAsync(q => q.Id == questDefinitionId && q.IsActive);

        if (questDef == null)
        {
            _logger.LogWarning("Quest definition {QuestId} not found or inactive", questDefinitionId);
            return null;
        }

        // Check if already assigned today
        var today = DateTime.UtcNow.Date;
        var existingInstance = await _context.QuestInstances
            .FirstOrDefaultAsync(qi => 
                qi.CharacterId == characterId &&
                qi.QuestDefinitionId == questDefinitionId &&
                qi.AssignedAt >= today &&
                qi.Status == QuestStatus.Pending);

        if (existingInstance != null)
        {
            _logger.LogDebug("Quest {QuestId} already assigned to character {CharacterId} today", 
                questDefinitionId, characterId);
            return existingInstance;
        }

        // Create new instance
        var deadline = questDef.DeadlineHours.HasValue
            ? DateTime.UtcNow.AddHours(questDef.DeadlineHours.Value)
            : DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1); // End of day

        var instance = new QuestInstance
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            QuestDefinitionId = questDefinitionId,
            Status = QuestStatus.Pending,
            AssignedAt = DateTime.UtcNow,
            Deadline = deadline
        };

        _context.QuestInstances.Add(instance);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned quest {QuestId} to character {CharacterId}, deadline: {Deadline}", 
            questDefinitionId, characterId, deadline);

        return instance;
    }
}
