using Microsoft.EntityFrameworkCore;
using Klario.DAL.Models;

namespace Klario.DAL.Context;

public class KlarioDbContext : DbContext
{
    public KlarioDbContext(DbContextOptions<KlarioDbContext> options) : base(options)
    {
    }

    public DbSet<SearchProfile> SearchProfiles => Set<SearchProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically applies SearchProfileConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KlarioDbContext).Assembly);
    }
}
