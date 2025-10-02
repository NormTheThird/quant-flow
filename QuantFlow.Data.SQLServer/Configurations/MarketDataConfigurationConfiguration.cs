namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for MarketDataConfigurationEntity
/// </summary>
public class MarketDataConfigurationConfiguration : IEntityTypeConfiguration<MarketDataConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<MarketDataConfigurationEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.SymbolId)
            .IsRequired();

        builder.Property(x => x.Exchange)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Is1mActive)
            .HasDefaultValue(false);

        builder.Property(x => x.Is5mActive)
            .HasDefaultValue(false);

        builder.Property(x => x.Is15mActive)
            .HasDefaultValue(false);

        builder.Property(x => x.Is1hActive)
            .HasDefaultValue(false);

        builder.Property(x => x.Is4hActive)
            .HasDefaultValue(false);

        builder.Property(x => x.Is1dActive)
            .HasDefaultValue(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.SymbolId)
            .HasDatabaseName("IX_MarketDataConfiguration_SymbolId");

        builder.HasIndex(x => new { x.SymbolId, x.Exchange })
            .IsUnique()
            .HasDatabaseName("IX_MarketDataConfiguration_SymbolId_Exchange");

        builder.HasIndex(x => x.Exchange)
            .HasDatabaseName("IX_MarketDataConfiguration_Exchange");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Foreign key relationship
        builder.HasOne(x => x.Symbol)
            .WithMany()
            .HasForeignKey(x => x.SymbolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}