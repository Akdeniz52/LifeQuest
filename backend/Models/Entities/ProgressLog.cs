namespace LevelingSystem.API.Models.Entities;

public enum EventType
{
    QuestComplete,
    QuestFail,
    LevelUp,
    StatChange,
    PenaltyAssigned,
    StatDecay
}

public class ProgressLog
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid? QuestInstanceId { get; set; }
    public EventType EventType { get; set; }
    public int XPChange { get; set; } = 0;
    public int? LevelBefore { get; set; }
    public int? LevelAfter { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
    public QuestInstance? QuestInstance { get; set; }
}
