namespace LevelingSystem.API.Models.DTOs;

public class CharacterProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public long TotalXP { get; set; }
    public int XPForNextLevel { get; set; }
    public int AvailableStatPoints { get; set; }
    public List<CharacterStatDto> Stats { get; set; } = new();
    public int UnreadMessages { get; set; }
}

public class DistributeStatPointRequest
{
    public Guid StatId { get; set; }
    public int Amount { get; set; } = 1;
}

public class CharacterStatDto
{
    public Guid Id { get; set; }
    public string StatName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsLocked { get; set; }
}

public class StatDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public double DecayRate { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UnlockLevel { get; set; }
    public bool IsLocked { get; set; }
}
