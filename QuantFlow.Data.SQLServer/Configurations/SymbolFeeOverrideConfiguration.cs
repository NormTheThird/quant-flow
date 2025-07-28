namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for SymbolFeeOverrideEntity
/// </summary>
public class SymbolFeeOverrideConfiguration : IEntityTypeConfiguration<SymbolFeeOverrideEntity>
{
    public void Configure(EntityTypeBuilder<SymbolFeeOverrideEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.MakerFeePercent)
            .HasPrecision(8, 5);

        builder.Property(x => x.TakerFeePercent)
            .HasPrecision(8, 5);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.ExchangeConfigurationId)
            .HasDatabaseName("IX_SymbolFeeOverrides_ExchangeConfigurationId");

        builder.HasIndex(x => new { x.Exchange, x.Symbol })
            .IsUnique()
            .HasDatabaseName("IX_SymbolFeeOverrides_Exchange_Symbol");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_SymbolFeeOverrides_IsActive");

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