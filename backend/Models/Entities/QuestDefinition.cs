namespace LevelingSystem.API.Models.Entities;

public enum RecurrenceType
{
    None,
    Daily,
    Weekly,
    Monthly
}

public enum QuestType
{
    Daily,
    Weekly,
    Monthly,
    Custom,
    Challenge,
    Penalty
}

public class QuestDefinition
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType QuestType { get; set; }
    public bool IsMandatory { get; set; } = false;
    public int BaseXP { get; set; }
    public double DifficultyMultiplier { get; set; } = 1.0;
    public bool IsActive { get; set; } = true;
    public RecurrenceType? RecurrenceType { get; set; }
    public bool AutoAssign { get; set; } = false;
    public int? DeadlineHours { get; set; }
    public string? WeeklyDays { get; set; } // Comma-separated day numbers (0=Sunday, 1=Monday, ..., 6=Saturday)
    public int? MonthlyDay { get; set; } // Day of month (1-31) for monthly quests
    public DateTime CreatedAt { get; set; }
    public int CompletionCount { get; set; } = 0; // Track how many times this quest has been completed
    
    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<QuestStatEffect> StatEffects { get; set; } = new List<QuestStatEffect>();
    public ICollection<QuestInstance> QuestInstances { get; set; } = new List<QuestInstance>();
}
