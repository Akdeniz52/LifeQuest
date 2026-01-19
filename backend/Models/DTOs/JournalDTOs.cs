namespace LevelingSystem.API.Models.DTOs;

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateJournalEntryRequest
{
    public string Content { get; set; } = string.Empty;
}
