using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Services;

namespace LevelingSystem.API.Models.DTOs;

// Skill DTOs
public class SkillDefinitionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool CanUnlock { get; set; }
    public string UnlockConditions { get; set; } = string.Empty;
    public string? Effects { get; set; }
    public int? CooldownHours { get; set; }
}

public class CharacterSkillResponse
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int TimesUsed { get; set; }
    public bool OnCooldown { get; set; }
    public DateTime? CooldownEndsAt { get; set; }
}

// Analytics DTOs
public class ProgressSummaryResponse
{
    public int TotalQuests { get; set; }
    public int CompletedQuests { get; set; }
    public int FailedQuests { get; set; }
    public double SuccessRate { get; set; }
    public long TotalXP { get; set; }
    public int AverageXPPerDay { get; set; }
    public int CurrentLevel { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
}

public class DailyStatsResponse
{
    public DateOnly Date { get; set; }
    public int QuestsCompleted { get; set; }
    public int XPGained { get; set; }
    public Dictionary<string, double> StatChanges { get; set; } = new();
}

public class StatTrendResponse
{
    public string StatName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Change7Days { get; set; }
    public double Change30Days { get; set; }
}
