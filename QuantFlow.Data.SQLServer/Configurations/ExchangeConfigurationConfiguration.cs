namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for ExchangeConfigurationEntity
/// </summary>
public class ExchangeConfigurationConfiguration : IEntityTypeConfiguration<ExchangeConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<ExchangeConfigurationEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ApiEndpoint)
            .HasMaxLength(200);

        builder.Property(x => x.BaseMakerFeePercent)
            .HasPrecision(8, 5);

        builder.Property(x => x.BaseTakerFeePercent)
            .HasPrecision(8, 5);

        // Indexes
        builder.HasIndex(x => x.Exchange)
            .IsUnique()
            .HasDatabaseName("IX_ExchangeConfigurations_Exchange");

        builder.HasIndex(x => new { x.IsActive, x.IsSupported })
            .HasDatabaseName("IX_ExchangeConfigurations_Active_Supported");

        // Default values
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsSupported)
            .HasDefaultValue(true);

        builder.Property(x => x.MaxRequestsPerMinute)
            .HasDefaultValue(60);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        builder.HasMany(x => x.FeeTiers)
            .WithOne(x => x.ExchangeConfiguration)
            .HasForeignKey(x => x.ExchangeConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SymbolOverrides)
            .WithOne(x => x.ExchangeConfiguration)
            .HasForeignKey(x => x.ExchangeConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}