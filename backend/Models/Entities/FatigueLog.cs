namespace LevelingSystem.API.Models.Entities;

public class FatigueLog
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public DateOnly Date { get; set; }
    public double FatigueLevel { get; set; } = 0;
    public int QuestsCompleted { get; set; } = 0;
    public int QuestsAssigned { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
}
