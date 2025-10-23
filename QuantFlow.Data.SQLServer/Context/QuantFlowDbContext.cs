namespace QuantFlow.Data.SQLServer.Context;

/// <summary>
/// Entity Framework Database Context for QuantFlow application
/// </summary>
public class QuantFlowDbContext : DbContext
{
    public QuantFlowDbContext(DbContextOptions<QuantFlowDbContext> options) : base(options)
    {
    }
    public DbSet<AlgorithmEffectivenessEntity> AlgorithmEffectiveness { get; set; }
    public DbSet<AlgorithmEntity> Algorithms { get; set; }
    public DbSet<AlgorithmPositionEntity> AlgorithmPositions { get; set; } = null!;
    public DbSet<BacktestRunEntity> BacktestRuns { get; set; } = null!;
    public DbSet<MarketDataConfigurationEntity> MarketDataConfigurations { get; set; } = null!;
    public DbSet<PortfolioEntity> Portfolios { get; set; } = null!;
    public DbSet<SymbolEntity> Symbols { get; set; } = null!;
    public DbSet<TradeEntity> Trades { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<UserExchangeDetailsEntity> UserExchangeDetails { get; set; } = null!;
    public DbSet<UserRefreshTokenEntity> UserRefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isDeletedProperty = entityType.FindProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, isDeletedProperty.PropertyInfo!),
                    Expression.Constant(false));

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    Expression.Lambda(body, parameter));
            }
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit fields for all modified entities
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("CreatedAt").CurrentValue == null)
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;

                if (string.IsNullOrEmpty(entry.Property("CreatedBy").CurrentValue?.ToString()))
                    entry.Property("CreatedBy").CurrentValue = "System";
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;

                if (string.IsNullOrEmpty(entry.Property("UpdatedBy").CurrentValue?.ToString()))
                    entry.Property("UpdatedBy").CurrentValue = "System";
            }
        }
    }
}