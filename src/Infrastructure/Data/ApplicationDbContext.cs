using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;

namespace TTX.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasDbFunction(DateTruncMethod).HasName("date_trunc").IsBuiltIn();
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Name).HasColumnName("name");
            entity.Property(u => u.TwitchId).HasColumnName("twitch_id");
            entity.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(u => u.Credits).HasColumnName("credits");
            entity.Property(u => u.Type).HasColumnName("type");
            entity.Property(l => l.CreatedAt).HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(c => c.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.LootBoxes)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(u => u.TwitchId).IsUnique();
            entity.HasIndex(u => u.Name).IsUnique();
            entity.HasIndex(u => u.TwitchId).IsUnique();
            entity.HasIndex(u => u.Type);
        });

        modelBuilder.Entity<Creator>(entity =>
        {
            entity.ToTable("creators");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Name).HasColumnName("name");
            entity.Property(c => c.Ticker).HasColumnName("ticker");
            entity.Property(c => c.Slug).HasColumnName("slug");
            entity.Property(c => c.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(c => c.Value).HasColumnName("value");
            entity.Property(c => c.CreatedAt).HasColumnName("created_at");
            entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            entity.OwnsOne(c => c.StreamStatus, ss =>
            {
                ss.WithOwner();
                ss.Property(s => s.IsLive).HasColumnName("stream_is_live");
                ss.Property(s => s.StartedAt).HasColumnName("stream_started_at");
                ss.Property(s => s.EndedAt).HasColumnName("stream_ended_at");
            });

            entity.HasMany(c => c.Transactions)
                .WithOne(t => t.Creator)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(c => c.History);
            entity.HasMany<Vote>()
                .WithOne()
                .HasForeignKey(v => v.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.Ticker).IsUnique();
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("votes");

            entity.HasNoKey();
            entity.Property(v => v.CreatorId).HasColumnName("creator_id");
            entity.Property(v => v.Value).HasColumnName("value");
            entity.Property(v => v.Time).HasColumnName("time");

            entity.HasIndex(v => new { v.CreatorId, v.Time });
        });

        modelBuilder.Entity<LootBox>(entity =>
        {
            entity.ToTable("loot_boxes");

            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).HasColumnName("id");
            entity.Property(l => l.UserId).HasColumnName("user_id");
            entity.Property(l => l.ResultId).HasColumnName("result_id");
            entity.Property(l => l.CreatedAt).HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(l => l.User).WithMany(u => u.LootBoxes).HasForeignKey(l => l.UserId).IsRequired();
            entity.HasOne(l => l.Result).WithMany().HasForeignKey(l => l.ResultId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.Quantity).HasColumnName("quantity");
            entity.Property(t => t.Value).HasColumnName("value");
            entity.Property(t => t.Action).HasColumnName("action");
            entity.Property(t => t.UserId).HasColumnName("user_id");
            entity.Property(t => t.CreatorId).HasColumnName("creator_id");
            entity.Property(t => t.CreatedAt).HasColumnName("created_at");
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId);
            entity.HasOne(t => t.Creator)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CreatorId);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ModelBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (ModelBase)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }

    private static readonly MethodInfo DateTruncMethod
        = typeof(ApplicationDbContext).GetRuntimeMethod(nameof(DateTrunc), [typeof(string), typeof(DateTime)])!;

    public static DateTime DateTrunc(string field, DateTime source)
        => throw new NotSupportedException();
}