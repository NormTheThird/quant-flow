namespace QuantFlow.Common.Infrastructure.Vault;

public class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly string _vaultUrl;
    private readonly string _mountPoint;
    private readonly string _token;

    public VaultConfigurationProvider(string vaultUrl, string mountPoint, string token)
    {
        _vaultUrl = vaultUrl;
        _mountPoint = mountPoint;
        _token = token;
    }

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        try
        {
            var authMethod = new TokenAuthMethodInfo(_token);
            var vaultClientSettings = new VaultClientSettings(_vaultUrl, authMethod);
            var vaultClient = new VaultClient(vaultClientSettings);

            var influxSecret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("influxdb", mountPoint: _mountPoint);
            Data["InfluxDb:Url"] = influxSecret.Data.Data["url"].ToString();
            Data["InfluxDb:Token"] = influxSecret.Data.Data["token"].ToString();
            Data["InfluxDb:Bucket"] = influxSecret.Data.Data["bucket"].ToString();
            Data["InfluxDb:Organization"] = influxSecret.Data.Data["organization"].ToString();

            var krakenSecret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("kraken", mountPoint: _mountPoint);
            Data["Kraken:ApiKey"] = krakenSecret.Data.Data["apikey"].ToString();
            Data["Kraken:ApiSecret"] = krakenSecret.Data.Data["apisecret"].ToString();

            var discordSecret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("discord", mountPoint: _mountPoint);
            Data["Discord:Token"] = discordSecret.Data.Data["token"].ToString();
            Data["Discord:ServerId"] = discordSecret.Data.Data["serverId"].ToString();
            Data["Discord:ClientId"] = discordSecret.Data.Data["clientId"].ToString();
            Data["Discord:ClientSecret"] = discordSecret.Data.Data["clientSecret"].ToString();
            Data["Discord:PTMemberRoleId"] = discordSecret.Data.Data["ptMemberRoleId"].ToString();
            Data["Discord:VerifiedRoleId"] = discordSecret.Data.Data["verifiedRoleId"].ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load Vault configuration: {ex.Message}");
            Console.WriteLine($"Exception details: {ex}");
        }
    }
}