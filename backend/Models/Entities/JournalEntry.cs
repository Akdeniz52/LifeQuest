using System.ComponentModel.DataAnnotations;

namespace LevelingSystem.API.Models.Entities;

public class JournalEntry
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public DateTime Date { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Character Character { get; set; } = null!;
}
