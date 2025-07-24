namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for SubscriptionEntity
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Type)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Subscriptions_UserId");

        builder.HasIndex(x => new { x.UserId, x.IsActive })
            .HasDatabaseName("IX_Subscriptions_UserId_IsActive");

        builder.HasIndex(x => x.EndDate)
            .HasDatabaseName("IX_Subscriptions_EndDate");

        // Default values
        builder.Property(x => x.Type)
            .HasDefaultValue(1); // SubscriptionType.Free

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.MaxPortfolios)
            .HasDefaultValue(1);

        builder.Property(x => x.MaxAlgorithms)
            .HasDefaultValue(5);

        builder.Property(x => x.MaxBacktestRuns)
            .HasDefaultValue(10);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}