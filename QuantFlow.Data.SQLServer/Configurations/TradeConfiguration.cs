namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for TradeEntity
/// </summary>
public class TradeConfiguration : IEntityTypeConfiguration<TradeEntity>
{
    public void Configure(EntityTypeBuilder<TradeEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AlgorithmReason)
            .HasMaxLength(500);

        // Decimal precisions
        builder.Property(x => x.Quantity)
            .HasPrecision(18, 8);

        builder.Property(x => x.Price)
            .HasPrecision(18, 8);

        builder.Property(x => x.Value)
            .HasPrecision(18, 2);

        builder.Property(x => x.Commission)
            .HasPrecision(18, 6);

        builder.Property(x => x.NetValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.PortfolioBalanceBefore)
            .HasPrecision(18, 2);

        builder.Property(x => x.PortfolioBalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(x => x.AlgorithmConfidence)
            .HasPrecision(3, 2);

        builder.Property(x => x.RealizedProfitLoss)
            .HasPrecision(18, 2);

        builder.Property(x => x.RealizedProfitLossPercent)
            .HasPrecision(8, 4);

        // Indexes
        builder.HasIndex(x => x.BacktestRunId)
            .HasDatabaseName("IX_Trades_BacktestRunId");

        builder.HasIndex(x => x.ExecutionTimestamp)
            .HasDatabaseName("IX_Trades_ExecutionTimestamp");

        builder.HasIndex(x => new { x.Symbol, x.Type })
            .HasDatabaseName("IX_Trades_Symbol_Type");

        builder.HasIndex(x => x.EntryTradeId)
            .HasDatabaseName("IX_Trades_EntryTradeId");

        // Default values
        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Self-referencing relationship for entry trades
        builder.HasOne(x => x.EntryTrade)
            .WithMany()
            .HasForeignKey(x => x.EntryTradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}