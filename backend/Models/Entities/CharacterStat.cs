namespace LevelingSystem.API.Models.Entities;

public class CharacterStat
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid StatDefinitionId { get; set; }
    public double CurrentValue { get; set; } = 0;
    public DateTime? LastUsedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
    public StatDefinition StatDefinition { get; set; } = null!;
}
