namespace LevelingSystem.API.Models.Entities;

public class StatDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Physical, Mental, Behavioral, Meta
    public string Description { get; set; } = string.Empty;
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;
    public double DecayRate { get; set; } = 0.02; // 2% per day default
    public bool IsActive { get; set; } = true;
    public int UnlockLevel { get; set; } = 1;
    
    // Navigation
    public ICollection<CharacterStat> CharacterStats { get; set; } = new List<CharacterStat>();
    public ICollection<QuestStatEffect> QuestStatEffects { get; set; } = new List<QuestStatEffect>();
}
