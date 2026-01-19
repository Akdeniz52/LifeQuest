namespace LevelingSystem.API.Models.Entities;

public enum RecurrenceType
{
    None,
    Daily,
    Weekly
}

public enum QuestType
{
    Daily,
    Weekly,
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
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<QuestStatEffect> StatEffects { get; set; } = new List<QuestStatEffect>();
    public ICollection<QuestInstance> QuestInstances { get; set; } = new List<QuestInstance>();
}
