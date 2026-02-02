using System.Text.Json;
using LifeGames.Domain.Entities;
using LifeGames.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeGames.Infrastructure.Configurations;

public class BoardGenerationConfiguration : IEntityTypeConfiguration<BoardGeneration>
{
    public void Configure(EntityTypeBuilder<BoardGeneration> builder)
    {
        builder.ToTable("BoardGeneration");

        builder.HasKey(bg => bg.Id);

        builder.Property(bg => bg.BoardId)
            .IsRequired();

        builder.Property(bg => bg.GenerationNumber)
            .IsRequired();

        builder.Property(bg => bg.ComputedAt)
            .IsRequired();

        // Foreign key relationship configured in BoardConfiguration

        builder.Property(bg => bg.Cells)
            .HasConversion(
                cells => JsonSerializer.Serialize(cells.Select(c => new { c.X, c.Y }), (JsonSerializerOptions?)null),
                json => DeserializeCells(json))
            .HasColumnType("json");

        builder.HasIndex(bg => new { bg.BoardId, bg.GenerationNumber })
            .IsUnique();

        builder.HasIndex(bg => bg.BoardId);
    }

    private static HashSet<Cell> DeserializeCells(string json)
    {
        if (string.IsNullOrEmpty(json))
            return [];

        var cellData = JsonSerializer.Deserialize<List<CellData>>(json);
        return cellData?.Select(c => new Cell(c.X, c.Y)).ToHashSet() ?? [];
    }

    private record CellData(int X, int Y);
}
