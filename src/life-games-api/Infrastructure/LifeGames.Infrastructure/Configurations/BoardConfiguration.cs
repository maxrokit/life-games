using LifeGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeGames.Infrastructure.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Board");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .HasMaxLength(200);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.HasIndex(b => b.CreatedAt);

        // Configure one-to-many relationship with BoardGeneration
        builder.Metadata.FindNavigation(nameof(Board.Generations))!
            .SetField("_generations");

        builder.HasMany(b => b.Generations)
            .WithOne(bg => bg.Board!)
            .HasForeignKey(bg => bg.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
