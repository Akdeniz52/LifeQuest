using Microsoft.EntityFrameworkCore;
using LevelingSystem.API.Models.Entities;

namespace LevelingSystem.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<StatDefinition> StatDefinitions { get; set; }
    public DbSet<CharacterStat> CharacterStats { get; set; }
    public DbSet<QuestDefinition> QuestDefinitions { get; set; }
    public DbSet<QuestStatEffect> QuestStatEffects { get; set; }
    public DbSet<QuestInstance> QuestInstances { get; set; }
    public DbSet<ProgressLog> ProgressLogs { get; set; }
    public DbSet<SystemMessage> SystemMessages { get; set; }
    public DbSet<Streak> Streaks { get; set; }
    public DbSet<FatigueLog> FatigueLogs { get; set; }
    public DbSet<SkillDefinition> SkillDefinitions { get; set; }
    public DbSet<CharacterSkill> CharacterSkills { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Character Configuration
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.CurrentXP).HasDefaultValue(0);
            entity.Property(e => e.TotalXP).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Character)
                .HasForeignKey<Character>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StatDefinition Configuration
        modelBuilder.Entity<StatDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MinValue).HasDefaultValue(0);
            entity.Property(e => e.MaxValue).HasDefaultValue(100);
            entity.Property(e => e.DecayRate).HasDefaultValue(0.02);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UnlockLevel).HasDefaultValue(1);
        });

        // CharacterStat Configuration
        modelBuilder.Entity<CharacterStat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CharacterId, e.StatDefinitionId }).IsUnique();
            entity.Property(e => e.CurrentValue).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.Stats)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.StatDefinition)
                .WithMany(s => s.CharacterStats)
                .HasForeignKey(e => e.StatDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuestDefinition Configuration
        modelBuilder.Entity<QuestDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.QuestType).HasConversion<string>();
            entity.Property(e => e.IsMandatory).HasDefaultValue(false);
            entity.Property(e => e.DifficultyMultiplier).HasDefaultValue(1.0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RecurrenceType).HasConversion<string>();
            entity.Property(e => e.AutoAssign).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuestStatEffect Configuration
        modelBuilder.Entity<QuestStatEffect>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.QuestDefinitionId, e.StatDefinitionId }).IsUnique();
            entity.Property(e => e.EffectMultiplier).HasDefaultValue(1.0);

            entity.HasOne(e => e.QuestDefinition)
                .WithMany(q => q.StatEffects)
                .HasForeignKey(e => e.QuestDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.StatDefinition)
                .WithMany(s => s.QuestStatEffects)
                .HasForeignKey(e => e.StatDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuestInstance Configuration
        modelBuilder.Entity<QuestInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>().HasDefaultValue(QuestStatus.Pending);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.QuestInstances)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.QuestDefinition)
                .WithMany(q => q.QuestInstances)
                .HasForeignKey(e => e.QuestDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProgressLog Configuration
        modelBuilder.Entity<ProgressLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasConversion<string>();
            entity.Property(e => e.XPChange).HasDefaultValue(0);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.ProgressLogs)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.QuestInstance)
                .WithMany()
                .HasForeignKey(e => e.QuestInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SystemMessage Configuration
        modelBuilder.Entity<SystemMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageType).HasConversion<string>();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.SystemMessages)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Streak Configuration
        modelBuilder.Entity<Streak>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CharacterId, e.StreakType }).IsUnique();
            entity.Property(e => e.StreakType).HasConversion<string>();
            entity.Property(e => e.CurrentStreak).HasDefaultValue(0);
            entity.Property(e => e.LongestStreak).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.Streaks)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FatigueLog Configuration
        modelBuilder.Entity<FatigueLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CharacterId, e.Date }).IsUnique();
            entity.Property(e => e.FatigueLevel).HasDefaultValue(0);
            entity.Property(e => e.QuestsCompleted).HasDefaultValue(0);
            entity.Property(e => e.QuestsAssigned).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany(c => c.FatigueLogs)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SkillDefinition Configuration
        modelBuilder.Entity<SkillDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SkillType).HasConversion<string>();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.UnlockConditions).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Effects).HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // CharacterSkill Configuration
        modelBuilder.Entity<CharacterSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CharacterId, e.SkillDefinitionId }).IsUnique();
            entity.Property(e => e.TimesUsed).HasDefaultValue(0);

            entity.HasOne(e => e.Character)
                .WithMany(c => c.Skills)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SkillDefinition)
                .WithMany(s => s.CharacterSkills)
                .HasForeignKey(e => e.SkillDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntry Configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Character)
                .WithMany()
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
