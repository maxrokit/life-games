using LifeGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LifeGames.Infrastructure.Data;

public class LifeGamesDbContext(DbContextOptions<LifeGamesDbContext> options) : DbContext(options)
{
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardGeneration> BoardGenerations => Set<BoardGeneration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LifeGamesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
