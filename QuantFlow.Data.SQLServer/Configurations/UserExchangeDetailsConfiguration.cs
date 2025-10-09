namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for UserExchangeDetailsEntity
/// </summary>
public class UserExchangeDetailsConfiguration : IEntityTypeConfiguration<UserExchangeDetailsEntity>
{
    public void Configure(EntityTypeBuilder<UserExchangeDetailsEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Exchange)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.KeyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.KeyValue)
            .IsRequired()
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(x => new { x.UserId, x.Exchange, x.KeyName })
            .IsUnique()
            .HasDatabaseName("IX_UserExchangeDetails_UserId_Exchange_KeyName");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserExchangeDetails_UserId");

        // Default values
        builder.Property(x => x.IsEncrypted)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}