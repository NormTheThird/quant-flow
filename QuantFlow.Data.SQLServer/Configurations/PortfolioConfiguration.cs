namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for PortfolioEntity
/// </summary>
public class PortfolioConfiguration : IEntityTypeConfiguration<PortfolioEntity>
{
    public void Configure(EntityTypeBuilder<PortfolioEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.InitialBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Mode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Exchange)
            .HasMaxLength(50);

        builder.Property(x => x.MaxPositionSizePercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.CommissionRate)
            .HasPrecision(8, 6);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Portfolios_UserId");

        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Portfolios_UserId_Name");

        builder.HasIndex(x => new { x.UserId, x.Mode, x.Exchange })
            .HasDatabaseName("IX_Portfolios_UserId_Mode_Exchange");

        // Default values
        builder.Property(x => x.Status)
            .HasDefaultValue("Inactive");

        builder.Property(x => x.Mode)
            .HasDefaultValue("TestMode");

        builder.Property(x => x.MaxPositionSizePercent)
            .HasDefaultValue(10.0m);

        builder.Property(x => x.CommissionRate)
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
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserExchangeDetails)
            .WithMany()
            .HasForeignKey(x => x.UserExchangeDetailsId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}