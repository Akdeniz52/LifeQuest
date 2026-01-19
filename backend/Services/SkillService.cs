using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LevelingSystem.API.Services;

// Unlock condition models
public class UnlockConditions
{
    public int MinLevel { get; set; }
    public List<StatRequirement> RequiredStats { get; set; } = new();
}

public class StatRequirement
{
    public Guid StatId { get; set; }
    public double MinValue { get; set; }
}

public class SkillService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SkillService> _logger;

    public SkillService(
        ApplicationDbContext context,
        ILogger<SkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SkillDefinition>> GetAvailableSkills(Guid characterId)
    {
        var character = await _context.Characters
            .Include(c => c.Stats)
                .ThenInclude(s => s.StatDefinition)
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null)
        {
            return new List<SkillDefinition>();
        }

        var allSkills = await _context.SkillDefinitions
            .Where(s => s.IsActive)
            .ToListAsync();

        var unlockedSkillIds = character.Skills.Select(s => s.SkillDefinitionId).ToHashSet();

        var availableSkills = new List<SkillDefinition>();

        foreach (var skill in allSkills)
        {
            if (unlockedSkillIds.Contains(skill.Id))
            {
                continue; // Already unlocked
            }

            if (await CanUnlockSkill(character, skill))
            {
                availableSkills.Add(skill);
            }
        }

        return availableSkills;
    }

    public async Task<bool> CanUnlockSkill(Guid characterId, Guid skillId)
    {
        var character = await _context.Characters
            .Include(c => c.Stats)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        var skill = await _context.SkillDefinitions
            .FirstOrDefaultAsync(s => s.Id == skillId);

        if (character == null || skill == null)
        {
            return false;
        }

        return await CanUnlockSkill(character, skill);
    }

    private async Task<bool> CanUnlockSkill(Character character, SkillDefinition skill)
    {
        try
        {
            var conditions = JsonSerializer.Deserialize<UnlockConditions>(skill.UnlockConditions);
            if (conditions == null)
            {
                return false;
            }

            // Check level requirement
            if (character.Level < conditions.MinLevel)
            {
                return false;
            }

            // Check stat requirements
            foreach (var statReq in conditions.RequiredStats)
            {
                var characterStat = character.Stats.FirstOrDefault(s => s.StatDefinitionId == statReq.StatId);
                if (characterStat == null || characterStat.CurrentValue < statReq.MinValue)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CharacterSkill?> UnlockSkill(Guid characterId, Guid skillId)
    {
        // Check if already unlocked
        var existing = await _context.CharacterSkills
            .FirstOrDefaultAsync(cs => cs.CharacterId == characterId && cs.SkillDefinitionId == skillId);

        if (existing != null)
        {
            return existing;
        }

        // Check if can unlock
        if (!await CanUnlockSkill(characterId, skillId))
        {
            return null;
        }

        var characterSkill = new CharacterSkill
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            SkillDefinitionId = skillId,
            UnlockedAt = DateTime.UtcNow,
            TimesUsed = 0
        };

        _context.CharacterSkills.Add(characterSkill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Skill {SkillId} unlocked for character {CharacterId}", 
            skillId, characterId);

        return characterSkill;
    }

    public async Task<bool> UseActiveSkill(Guid characterId, Guid skillId)
    {
        var characterSkill = await _context.CharacterSkills
            .Include(cs => cs.SkillDefinition)
            .FirstOrDefaultAsync(cs => cs.CharacterId == characterId && cs.SkillDefinitionId == skillId);

        if (characterSkill == null)
        {
            return false; // Skill not unlocked
        }

        var skill = characterSkill.SkillDefinition;

        if (skill.SkillType != SkillType.Active)
        {
            return false; // Not an active skill
        }

        // Check cooldown
        if (skill.CooldownHours.HasValue && characterSkill.LastUsedAt.HasValue)
        {
            var cooldownEnd = characterSkill.LastUsedAt.Value.AddHours(skill.CooldownHours.Value);
            if (DateTime.UtcNow < cooldownEnd)
            {
                return false; // Still on cooldown
            }
        }

        // Use skill
        characterSkill.LastUsedAt = DateTime.UtcNow;
        characterSkill.TimesUsed++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Active skill {SkillId} used by character {CharacterId}", 
            skillId, characterId);

        return true;
    }

    public async Task CheckAndUnlockSkills(Guid characterId)
    {
        var availableSkills = await GetAvailableSkills(characterId);

        foreach (var skill in availableSkills)
        {
            await UnlockSkill(characterId, skill.Id);
        }
    }
}
