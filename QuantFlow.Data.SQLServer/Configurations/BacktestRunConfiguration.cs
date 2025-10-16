namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for BacktestRunEntity
/// </summary>
public class BacktestRunConfiguration : IEntityTypeConfiguration<BacktestRunEntity>
{
    public void Configure(EntityTypeBuilder<BacktestRunEntity> builder)
    {
        builder.HasKey(_ => _.Id);

        builder.Property(_ => _.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(_ => _.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(_ => _.AlgorithmParameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(_ => _.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(_ => _.UserId)
            .HasDatabaseName("IX_BacktestRun_UserId");

        builder.HasIndex(_ => _.Status)
            .HasDatabaseName("IX_BacktestRun_Status");

        builder.Property(_ => _.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(_ => _.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(_ => !_.IsDeleted);

        builder.HasOne(_ => _.User)
            .WithMany(_ => _.BacktestRuns)
            .HasForeignKey(_ => _.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}