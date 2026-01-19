namespace LevelingSystem.API.Models.Entities;

public enum QuestStatus
{
    Pending,
    Completed,
    Failed,
    Expired
}

public class QuestInstance
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid QuestDefinitionId { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Pending;
    public DateTime AssignedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
    public QuestDefinition QuestDefinition { get; set; } = null!;
}
