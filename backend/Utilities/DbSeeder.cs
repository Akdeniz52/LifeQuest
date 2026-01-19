using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LevelingSystem.API.Utilities;

public static class DbSeeder
{
    public static async Task SeedDefaultStats(ApplicationDbContext context)
    {
        // Check if stats already exist
        var existingStatCount = await context.StatDefinitions.CountAsync();
        if (existingStatCount > 0)
        {
            Log.Information("Stats already seeded ({Count} stats found), skipping", existingStatCount);
            return;
        }

        var defaultStats = new List<StatDefinition>
        {
            // Physical Stats
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Strength",
                Category = "Physical",
                Description = "Physical power and fitness from exercise and sports",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.03,
                IsActive = true,
                UnlockLevel = 1
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Stamina",
                Category = "Physical",
                Description = "Endurance and ability to work for extended periods",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.025,
                IsActive = true,
                UnlockLevel = 1
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Agility",
                Category = "Physical",
                Description = "Speed and coordination",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.03,
                IsActive = true,
                UnlockLevel = 1
            },

            // Mental Stats
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Focus",
                Category = "Mental",
                Description = "Concentration and attention span from deep work",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.03,
                IsActive = true,
                UnlockLevel = 1
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Intelligence",
                Category = "Mental",
                Description = "Learning speed and knowledge acquisition",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.015,
                IsActive = true,
                UnlockLevel = 1
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Creativity",
                Category = "Mental",
                Description = "Innovation and problem-solving ability",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.02,
                IsActive = true,
                UnlockLevel = 5
            },

            // Behavioral Stats
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Discipline",
                Category = "Behavioral",
                Description = "Self-control and consistency in completing tasks",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.025,
                IsActive = true,
                UnlockLevel = 1
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Willpower",
                Category = "Behavioral",
                Description = "Resistance to procrastination and distractions",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.02,
                IsActive = true,
                UnlockLevel = 5
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Consistency",
                Category = "Behavioral",
                Description = "Ability to maintain streaks and habits",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.015,
                IsActive = true,
                UnlockLevel = 10
            },

            // Social Stats
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Charisma",
                Category = "Social",
                Description = "Social influence and communication skills",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.02,
                IsActive = true,
                UnlockLevel = 5
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Leadership",
                Category = "Social",
                Description = "Ability to inspire and guide others",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.015,
                IsActive = true,
                UnlockLevel = 10
            },
            new StatDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Empathy",
                Category = "Social",
                Description = "Understanding and connecting with others",
                MinValue = 0,
                MaxValue = 100,
                DecayRate = 0.01,
                IsActive = true,
                UnlockLevel = 15
            }
        };

        await context.StatDefinitions.AddRangeAsync(defaultStats);
        await context.SaveChangesAsync();
        Log.Information("Default stat definitions seeded successfully");
    }

    public static async Task SeedDefaultSkills(ApplicationDbContext context)
    {
        if (await context.SkillDefinitions.AnyAsync())
        {
            Log.Information("Skills already exist, skipping seed");
            return;
        }

        // Get stat IDs for unlock conditions
        var intelligence = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Intelligence");
        var focus = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Focus");
        var willpower = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Willpower");
        var stamina = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Stamina");
        var strength = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Strength");
        var charisma = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Charisma");
        var discipline = await context.StatDefinitions.FirstOrDefaultAsync(s => s.Name == "Discipline");

        var skills = new List<SkillDefinition>
        {
            // Passive Skills - Mental
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Quick Learner",
                Description = "Gain 10% bonus XP from all completed quests",
                SkillType = SkillType.Passive,
                Category = "Mental",
                UnlockConditions = $"{{\"minLevel\":3,\"requiredStats\":[{{\"statId\":\"{intelligence?.Id}\",\"minValue\":25}}]}}",
                Effects = "{\"xpBonus\":0.1}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Iron Will",
                Description = "Reduce stat decay rate by 50%",
                SkillType = SkillType.Passive,
                Category = "Mental",
                UnlockConditions = $"{{\"minLevel\":5,\"requiredStats\":[{{\"statId\":\"{willpower?.Id}\",\"minValue\":30}}]}}",
                Effects = "{\"decayReduction\":0.5}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Focused Mind",
                Description = "Reduce fatigue accumulation by 30%",
                SkillType = SkillType.Passive,
                Category = "Mental",
                UnlockConditions = $"{{\"minLevel\":4,\"requiredStats\":[{{\"statId\":\"{focus?.Id}\",\"minValue\":35}}]}}",
                Effects = "{\"fatigueReduction\":0.3}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Active Skills - Mental
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Focus Burst",
                Description = "Temporarily boost Focus by 15 points for 1 hour",
                SkillType = SkillType.Active,
                Category = "Mental",
                UnlockConditions = $"{{\"minLevel\":6,\"requiredStats\":[{{\"statId\":\"{focus?.Id}\",\"minValue\":40}}]}}",
                Effects = "{\"statBoost\":{\"Focus\":15},\"duration\":1}",
                CooldownHours = 24,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Mental Clarity",
                Description = "Reset fatigue to 0 for the day",
                SkillType = SkillType.Active,
                Category = "Mental",
                UnlockConditions = $"{{\"minLevel\":8,\"requiredStats\":[{{\"statId\":\"{intelligence?.Id}\",\"minValue\":45}}]}}",
                Effects = "{\"resetFatigue\":true}",
                CooldownHours = 48,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Passive Skills - Physical
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Endurance Training",
                Description = "Increase Stamina gains from quests by 25%",
                SkillType = SkillType.Passive,
                Category = "Physical",
                UnlockConditions = $"{{\"minLevel\":4,\"requiredStats\":[{{\"statId\":\"{stamina?.Id}\",\"minValue\":30}}]}}",
                Effects = "{\"statGainBonus\":{\"Stamina\":0.25}}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Warrior's Spirit",
                Description = "Gain 5% bonus XP for each day in your current streak",
                SkillType = SkillType.Passive,
                Category = "Physical",
                UnlockConditions = $"{{\"minLevel\":7,\"requiredStats\":[{{\"statId\":\"{strength?.Id}\",\"minValue\":35}}]}}",
                Effects = "{\"streakXPBonus\":0.05}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Active Skills - Physical
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Second Wind",
                Description = "Restore 20 points to all physical stats",
                SkillType = SkillType.Active,
                Category = "Physical",
                UnlockConditions = $"{{\"minLevel\":10,\"requiredStats\":[{{\"statId\":\"{stamina?.Id}\",\"minValue\":50}}]}}",
                Effects = "{\"restoreStats\":{\"Strength\":20,\"Stamina\":20,\"Agility\":20}}",
                CooldownHours = 72,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Passive Skills - Social
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Natural Leader",
                Description = "Increase Charisma gains by 30%",
                SkillType = SkillType.Passive,
                Category = "Social",
                UnlockConditions = $"{{\"minLevel\":5,\"requiredStats\":[{{\"statId\":\"{charisma?.Id}\",\"minValue\":30}}]}}",
                Effects = "{\"statGainBonus\":{\"Charisma\":0.3}}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Passive Skills - Discipline
            new SkillDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Unbreakable Discipline",
                Description = "Reduce Discipline loss from failed quests by 50%",
                SkillType = SkillType.Passive,
                Category = "Discipline",
                UnlockConditions = $"{{\"minLevel\":6,\"requiredStats\":[{{\"statId\":\"{discipline?.Id}\",\"minValue\":40}}]}}",
                Effects = "{\"disciplineLossReduction\":0.5}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.SkillDefinitions.AddRangeAsync(skills);
        await context.SaveChangesAsync();
        Log.Information("Default skills seeded successfully: {Count} skills", skills.Count);
    }
}
