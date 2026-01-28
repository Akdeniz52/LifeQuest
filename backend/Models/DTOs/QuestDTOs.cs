using LevelingSystem.API.Models.Entities;

namespace LevelingSystem.API.Models.DTOs;

public class CreateQuestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QuestType { get; set; } = "Custom"; // Daily, Weekly, Custom, Penalty
    public bool IsMandatory { get; set; } = false;
    public int BaseXP { get; set; }
    public double DifficultyMultiplier { get; set; } = 1.0;
    public List<QuestStatEffectDto> StatEffects { get; set; } = new();
}

public class UpdateQuestRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsMandatory { get; set; }
    public int? BaseXP { get; set; }
    public double? DifficultyMultiplier { get; set; }
    public bool? IsActive { get; set; }
}

public class QuestStatEffectDto
{
    public Guid StatDefinitionId { get; set; }
    public double EffectMultiplier { get; set; } = 1.0;
}

public class QuestDefinitionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QuestType { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public int BaseXP { get; set; }
    public double DifficultyMultiplier { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuestStatEffectDto> StatEffects { get; set; } = new();
}

public class QuestInstanceResponse
{
    public Guid Id { get; set; }
    public Guid QuestDefinitionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QuestType { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public int BaseXP { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CompletionCount { get; set; } = 0;
}

public class CompleteQuestResponse
{
    public bool Success { get; set; }
    public int XPGained { get; set; }
    public bool LeveledUp { get; set; }
    public int? NewLevel { get; set; }
    public List<StatChangeDto> StatChanges { get; set; } = new();
    public string? SystemMessage { get; set; }
}

public class StatChangeDto
{
    public string StatName { get; set; } = string.Empty;
    public double OldValue { get; set; }
    public double NewValue { get; set; }
    public double Change { get; set; }
}
