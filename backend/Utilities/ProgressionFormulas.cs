namespace LevelingSystem.API.Utilities;

public static class ProgressionFormulas
{
    /// <summary>
    /// Calculate XP required to reach a specific level
    /// Formula: 100 * level^1.5
    /// </summary>
    public static int CalculateXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return (int)(100 * Math.Pow(level, 1.5));
    }

    /// <summary>
    /// Calculate total XP required from level 1 to target level
    /// </summary>
    public static long CalculateTotalXPForLevel(int level)
    {
        long total = 0;
        for (int i = 2; i <= level; i++)
        {
            total += CalculateXPForLevel(i);
        }
        return total;
    }

    /// <summary>
    /// Calculate level from total XP
    /// </summary>
    public static int CalculateLevelFromXP(long totalXP)
    {
        int level = 1;
        long xpAccumulated = 0;

        while (true)
        {
            int xpForNextLevel = CalculateXPForLevel(level + 1);
            if (xpAccumulated + xpForNextLevel > totalXP)
                break;
            
            xpAccumulated += xpForNextLevel;
            level++;
        }

        return level;
    }

    /// <summary>
    /// Calculate earned XP with fatigue modifier
    /// Formula: base_xp * difficulty_multiplier * (1 - fatigue)
    /// </summary>
    public static int CalculateEarnedXP(int baseXP, double difficultyMultiplier, double fatigue = 0)
    {
        return (int)(baseXP * difficultyMultiplier * (1 - Math.Clamp(fatigue, 0, 0.8)));
    }

    /// <summary>
    /// Calculate stat gain from quest completion
    /// Formula: base_xp * stat_effect_multiplier
    /// </summary>
    public static double CalculateStatGain(int baseXP, double statEffectMultiplier)
    {
        return baseXP * statEffectMultiplier * 0.1; // 0.1 is scaling factor
    }

    /// <summary>
    /// Calculate stat decay based on days unused
    /// Formula: stat_value * decay_rate * days_unused
    /// </summary>
    public static double CalculateStatDecay(double currentValue, double decayRate, int daysUnused)
    {
        double decay = currentValue * decayRate * daysUnused;
        return Math.Max(0, currentValue - decay);
    }
}
