namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for SymbolEntity
/// </summary>
public class SymbolConfiguration : IEntityTypeConfiguration<SymbolEntity>
{
    public void Configure(EntityTypeBuilder<SymbolEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.BaseAsset)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.QuoteAsset)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.MinTradeAmount)
            .HasPrecision(18, 8);

        // Indexes
        builder.HasIndex(x => x.Symbol)
            .IsUnique()
            .HasDatabaseName("IX_Symbols_Symbol");

        builder.HasIndex(x => new { x.BaseAsset, x.QuoteAsset })
            .HasDatabaseName("IX_Symbols_BaseAsset_QuoteAsset");

        // Default values
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.PricePrecision)
            .HasDefaultValue(8);

        builder.Property(x => x.QuantityPrecision)
            .HasDefaultValue(8);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}