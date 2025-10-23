namespace QuantFlow.Data.SQLServer.Configurations;

/// <summary>
/// Entity Framework configuration for AlgorithmEntity
/// </summary>
public class AlgorithmConfiguration : IEntityTypeConfiguration<AlgorithmEntity>
{
    public void Configure(EntityTypeBuilder<AlgorithmEntity> builder)
    {
        builder.HasKey(_ => _.Id);

        builder.Property(_ => _.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(_ => _.Abbreviation)
            .HasMaxLength(20);

        builder.Property(_ => _.Description)
            .IsRequired();

        builder.Property(_ => _.AlgorithmType)
            .IsRequired();

        builder.Property(_ => _.AlgorithmSource)
            .IsRequired();

        builder.Property(_ => _.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(_ => _.Version)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("1.0");

        builder.Property(_ => _.CreatedBy)
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(_ => _.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(_ => _.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(_ => _.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(_ => _.Name)
            .HasDatabaseName("IX_Algorithms_Name");

        builder.HasIndex(_ => _.AlgorithmSource)
            .HasDatabaseName("IX_Algorithms_AlgorithmSource");

        builder.HasIndex(_ => _.IsEnabled)
            .HasDatabaseName("IX_Algorithms_IsEnabled");

        builder.HasQueryFilter(_ => !_.IsDeleted);
    }
}