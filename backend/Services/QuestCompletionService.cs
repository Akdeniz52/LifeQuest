using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Models.DTOs;
using LevelingSystem.API.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LevelingSystem.API.Services;

public class QuestCompletionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestCompletionService> _logger;

    public QuestCompletionService(
        ApplicationDbContext context,
        ILogger<QuestCompletionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CompleteQuestResponse> CompleteQuest(Guid questInstanceId, Guid characterId)
    {
        var questInstance = await _context.QuestInstances
            .Include(qi => qi.QuestDefinition)
                .ThenInclude(qd => qd.StatEffects)
                    .ThenInclude(se => se.StatDefinition)
            .Include(qi => qi.Character)
                .ThenInclude(c => c.Stats)
                    .ThenInclude(s => s.StatDefinition)
            .FirstOrDefaultAsync(qi => qi.Id == questInstanceId && qi.CharacterId == characterId);

        if (questInstance == null)
        {
            throw new InvalidOperationException("Quest instance not found");
        }

        if (questInstance.Status != QuestStatus.Pending)
        {
            throw new InvalidOperationException($"Quest is already {questInstance.Status}");
        }

        var character = questInstance.Character;
        var questDef = questInstance.QuestDefinition;

        // Calculate XP (fatigue = 0 for now, will be implemented in Sprint 2)
        var earnedXP = ProgressionFormulas.CalculateEarnedXP(
            questDef.BaseXP,
            questDef.DifficultyMultiplier,
            fatigue: 0
        );

        var oldLevel = character.Level;
        var statChanges = new List<StatChangeDto>();

        // Update stats
        foreach (var statEffect in questDef.StatEffects)
        {
            var characterStat = character.Stats
                .FirstOrDefault(s => s.StatDefinitionId == statEffect.StatDefinitionId);

            if (characterStat != null)
            {
                var oldValue = characterStat.CurrentValue;
                var statGain = ProgressionFormulas.CalculateStatGain(
                    questDef.BaseXP,
                    statEffect.EffectMultiplier
                );

                characterStat.CurrentValue = Math.Min(
                    characterStat.CurrentValue + statGain,
                    characterStat.StatDefinition.MaxValue
                );
                characterStat.LastUsedAt = DateTime.UtcNow;
                characterStat.UpdatedAt = DateTime.UtcNow;

                statChanges.Add(new StatChangeDto
                {
                    StatName = characterStat.StatDefinition.Name,
                    OldValue = Math.Round(oldValue, 2),
                    NewValue = Math.Round(characterStat.CurrentValue, 2),
                    Change = Math.Round(statGain, 2)
                });
            }
        }

        // Update character XP and level
        character.TotalXP += earnedXP;
        character.CurrentXP += earnedXP;

        var newLevel = ProgressionFormulas.CalculateLevelFromXP(character.TotalXP);
        var leveledUp = newLevel > oldLevel;

        if (leveledUp)
        {
            character.Level = newLevel;
            character.CurrentXP = (int)(character.TotalXP - ProgressionFormulas.CalculateTotalXPForLevel(newLevel));
            
            // Grant stat points for level up
            var levelsGained = newLevel - oldLevel;
            character.AvailableStatPoints += levelsGained;

            // Unlock new stats
            await UnlockNewStats(character);
        }

        character.UpdatedAt = DateTime.UtcNow;

        // Update quest instance
        questInstance.Status = QuestStatus.Completed;
        questInstance.CompletedAt = DateTime.UtcNow;

        // Increment completion count for the quest definition
        questDef.CompletionCount++;

        // Create progress log
        var progressLog = new ProgressLog
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            QuestInstanceId = questInstanceId,
            EventType = EventType.QuestComplete,
            XPChange = earnedXP,
            LevelBefore = oldLevel,
            LevelAfter = character.Level,
            Metadata = JsonSerializer.Serialize(new
            {
                QuestTitle = questDef.Title,
                StatChanges = statChanges
            }),
            CreatedAt = DateTime.UtcNow
        };
        _context.ProgressLogs.Add(progressLog);

        // Create system message
        string messageContent;
        if (leveledUp)
        {
            messageContent = $"Quest '{questDef.Title}' completed!\n+{earnedXP} XP\nLevel Up! {oldLevel} → {newLevel}";
            
            // Add level up log
            var levelUpLog = new ProgressLog
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                EventType = EventType.LevelUp,
                XPChange = 0,
                LevelBefore = oldLevel,
                LevelAfter = newLevel,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProgressLogs.Add(levelUpLog);
        }
        else
        {
            messageContent = $"Quest '{questDef.Title}' completed!\n+{earnedXP} XP";
        }

        var systemMessage = new SystemMessage
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            MessageType = leveledUp ? MessageType.LevelUp : MessageType.Achievement,
            Title = "[ SYSTEM ]",
            Content = messageContent,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.SystemMessages.Add(systemMessage);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Quest completed: {QuestId} by character {CharacterId}, XP: {XP}, LevelUp: {LevelUp}",
            questInstanceId, characterId, earnedXP, leveledUp
        );

        return new CompleteQuestResponse
        {
            Success = true,
            XPGained = earnedXP,
            LeveledUp = leveledUp,
            NewLevel = leveledUp ? newLevel : null,
            StatChanges = statChanges,
            SystemMessage = messageContent
        };
    }

    public async Task<CompleteQuestResponse> FailQuest(Guid questInstanceId, Guid characterId)
    {
        var questInstance = await _context.QuestInstances
            .Include(qi => qi.QuestDefinition)
            .Include(qi => qi.Character)
                .ThenInclude(c => c.Stats)
                    .ThenInclude(s => s.StatDefinition)
            .FirstOrDefaultAsync(qi => qi.Id == questInstanceId && qi.CharacterId == characterId);

        if (questInstance == null)
        {
            throw new InvalidOperationException("Quest instance not found");
        }

        if (questInstance.Status != QuestStatus.Pending)
        {
            throw new InvalidOperationException($"Quest is already {questInstance.Status}");
        }

        var character = questInstance.Character;
        var questDef = questInstance.QuestDefinition;

        // Reduce Discipline stat
        var disciplineStat = character.Stats
            .FirstOrDefault(s => s.StatDefinition.Name == "Discipline");

        var statChanges = new List<StatChangeDto>();

        if (disciplineStat != null)
        {
            var oldValue = disciplineStat.CurrentValue;
            var penalty = 2.0; // Fixed penalty for now
            disciplineStat.CurrentValue = Math.Max(0, disciplineStat.CurrentValue - penalty);
            disciplineStat.UpdatedAt = DateTime.UtcNow;

            statChanges.Add(new StatChangeDto
            {
                StatName = "Discipline",
                OldValue = Math.Round(oldValue, 2),
                NewValue = Math.Round(disciplineStat.CurrentValue, 2),
                Change = -penalty
            });
        }

        character.UpdatedAt = DateTime.UtcNow;

        // Update quest instance
        questInstance.Status = QuestStatus.Failed;
        questInstance.FailedAt = DateTime.UtcNow;

        // Create progress log
        var progressLog = new ProgressLog
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            QuestInstanceId = questInstanceId,
            EventType = EventType.QuestFail,
            XPChange = 0,
            Metadata = JsonSerializer.Serialize(new
            {
                QuestTitle = questDef.Title,
                IsMandatory = questDef.IsMandatory,
                StatChanges = statChanges
            }),
            CreatedAt = DateTime.UtcNow
        };
        _context.ProgressLogs.Add(progressLog);

        // Create system message
        var messageContent = $"Quest '{questDef.Title}' failed.\nDiscipline -2";
        if (questDef.IsMandatory)
        {
            messageContent += "\n⚠️ Penalty quest will be assigned.";
        }

        var systemMessage = new SystemMessage
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            MessageType = MessageType.QuestFail,
            Title = "[ SYSTEM ]",
            Content = messageContent,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.SystemMessages.Add(systemMessage);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Quest failed: {QuestId} by character {CharacterId}, Mandatory: {IsMandatory}",
            questInstanceId, characterId, questDef.IsMandatory
        );

        return new CompleteQuestResponse
        {
            Success = true,
            XPGained = 0,
            LeveledUp = false,
            StatChanges = statChanges,
            SystemMessage = messageContent
        };
    }

    private async Task UnlockNewStats(Character character)
    {
        var unlockedStats = await _context.StatDefinitions
            .Where(s => s.IsActive && s.UnlockLevel == character.Level)
            .ToListAsync();

        foreach (var statDef in unlockedStats)
        {
            // Check if stat already exists
            var exists = character.Stats.Any(s => s.StatDefinitionId == statDef.Id);
            if (!exists)
            {
                var newStat = new CharacterStat
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    StatDefinitionId = statDef.Id,
                    CurrentValue = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CharacterStats.Add(newStat);

                // Create unlock message
                var unlockMessage = new SystemMessage
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    MessageType = MessageType.StatUnlock,
                    Title = "[ SYSTEM ]",
                    Content = $"New stat unlocked: {statDef.Name}\n{statDef.Description}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.SystemMessages.Add(unlockMessage);

                _logger.LogInformation(
                    "Stat unlocked: {StatName} for character {CharacterId} at level {Level}",
                    statDef.Name, character.Id, character.Level
                );
            }
        }
    }
}
