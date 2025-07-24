namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for PortfolioEntity
/// </summary>
public class PortfolioConfiguration : IEntityTypeConfiguration<PortfolioEntity>
{
    public void Configure(EntityTypeBuilder<PortfolioEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.InitialBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.MaxPositionSizePercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.CommissionRate)
            .HasPrecision(8, 6);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Portfolios_UserId");

        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Portfolios_UserId_Name");

        // Default values
        builder.Property(x => x.Status)
            .HasDefaultValue(1); // PortfolioStatus.Active

        builder.Property(x => x.MaxPositionSizePercent)
            .HasDefaultValue(10.0m);

        builder.Property(x => x.CommissionRate)
            .HasDefaultValue(0.001m);

        builder.Property(x => x.AllowShortSelling)
            .HasDefaultValue(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        builder.HasMany(x => x.BacktestRuns)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}