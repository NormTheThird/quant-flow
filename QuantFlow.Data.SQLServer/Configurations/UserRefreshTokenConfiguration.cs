namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for UserRefreshTokenEntity
/// </summary>
public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<UserRefreshTokenEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.TokenType)
            .HasMaxLength(50);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("IX_UserRefreshToken_Token");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserRefreshToken_UserId");

        builder.HasIndex(x => new { x.UserId, x.TokenType })
            .HasDatabaseName("IX_UserRefreshToken_UserId_TokenType");

        // Default values
        builder.Property(x => x.IsRevoked)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Foreign key relationship
        builder.HasOne(x => x.User)
            .WithMany() // Add navigation property to UserEntity if needed
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}