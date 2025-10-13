namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for AlgorithmPositionEntity
/// </summary>
public class AlgorithmPositionConfiguration : IEntityTypeConfiguration<AlgorithmPositionEntity>
{
    public void Configure(EntityTypeBuilder<AlgorithmPositionEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.PositionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AllocatedPercent)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MaxPositionSizePercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.ExchangeFees)
            .HasPrecision(8, 6);

        builder.Property(x => x.CurrentValue)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(x => x.PortfolioId)
            .HasDatabaseName("IX_AlgorithmPositions_PortfolioId");

        builder.HasIndex(x => x.AlgorithmId)
            .HasDatabaseName("IX_AlgorithmPositions_AlgorithmId");

        // UNIQUE CONSTRAINT: PortfolioId + PositionName (within non-deleted)
        builder.HasIndex(x => new { x.PortfolioId, x.PositionName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_AlgorithmPositions_PortfolioId_PositionName");

        // Default values
        builder.Property(x => x.MaxPositionSizePercent)
            .HasDefaultValue(10.0m);

        builder.Property(x => x.ExchangeFees)
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
        builder.HasOne(x => x.Portfolio)
            .WithMany(x => x.AlgorithmPositions)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Trades)
            .WithOne()
            .HasForeignKey("AlgorithmPositionId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}