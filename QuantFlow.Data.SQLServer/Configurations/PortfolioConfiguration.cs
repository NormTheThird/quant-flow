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
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.BaseCurrency)
            .IsRequired()
            .HasMaxLength(10);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Portfolios_UserId");

        // UNIQUE CONSTRAINT: UserId + Exchange + BaseCurrency
        builder.HasIndex(x => new { x.UserId, x.Exchange, x.BaseCurrency })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Portfolios_UserId_Exchange_BaseCurrency");

        // Default values
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
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AlgorithmPositions)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}