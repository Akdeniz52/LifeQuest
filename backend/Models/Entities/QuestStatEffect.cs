namespace LevelingSystem.API.Models.Entities;

public class QuestStatEffect
{
    public Guid Id { get; set; }
    public Guid QuestDefinitionId { get; set; }
    public Guid StatDefinitionId { get; set; }
    public double EffectMultiplier { get; set; } = 1.0;
    
    // Navigation
    public QuestDefinition QuestDefinition { get; set; } = null!;
    public StatDefinition StatDefinition { get; set; } = null!;
}
