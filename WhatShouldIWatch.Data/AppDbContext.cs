using Microsoft.EntityFrameworkCore;
using WhatShouldIWatch.Data.Entities;

namespace WhatShouldIWatch.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ContentType> ContentTypes => Set<ContentType>();
    public DbSet<Content> Contents => Set<Content>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("CNT");

        modelBuilder.Entity<ContentType>(e =>
        {
            e.ToTable("ContentType", "CNT");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Content>(e =>
        {
            e.ToTable("Content", "CNT");
            e.HasKey(x => x.Id);
            e.Property(x => x.ContentNameTr).HasMaxLength(500);
            e.Property(x => x.ContentNameEn).HasMaxLength(500);
            e.Property(x => x.EmotionTarget).HasMaxLength(500);
            e.Property(x => x.Genre).HasMaxLength(500);
            e.Property(x => x.Emotion).HasMaxLength(500);
            e.Property(x => x.CreatedAt).IsRequired();
            e.HasOne(x => x.ContentType)
                .WithMany(x => x.Contents)
                .HasForeignKey(x => x.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
