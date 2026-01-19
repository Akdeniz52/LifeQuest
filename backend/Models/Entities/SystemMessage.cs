namespace LevelingSystem.API.Models.Entities;

public enum MessageType
{
    LevelUp,
    QuestFail,
    Streak,
    Penalty,
    StatUnlock,
    Achievement
}

public class SystemMessage
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public MessageType MessageType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
}
