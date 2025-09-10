namespace QuantFlow.Common.Infrastructure.Vault;

public static class VaultConfigurationExtensions
{
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder builder,
        string vaultUrl, string mountPoint, string token)
    {
        return builder.Add(new VaultConfigurationSource
        {
            VaultUrl = vaultUrl,
            MountPoint = mountPoint,
            Token = token
        });
    }
}