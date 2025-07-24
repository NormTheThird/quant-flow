namespace QuantFlow.Data.SQLServer.Context;

/// <summary>
/// Entity Framework Core database context for SQL Server
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<PortfolioEntity> Portfolios { get; set; } = null!;
    public DbSet<BacktestRunEntity> BacktestRuns { get; set; } = null!;
    public DbSet<TradeEntity> Trades { get; set; } = null!;
    public DbSet<SymbolEntity> Symbols { get; set; } = null!;
    public DbSet<ExchangeSymbolEntity> ExchangeSymbols { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Update timestamps before saving
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("CreatedAt").CurrentValue == null)
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}