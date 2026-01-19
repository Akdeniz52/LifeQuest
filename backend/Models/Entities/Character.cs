namespace LevelingSystem.API.Models.Entities;

public class Character
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public long TotalXP { get; set; } = 0;
    public int AvailableStatPoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
    public ICollection<CharacterStat> Stats { get; set; } = new List<CharacterStat>();
    public ICollection<QuestInstance> QuestInstances { get; set; } = new List<QuestInstance>();
    public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    public ICollection<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
    public ICollection<Streak> Streaks { get; set; } = new List<Streak>();
    public ICollection<FatigueLog> FatigueLogs { get; set; } = new List<FatigueLog>();
    public ICollection<CharacterSkill> Skills { get; set; } = new List<CharacterSkill>();
}
