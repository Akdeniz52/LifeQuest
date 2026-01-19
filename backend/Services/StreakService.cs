using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelingSystem.API.Services;

public class StreakService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StreakService> _logger;

    public StreakService(
        ApplicationDbContext context,
        ILogger<StreakService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UpdateStreak(Guid characterId, StreakType type)
    {
        var streak = await _context.Streaks
            .FirstOrDefaultAsync(s => s.CharacterId == characterId && s.StreakType == type);

        if (streak == null)
        {
            // Create new streak
            streak = new Streak
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                StreakType = type,
                CurrentStreak = 1,
                LongestStreak = 1,
                LastCompletedDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Streaks.Add(streak);
        }
        else
        {
            var today = DateTime.UtcNow.Date;
            var lastCompleted = streak.LastCompletedDate?.Date;

            if (lastCompleted == today)
            {
                // Already completed today
                return;
            }

            if (lastCompleted == today.AddDays(-1))
            {
                // Consecutive day
                streak.CurrentStreak++;
            }
            else
            {
                // Streak broken
                streak.CurrentStreak = 1;
            }

            if (streak.CurrentStreak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.CurrentStreak;
            }

            streak.LastCompletedDate = DateTime.UtcNow;
            streak.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated {Type} streak for character {CharacterId}: {CurrentStreak}", 
            type, characterId, streak.CurrentStreak);
    }

    public async Task<int> GetCurrentStreak(Guid characterId, StreakType type)
    {
        var streak = await _context.Streaks
            .FirstOrDefaultAsync(s => s.CharacterId == characterId && s.StreakType == type);

        return streak?.CurrentStreak ?? 0;
    }

    public async Task ResetStreak(Guid characterId, StreakType type)
    {
        var streak = await _context.Streaks
            .FirstOrDefaultAsync(s => s.CharacterId == characterId && s.StreakType == type);

        if (streak != null)
        {
            streak.CurrentStreak = 0;
            streak.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reset {Type} streak for character {CharacterId}", 
                type, characterId);
        }
    }

    public async Task CheckAndResetStreaks()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        
        var streaksToReset = await _context.Streaks
            .Where(s => s.LastCompletedDate < yesterday && s.CurrentStreak > 0)
            .ToListAsync();

        foreach (var streak in streaksToReset)
        {
            streak.CurrentStreak = 0;
            streak.UpdatedAt = DateTime.UtcNow;
        }

        if (streaksToReset.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reset {Count} streaks due to inactivity", streaksToReset.Count);
        }
    }
}
