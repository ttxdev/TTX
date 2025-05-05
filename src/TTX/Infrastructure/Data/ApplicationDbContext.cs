using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TTX.Infrastructure.Data.Converters;
using TTX.Models;

namespace TTX.Infrastructure.Data
{
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

                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(new ModelIdConverter())
                    .UseIdentityColumn()
                    .HasColumnOrder(0)
                    .HasColumnName("id");
                entity.Property(u => u.Name)
                    .HasConversion(new NameConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("name");
                entity.Property(u => u.Slug)
                    .HasConversion(new SlugConverter())
                    .HasColumnOrder(2)
                    .HasColumnName("slug");
                entity.Property(u => u.TwitchId)
                    .HasConversion(new TwitchIdConverter())
                    .HasColumnOrder(3)
                    .HasColumnName("twitch_id");
                entity.Property(u => u.AvatarUrl)
                    .HasConversion(
                        a => a.ToString(),
                        a => new Uri(a)
                    )
                    .HasColumnOrder(4)
                    .HasColumnName("avatar_url");
                entity.Property(u => u.Credits)
                    .HasConversion(new CreditsConverter())
                    .HasColumnOrder(5)
                    .HasColumnName("credits");
                entity.Property(u => u.Portfolio)
                    .HasColumnOrder(6)
                    .HasColumnName("portfolio");
                entity.Property(u => u.Type)
                    .HasColumnOrder(7)
                    .HasConversion(
                        t => t.ToString(),
                        t => Enum.Parse<PlayerType>(t)
                    )
                    .HasColumnName("type");
                entity.Property(l => l.CreatedAt)
                    .HasColumnOrder(8)
                    .HasColumnName("created_at");
                entity.Property(l => l.UpdatedAt)
                    .HasColumnOrder(9)
                    .HasColumnName("updated_at");

                entity.Ignore(p => p.History);
                entity.HasMany(c => c.Transactions)
                    .WithOne(t => t.Player)
                    .HasForeignKey(t => t.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.HasMany(c => c.LootBoxes)
                    .WithOne(t => t.Player)
                    .HasForeignKey(t => t.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.HasIndex(c => c.Slug).IsUnique();
                entity.HasIndex(u => u.TwitchId).IsUnique();
                entity.HasIndex(u => u.Type);
            });

            modelBuilder.Entity<Creator>(entity =>
            {
                entity.ToTable("creators");

                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(new ModelIdConverter())
                    .UseIdentityColumn()
                    .HasColumnOrder(0)
                    .HasColumnName("id");
                entity.Property(c => c.Name)
                    .HasConversion(new NameConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("name");
                entity.Property(c => c.Slug)
                    .HasConversion(new SlugConverter())
                    .HasColumnOrder(2)
                    .HasColumnName("slug");
                entity.Property(c => c.Ticker)
                    .HasConversion(new TickerConverter())
                    .HasColumnOrder(3)
                    .HasColumnName("ticker");
                entity.Property(c => c.TwitchId)
                    .HasConversion(new TwitchIdConverter())
                    .HasColumnOrder(4)
                    .HasColumnName("twitch_id");
                entity.Property(u => u.AvatarUrl)
                    .HasConversion(
                        a => a.ToString(),
                        a => new Uri(a)
                    )
                    .HasColumnOrder(5)
                    .HasColumnName("avatar_url");
                entity.Property(c => c.Value)
                    .HasConversion(new CreditsConverter())
                    .HasColumnOrder(6)
                    .HasColumnName("value");
                entity.OwnsOne(c => c.StreamStatus, ss =>
                {
                    ss.WithOwner();
                    ss.Property(s => s.IsLive)
                        .HasColumnOrder(7)
                        .HasColumnName("stream_is_live");
                    ss.Property(s => s.StartedAt)
                        .HasColumnOrder(8)
                        .HasColumnName("stream_started_at");
                    ss.Property(s => s.EndedAt)
                        .HasColumnOrder(9)
                        .HasColumnName("stream_ended_at");
                });
                entity.Property(c => c.CreatedAt)
                    .HasColumnOrder(10)
                    .HasColumnName("created_at");
                entity.Property(c => c.UpdatedAt)
                    .HasColumnOrder(11)
                    .HasColumnName("updated_at");

                entity.HasMany(c => c.Transactions)
                    .WithOne(t => t.Creator)
                    .HasForeignKey(t => t.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.Ignore(c => c.History);
                entity.HasMany<Vote>()
                    .WithOne()
                    .HasForeignKey(v => v.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.Slug).IsUnique();
                entity.HasIndex(c => c.Ticker).IsUnique();
                entity.HasIndex(u => u.TwitchId).IsUnique();
            });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.ToTable("votes");

                entity.HasNoKey();
                entity.Property(v => v.CreatorId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(0)
                    .HasColumnName("creator_id");
                entity.Property(v => v.Value)
                    .HasConversion(new CreditsConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("value");
                entity.Property(v => v.Time)
                    .HasColumnOrder(2)
                    .HasColumnName("time");

                entity.HasOne(v => v.Creator)
                    .WithMany()
                    .HasForeignKey(v => v.CreatorId)
                    .IsRequired();

                entity.HasIndex(v => new { v.CreatorId, v.Time });
            });

            modelBuilder.Entity<PortfolioSnapshot>(entity =>
            {
                entity.ToTable("player_portfolios");

                entity.HasNoKey();
                entity.Property(p => p.PlayerId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(0)
                    .HasColumnName("player_id");
                entity.Property(p => p.Value)
                    .HasColumnOrder(2)
                    .HasColumnName("value");
                entity.Property(p => p.Time)
                    .HasColumnOrder(3)
                    .HasColumnName("time");
                ;

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
                    .UseIdentityColumn()
                    .HasColumnOrder(0)
                    .HasColumnName("id");
                entity.Property(l => l.PlayerId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("player_id");
                entity.Property(l => l.ResultId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(2)
                    .HasColumnName("result_id");
                entity.Property(l => l.CreatedAt)
                    .HasColumnOrder(3)
                    .HasColumnName("created_at");
                entity.Property(l => l.UpdatedAt)
                    .HasColumnOrder(4)
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
                    .UseIdentityColumn()
                    .HasColumnOrder(0)
                    .HasColumnName("id");
                entity.Property(t => t.Quantity)
                    .HasConversion(new QuantityConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("quantity");
                entity.Property(t => t.Value)
                    .HasConversion(new CreditsConverter())
                    .HasColumnOrder(2)
                    .HasColumnName("value");
                entity.Property(t => t.Action)
                    .HasColumnOrder(3)
                    .HasConversion(
                        t => t.ToString(),
                        t => Enum.Parse<TransactionAction>(t)
                    )
                    .HasColumnName("action");
                entity.Property(t => t.PlayerId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(4)
                    .HasColumnName("player_id");
                entity.Property(t => t.CreatorId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(5)
                    .HasColumnName("creator_id");
                entity.Property(t => t.CreatedAt)
                    .HasColumnOrder(6)
                    .HasColumnName("created_at");
                entity.Property(t => t.UpdatedAt)
                    .HasColumnOrder(7)
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
                    .UseIdentityColumn()
                    .HasColumnOrder(0)
                    .HasColumnName("id");
                entity.Property(a => a.SubmitterId)
                    .HasConversion(new ModelIdConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("submitter_id");
                entity.Property(a => a.Name)
                    .HasConversion(new NameConverter())
                    .HasColumnOrder(1)
                    .HasColumnName("name");
                entity.Property(a => a.TwitchId)
                    .HasConversion(new TwitchIdConverter())
                    .HasColumnOrder(2)
                    .HasColumnName("twitch_id");
                entity.Property(a => a.Ticker)
                    .HasConversion(new TickerConverter())
                    .HasColumnOrder(3)
                    .HasColumnName("ticker");
                entity.Property(a => a.Status)
                    .HasColumnOrder(4)
                    .HasConversion(
                        s => s.ToString(),
                        s => Enum.Parse<CreatorApplicationStatus>(s)
                    )
                    .HasColumnName("status");
                entity.Property(a => a.CreatedAt)
                    .HasColumnOrder(5)
                    .HasColumnName("created_at");
                entity.Property(a => a.UpdatedAt)
                    .HasColumnOrder(6)
                    .HasColumnName("updated_at");

                entity.HasOne<Player>()
                    .WithMany()
                    .HasForeignKey(t => t.SubmitterId)
                    .IsRequired();
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

        #region DbSet

        public DbSet<Player> Players => Set<Player>();
        public DbSet<Creator> Creators => Set<Creator>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<PortfolioSnapshot> Portfolios => Set<PortfolioSnapshot>();
        public DbSet<LootBox> LootBoxes => Set<LootBox>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<CreatorApplication> CreatorApplications => Set<CreatorApplication>();

        #endregion
    }
}