using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LevelingSystem.API.Services;

public class StatDecayService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StatDecayService> _logger;

    public StatDecayService(
        ApplicationDbContext context,
        ILogger<StatDecayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessDailyDecay()
    {
        var characters = await _context.Characters
            .Include(c => c.Stats)
                .ThenInclude(s => s.StatDefinition)
            .ToListAsync();

        int totalDecayed = 0;

        foreach (var character in characters)
        {
            var decayed = await ProcessCharacterDecay(character.Id);
            totalDecayed += decayed;
        }

        _logger.LogInformation("Processed daily stat decay for {CharacterCount} characters, {StatCount} stats decayed", 
            characters.Count, totalDecayed);
    }

    public async Task<int> ProcessCharacterDecay(Guid characterId)
    {
        var character = await _context.Characters
            .Include(c => c.Stats)
                .ThenInclude(s => s.StatDefinition)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null)
        {
            return 0;
        }

        int decayedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var stat in character.Stats)
        {
            if (stat.CurrentValue <= 0)
            {
                continue; // Already at minimum
            }

            // Calculate days since last use
            var daysSinceUse = stat.LastUsedAt.HasValue
                ? (now - stat.LastUsedAt.Value).Days
                : 1; // If never used, decay by 1 day

            if (daysSinceUse == 0)
            {
                continue; // Used today, no decay
            }

            var oldValue = stat.CurrentValue;
            var newValue = ProgressionFormulas.CalculateStatDecay(
                stat.CurrentValue,
                stat.StatDefinition.DecayRate,
                daysSinceUse
            );

            if (newValue < oldValue)
            {
                stat.CurrentValue = newValue;
                stat.UpdatedAt = now;
                decayedCount++;

                // Log decay
                var progressLog = new ProgressLog
                {
                    Id = Guid.NewGuid(),
                    CharacterId = characterId,
                    EventType = EventType.StatDecay,
                    XPChange = 0,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        StatName = stat.StatDefinition.Name,
                        OldValue = Math.Round(oldValue, 2),
                        NewValue = Math.Round(newValue, 2),
                        DaysUnused = daysSinceUse
                    }),
                    CreatedAt = now
                };
                _context.ProgressLogs.Add(progressLog);

                // Create system message if significant decay (>5 points)
                if (oldValue - newValue >= 5)
                {
                    var message = new SystemMessage
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = characterId,
                        MessageType = MessageType.StatUnlock, // Reusing for stat changes
                        Title = "[ SYSTEM ]",
                        Content = $"Stat decay: {stat.StatDefinition.Name} decreased by {Math.Round(oldValue - newValue, 1)} points due to {daysSinceUse} days of inactivity.",
                        IsRead = false,
                        CreatedAt = now
                    };
                    _context.SystemMessages.Add(message);
                }
            }
        }

        if (decayedCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Decayed {Count} stats for character {CharacterId}", 
                decayedCount, characterId);
        }

        return decayedCount;
    }
}
