namespace LevelingSystem.API.Models.Entities;

public class CharacterSkill
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid SkillDefinitionId { get; set; }
    public DateTime UnlockedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int TimesUsed { get; set; } = 0;
    
    // Navigation
    public Character Character { get; set; } = null!;
    public SkillDefinition SkillDefinition { get; set; } = null!;
}
