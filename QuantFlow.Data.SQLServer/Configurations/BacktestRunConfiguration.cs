//namespace QuantFlow.Data.SQLServer.Configurations;

///// <summary>
///// Entity Framework configuration for BacktestRunEntity
///// </summary>
//public class BacktestRunConfiguration : IEntityTypeConfiguration<BacktestRunEntity>
//{
//    public void Configure(EntityTypeBuilder<BacktestRunEntity> builder)
//    {
//        // Primary key
//        builder.HasKey(x => x.Id);

//        // Properties
//        builder.Property(x => x.Name)
//            .IsRequired()
//            .HasMaxLength(200);

//        builder.Property(x => x.Symbol)
//            .IsRequired()
//            .HasMaxLength(20);

//        builder.Property(x => x.AlgorithmParameters)
//            .HasColumnType("nvarchar(max)");

//        builder.Property(x => x.ErrorMessage)
//            .HasMaxLength(1000);

//        // Decimal precisions
//        builder.Property(x => x.InitialBalance)
//            .HasPrecision(18, 2);

//        builder.Property(x => x.FinalBalance)
//            .HasPrecision(18, 2);

//        builder.Property(x => x.TotalReturnPercent)
//            .HasPrecision(8, 4);

//        builder.Property(x => x.MaxDrawdownPercent)
//            .HasPrecision(8, 4);

//        builder.Property(x => x.SharpeRatio)
//            .HasPrecision(8, 4);

//        builder.Property(x => x.WinRatePercent)
//            .HasPrecision(5, 2);

//        builder.Property(x => x.AverageTradeReturnPercent)
//            .HasPrecision(8, 4);

//        builder.Property(x => x.CommissionRate)
//            .HasPrecision(8, 6);

//        // Indexes
//        builder.HasIndex(x => x.UserId)
//            .HasDatabaseName("IX_BacktestRuns_UserId");

//        builder.HasIndex(x => x.PortfolioId)
//            .HasDatabaseName("IX_BacktestRuns_PortfolioId");

//        builder.HasIndex(x => x.AlgorithmId)
//            .HasDatabaseName("IX_BacktestRuns_AlgorithmId");

//        builder.HasIndex(x => x.Status)
//            .HasDatabaseName("IX_BacktestRuns_Status");

//        builder.HasIndex(x => new { x.Symbol, x.CreatedAt })
//            .HasDatabaseName("IX_BacktestRuns_Symbol_CreatedAt");

//        // Default values
//        builder.Property(x => x.Status)
//            .HasDefaultValue(1); // BacktestStatus.Pending

//        builder.Property(x => x.IsDeleted)
//            .HasDefaultValue(false);

//        builder.Property(x => x.CreatedAt)
//            .HasDefaultValueSql("GETUTCDATE()");

//        // Soft delete filter
//        builder.HasQueryFilter(x => !x.IsDeleted);

//        // Relationships
//        builder.HasMany(x => x.Trades)
//            .WithOne(x => x.BacktestRun)
//            .HasForeignKey(x => x.BacktestRunId)
//            .OnDelete(DeleteBehavior.Cascade);
//    }
//}