using TwinBlade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TwinBlade.Domain.Enums;
using TwinBlade.Domain.Items;

namespace TwinBlade.Infrastructure.Persistence.Rds;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerProgress> PlayerProgresses => Set<PlayerProgress>();
    public DbSet<PlayerItem> PlayerItems => Set<PlayerItem>();
    public DbSet<PlayerEquipment> PlayerEquipments => Set<PlayerEquipment>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    public DbSet<ItemMeterials> ItemMaterials => Set<ItemMeterials>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(p => p.CognitoId).IsUnique();
            entity.Property(x => x.CognitoId).IsRequired();

            entity.Property(x => x.Username).IsRequired();
            entity.HasIndex(p => p.Username).IsUnique();

            entity.Property(x => x.Email).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();

            entity.HasOne(x => x.Progress)
                  .WithOne(p => p.Player)
                  .HasForeignKey<PlayerProgress>(x => x.PlayerId);

            entity.HasMany(x => x.InventoryItems)
                  .WithOne(i => i.Player)
                  .HasForeignKey(i => i.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.EquippedItems)
                  .WithOne(e => e.Player)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerProgress>(entity =>
        {
            entity.HasKey(x => x.PlayerId);
            entity.Property(x => x.Gold).IsRequired();
        });

        modelBuilder.Entity<PlayerItem>(entity =>
        {
            entity.HasKey(pi => new { pi.PlayerId, pi.ItemId });

            entity.HasOne(pi => pi.Item)
                  .WithMany()
                  .HasForeignKey(pi => pi.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(pi => pi.Quantity).IsRequired();
            entity.Property(pi => pi.AcquiredAt).IsRequired();
            entity.Property(pi => pi.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<PlayerEquipment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(pe => new { pe.PlayerId, pe.Slot }).IsUnique();

            entity.HasOne(pe => pe.Item)
                  .WithMany()
                  .HasForeignKey(pe => pe.ItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(pe => pe.Slot)
                  .HasConversion<string>()
                  .IsRequired();

            entity.Property(pe => pe.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).IsRequired();

            entity.HasOne(x => x.ItemType)
                  .WithMany()
                  .HasForeignKey(x => x.ItemTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ItemMaterial)
                  .WithMany()
                  .HasForeignKey(x => x.ItemMaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).IsRequired();
            entity.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<ItemMeterials>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).IsRequired();
            entity.Property(x => x.Name).IsRequired();
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

        modelBuilder.Ignore<RoomPlayerState>();
    }
}
