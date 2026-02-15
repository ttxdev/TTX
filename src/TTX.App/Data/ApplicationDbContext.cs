using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TTX.App.Data.Converters;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    private static readonly MethodInfo DateTruncMethod
        = typeof(ApplicationDbContext).GetRuntimeMethod(nameof(DateTrunc), [typeof(string), typeof(DateTime)])!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ModelId>().HaveConversion<ModelIdConverter>();
        configurationBuilder.Properties<Name>().HaveConversion<NameConverter>();
        configurationBuilder.Properties<Slug>().HaveConversion<SlugConverter>();
        configurationBuilder.Properties<PlatformId>().HaveConversion<PlatformIdConverter>();
        configurationBuilder.Properties<Quantity>().HaveConversion<QuantityConverter>();
        configurationBuilder.Properties<Ticker>().HaveConversion<TickerConverter>();
        configurationBuilder.Properties<Credits>()
            .HaveConversion<CreditsConverter>()
            .HavePrecision(2);
        configurationBuilder.Properties<Uri>().HaveConversion<string>();
    }

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
                .HasColumnName("id");
            entity.Property(p => p.Name)
                .HasColumnName("name");
            entity.Property(p => p.Slug)
                .HasColumnName("slug");
            entity.Property(p => p.Platform)
                .HasConversion(
                    p => p.ToString(),
                    p => Enum.Parse<Platform>(p)
                )
                .HasColumnName("platform");
            entity.Property(p => p.PlatformId)
                .HasColumnName("platform_id");
            entity.Property(p => p.AvatarUrl)
                .HasColumnName("avatar_url");
            entity.Property(p => p.Credits)
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

            entity.HasMany<PortfolioSnapshot>()
                .WithOne(p => p.Player)
                .HasForeignKey(p => p.PlayerId)
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
                .HasColumnName("id");
            entity.Property(c => c.Name)
                .HasColumnName("name");
            entity.Property(c => c.Slug)
                .HasColumnName("slug");
            entity.Property(c => c.Ticker)
                .HasColumnName("ticker");
            entity.Property(p => p.Platform)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<Platform>(t)
                )
                .HasColumnName("platform");
            entity.Property(c => c.PlatformId)
                .HasColumnName("platform_id");
            entity.Property(c => c.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(c => c.Value).HasColumnName("value");
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

            entity.HasMany<Vote>()
                .WithOne(v => v.Creator)
                .HasForeignKey(v => v.CreatorId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasMany<LootBox>()
                .WithOne(l => l.Result)
                .HasForeignKey(l => l.ResultId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(c => c.History);

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.Ticker).IsUnique();
            entity.HasIndex(u => new { u.Platform, u.PlatformId }).IsUnique();
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

        modelBuilder.Entity<PortfolioSnapshot>(entity =>
        {
            entity.ToTable("player_portfolios");

            entity.HasNoKey();
            entity.Property(p => p.PlayerId).HasColumnName("player_id");
            entity.Property(p => p.Value).HasColumnName("value");
            entity.Property(p => p.Time).HasColumnName("time");

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
            entity.Property(l => l.Id).ValueGeneratedOnAdd().HasColumnName("id");
            entity.Property(l => l.PlayerId).HasColumnName("player_id");
            entity.Property(l => l.ResultId).HasColumnName("result_id");
            entity.Property(l => l.CreatedAt).HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt).HasColumnName("updated_at");

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
            entity.Property(t => t.Id).ValueGeneratedOnAdd().HasColumnName("id");
            entity.Property(t => t.Quantity).HasColumnName("quantity");
            entity.Property(t => t.Value).HasColumnName("value");
            entity.Property(t => t.Action)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<TransactionAction>(t)
                )
                .HasColumnName("action");
            entity.Property(t => t.PlayerId).HasColumnName("player_id");
            entity.Property(t => t.CreatorId).HasColumnName("creator_id");
            entity.Property(t => t.CreatedAt).HasColumnName("created_at");
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at");

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
                .HasColumnName("id");
            entity.Property(a => a.SubmitterId)
                .HasColumnName("submitter_id");
            entity.Property(a => a.Name)
                .HasColumnName("name");
            entity.Property(a => a.PlatformId)
                .HasColumnName("platform_id");
            entity.Property(a => a.Ticker)
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
                .HasColumnName("id");
            entity.Property(a => a.PlatformId)
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        IEnumerable<EntityEntry<Model>> entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .OfType<EntityEntry<Model>>();

        foreach (EntityEntry<Model> entry in entries)
        {
            entry.Entity.Bump();
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
