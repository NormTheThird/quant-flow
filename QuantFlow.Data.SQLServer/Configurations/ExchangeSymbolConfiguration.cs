namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for ExchangeSymbolEntity
/// </summary>
public class ExchangeSymbolConfiguration : IEntityTypeConfiguration<ExchangeSymbolEntity>
{
    public void Configure(EntityTypeBuilder<ExchangeSymbolEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.ExchangeSymbolName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ApiEndpoint)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(x => x.SymbolId)
            .HasDatabaseName("IX_ExchangeSymbols_SymbolId");

        builder.HasIndex(x => new { x.SymbolId, x.Exchange })
            .IsUnique()
            .HasDatabaseName("IX_ExchangeSymbols_SymbolId_Exchange");

        builder.HasIndex(x => x.Exchange)
            .HasDatabaseName("IX_ExchangeSymbols_Exchange");

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