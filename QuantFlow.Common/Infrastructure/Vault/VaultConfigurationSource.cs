namespace QuantFlow.Common.Infrastructure.Vault;

public class VaultConfigurationSource : IConfigurationSource
{
    public string VaultUrl { get; set; } = string.Empty;
    public string MountPoint { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(VaultUrl, MountPoint, Token);
    }
}