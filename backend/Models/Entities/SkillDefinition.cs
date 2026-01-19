namespace LevelingSystem.API.Models.Entities;

public enum SkillType
{
    Passive,
    Active
}

public class SkillDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SkillType SkillType { get; set; }
    public string Category { get; set; } = string.Empty;
    public string UnlockConditions { get; set; } = "{}"; // JSONB
    public string? Effects { get; set; } // JSONB
    public int? CooldownHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
}
