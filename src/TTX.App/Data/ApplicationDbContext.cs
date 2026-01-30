using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TTX.App.Data.Converters;
using TTX.Domain.Models;

namespace TTX.App.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    private static readonly MethodInfo DateTruncMethod
        = typeof(ApplicationDbContext).GetRuntimeMethod(nameof(DateTrunc), [typeof(string), typeof(DateTime)])!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasDbFunction(DateTruncMethod).HasName("date_trunc").IsBuiltIn();
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players");

            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(p => p.Name)
                .HasConversion(new NameConverter())
                .HasColumnName("name");
            entity.Property(p => p.Slug)
                .HasConversion(new SlugConverter())
                .HasColumnName("slug");
            entity.Property(p => p.PlatformId)
                .HasConversion(new PlatformIdConverter())
                .HasColumnName("platform_id");
            entity.Property(p => p.AvatarUrl)
                .HasConversion(
                    a => a.ToString(),
                    a => new Uri(a)
                )
                .HasColumnName("avatar_url");
            entity.Property(p => p.Credits)
                .HasConversion(new CreditsConverter())
                .HasColumnName("credits");
            entity.Property(p => p.Portfolio)
                .HasColumnName("portfolio");
            entity.Property(p => p.Type)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<PlayerType>(t)
                )
                .HasColumnName("type");
            entity.Property(p => p.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasMany(p => p.Transactions)
                .WithOne(t => t.Player)
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasMany(p => p.LootBoxes)
                .WithOne(t => t.Player)
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.Ignore(p => p.History);

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(p => p.PlatformId).IsUnique();
            entity.HasIndex(p => p.Type);
        });

        modelBuilder.Entity<Creator>(entity =>
        {
            entity.ToTable("creators");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(c => c.Name)
                .HasConversion(new NameConverter())
                .HasColumnName("name");
            entity.Property(c => c.Slug)
                .HasConversion(new SlugConverter())
                .HasColumnName("slug");
            entity.Property(c => c.Ticker)
                .HasConversion(new TickerConverter())
                .HasColumnName("ticker");
            entity.Property(c => c.PlatformId)
                .HasConversion(new PlatformIdConverter())
                .HasColumnName("platform_id");
            entity.Property(u => u.AvatarUrl)
                .HasConversion(
                    a => a.ToString(),
                    a => new Uri(a)
                )
                .HasColumnName("avatar_url");
            entity.Property(c => c.Value)
                .HasConversion(new CreditsConverter())
                .HasColumnName("value");
            entity.OwnsOne(c => c.StreamStatus, ss =>
            {
                ss.WithOwner();
                ss.Property(s => s.IsLive)
                    .HasColumnName("stream_is_live");
                ss.Property(s => s.StartedAt)
                    .HasColumnName("stream_started_at");
                ss.Property(s => s.EndedAt)
                    .HasColumnName("stream_ended_at");
            });
            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasMany(c => c.Transactions)
                .WithOne(t => t.Creator)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.Ignore(c => c.History);

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.Ticker).IsUnique();
            entity.HasIndex(u => new { u.Platform, u.PlatformId }).IsUnique();
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("votes");

            entity.HasNoKey();
            entity.Property(v => v.CreatorId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("creator_id");
            entity.Property(v => v.Value)
                .HasConversion(new CreditsConverter())
                .HasColumnName("value");
            entity.Property(v => v.Time)
                .HasColumnName("time");

            entity.HasIndex(v => new { v.CreatorId, v.Time });
        });

        modelBuilder.Entity<PortfolioSnapshot>(entity =>
        {
            entity.ToTable("player_portfolios");

            entity.HasNoKey();
            entity.Property(p => p.PlayerId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("player_id");
            entity.Property(p => p.Value)
                .HasColumnName("value");
            entity.Property(p => p.Time)
                .HasColumnName("time");

            entity.HasOne(p => p.Player)
                .WithMany()
                .HasForeignKey(p => p.PlayerId)
                .IsRequired();

            entity.HasIndex(p => new { p.PlayerId, p.Time });
        });

        modelBuilder.Entity<LootBox>(entity =>
        {
            entity.ToTable("loot_boxes");

            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(l => l.PlayerId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("player_id");
            entity.Property(l => l.ResultId)
                .HasConversion(new ModelIdConverter()!)
                .HasColumnName("result_id");
            entity.Property(l => l.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(l => l.Player)
                .WithMany(u => u.LootBoxes)
                .HasForeignKey(l => l.PlayerId)
                .IsRequired();

            entity.HasOne(l => l.Result)
                .WithMany()
                .HasForeignKey(l => l.ResultId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(t => t.Quantity)
                .HasConversion(new QuantityConverter())
                .HasColumnName("quantity");
            entity.Property(t => t.Value)
                .HasConversion(new CreditsConverter())
                .HasColumnName("value");
            entity.Property(t => t.Action)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<TransactionAction>(t)
                )
                .HasColumnName("action");
            entity.Property(t => t.PlayerId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("player_id");
            entity.Property(t => t.CreatorId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("creator_id");
            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(t => t.Player)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.PlayerId)
                .IsRequired();

            entity.HasOne(t => t.Creator)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CreatorId)
                .IsRequired();
        });

        modelBuilder.Entity<CreatorApplication>(entity =>
        {
            entity.ToTable("creator_applications");

            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(a => a.SubmitterId)
                .HasConversion(new ModelIdConverter())
                .HasColumnName("submitter_id");
            entity.Property(a => a.Name)
                .HasConversion(new NameConverter())
                .HasColumnName("name");
            entity.Property(a => a.PlatformId)
                .HasConversion(new PlatformIdConverter())
                .HasColumnName("platform_id");
            entity.Property(a => a.Ticker)
                .HasConversion(new TickerConverter())
                .HasColumnName("ticker");
            entity.Property(a => a.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<CreatorApplicationStatus>(s)
                )
                .HasColumnName("status");
            entity.Property(a => a.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(a => a.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(a => a.Submitter)
                .WithMany()
                .HasForeignKey(a => a.SubmitterId)
                .IsRequired();
        });

        modelBuilder.Entity<CreatorOptOut>(entity =>
        {
            entity.ToTable("creator_opt_outs");

            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(new ModelIdConverter())
                .HasColumnName("id");
            entity.Property(a => a.PlatformId)
                .HasConversion(new PlatformIdConverter())
                .HasColumnName("platform_id");
            entity.Property(a => a.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(a => a.UpdatedAt)
                .HasColumnName("updated_at");
            entity.HasIndex(a => a.PlatformId)
                .IsUnique();
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
        IEnumerable<EntityEntry> entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Model && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (EntityEntry entry in entries)
        {
            Model entity = (Model)entry.Entity;
            entity.Bump();
        }
    }

    public static DateTime DateTrunc(string field, DateTime source)
    {
        throw new NotSupportedException();
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Creator> Creators => Set<Creator>();
    public DbSet<CreatorOptOut> CreatorOptOuts => Set<CreatorOptOut>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<PortfolioSnapshot> Portfolios => Set<PortfolioSnapshot>();
    public DbSet<LootBox> LootBoxes => Set<LootBox>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<CreatorApplication> CreatorApplications => Set<CreatorApplication>();
}
