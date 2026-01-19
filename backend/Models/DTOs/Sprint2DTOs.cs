using LevelingSystem.API.Models.Entities;

namespace LevelingSystem.API.Models.DTOs;

// Streak DTOs
public class StreakResponse
{
    public StreakType Type { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastCompletedDate { get; set; }
}

public class AllStreaksResponse
{
    public StreakResponse? Daily { get; set; }
    public StreakResponse? Weekly { get; set; }
}

// Fatigue DTOs
public class FatigueResponse
{
    public double CurrentFatigue { get; set; }
    public int QuestsCompletedToday { get; set; }
    public int QuestsAssignedToday { get; set; }
    public double XPPenalty { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

// Message DTOs
public class SystemMessageResponse
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessagesResponse
{
    public List<SystemMessageResponse> Messages { get; set; } = new();
    public int UnreadCount { get; set; }
}

// Quest Assignment DTO
public class AssignQuestResponse
{
    public Guid Id { get; set; }
    public Guid QuestDefinitionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime Deadline { get; set; }
}
