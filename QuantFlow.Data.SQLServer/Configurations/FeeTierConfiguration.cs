namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for FeeTierEntity
/// </summary>
public class FeeTierConfiguration : IEntityTypeConfiguration<FeeTierEntity>
{
    public void Configure(EntityTypeBuilder<FeeTierEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.MinimumVolumeThreshold)
            .HasPrecision(18, 2);

        builder.Property(x => x.MakerFeePercent)
            .HasPrecision(8, 5);

        builder.Property(x => x.TakerFeePercent)
            .HasPrecision(8, 5);

        // Indexes
        builder.HasIndex(x => x.ExchangeConfigurationId)
            .HasDatabaseName("IX_FeeTiers_ExchangeConfigurationId");

        builder.HasIndex(x => new { x.Exchange, x.TierLevel })
            .IsUnique()
            .HasDatabaseName("IX_FeeTiers_Exchange_TierLevel");

        builder.HasIndex(x => new { x.Exchange, x.MinimumVolumeThreshold })
            .HasDatabaseName("IX_FeeTiers_Exchange_VolumeThreshold");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_FeeTiers_IsActive");

        // Default values
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}