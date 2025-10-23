namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for AlgorithmEffectivenessEntity
/// </summary>
public class AlgorithmEffectivenessConfiguration : IEntityTypeConfiguration<AlgorithmEffectivenessEntity>
{
    public void Configure(EntityTypeBuilder<AlgorithmEffectivenessEntity> builder)
    {
        builder.HasKey(_ => _.Id);

        builder.Property(_ => _.AlgorithmId)
            .IsRequired();

        builder.Property(_ => _.Timeframe)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(_ => _.ReliabilityStars)
            .IsRequired();

        builder.Property(_ => _.OpportunityStars)
            .IsRequired();

        builder.Property(_ => _.RecommendedStars)
            .IsRequired();

        builder.Property(_ => _.ReliabilityReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(_ => _.OpportunityReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(_ => _.AverageWinRate)
            .HasPrecision(5, 4);

        builder.Property(_ => _.AverageReturnPerTrade)
            .HasPrecision(6, 4);

        builder.Property(_ => _.AverageSharpeRatio)
            .HasPrecision(5, 2);

        builder.Property(_ => _.AverageMaxDrawdown)
            .HasPrecision(5, 4);

        builder.Property(_ => _.AverageStopLossPercent)
            .HasPrecision(5, 4);

        builder.Property(_ => _.UpdatedBy)
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(_ => _.CreatedBy)
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(_ => _.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(_ => _.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(_ => new { _.AlgorithmId, _.Timeframe })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_AlgorithmEffectiveness_AlgorithmId_Timeframe");

        builder.HasIndex(_ => _.RecommendedStars)
            .HasDatabaseName("IX_AlgorithmEffectiveness_RecommendedStars");

        builder.HasQueryFilter(_ => !_.IsDeleted);

        builder.HasOne(_ => _.Algorithm)
            .WithMany()
            .HasForeignKey(_ => _.AlgorithmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}