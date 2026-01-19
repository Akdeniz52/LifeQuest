namespace LevelingSystem.API.Models.Entities;

public enum StreakType
{
    Daily,
    Weekly
}

public class Streak
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public StreakType StreakType { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime? LastCompletedDate { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
}
