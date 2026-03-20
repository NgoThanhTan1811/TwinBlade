using TwinBlade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TwinBlade.Infrastructure.Persistence.Rds;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerProgress> PlayerProgresses => Set<PlayerProgress>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<DungeonFloor> DungeonFloors => Set<DungeonFloor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();

            entity.HasOne(x => x.Progress)
                  .WithOne()
                  .HasForeignKey<PlayerProgress>(x => x.PlayerId);
        });

        modelBuilder.Entity<PlayerProgress>(entity =>
        {
            entity.HasKey(x => x.PlayerId);
            entity.Property(x => x.Gold).IsRequired();
            entity.OwnsMany(x => x.Inventory);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.OwnsMany(x => x.Players);
        });

        modelBuilder.Entity<MatchResult>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.OwnsMany(x => x.Players);
        });

        modelBuilder.Entity<DungeonFloor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FloorNumber).IsUnique();
        });
    }
}