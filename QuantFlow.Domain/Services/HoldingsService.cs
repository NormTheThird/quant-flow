namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing user holdings across exchanges
/// </summary>
public class HoldingsService : IHoldingsService
{
    private readonly ILogger<HoldingsService> _logger;
    private readonly IKrakenApiService _krakenApiService;
    private readonly IUserExchangeDetailsService _userExchangeDetailsService;
    private readonly IEncryptionService _encryptionService;

    public HoldingsService(ILogger<HoldingsService> logger, IKrakenApiService krakenApiService, IUserExchangeDetailsService userExchangeDetailsService,
                           IEncryptionService encryptionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _krakenApiService = krakenApiService ?? throw new ArgumentNullException(nameof(krakenApiService));
        _userExchangeDetailsService = userExchangeDetailsService ?? throw new ArgumentNullException(nameof(userExchangeDetailsService));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    public async Task<HoldingsSummary> GetKrakenHoldingsAsync(Guid userId)
    {
        _logger.LogInformation("Getting Kraken holdings for user: {UserId}", userId);

        try
        {
            // Check if user has Kraken credentials
            var exchangeDetailsList = await _userExchangeDetailsService.GetByUserIdAndExchangeAsync(userId, Exchange.Kraken);

            if (exchangeDetailsList == null || !exchangeDetailsList.Any())
            {
                _logger.LogInformation("User {UserId} has no Kraken credentials", userId);
                return new HoldingsSummary
                {
                    HasCredentials = false,
                    ErrorMessage = "No Kraken API credentials found. Please add your credentials in Settings."
                };
            }

            // Extract API Key and Secret from the key-value pairs
            var apiKeyDetail = exchangeDetailsList.FirstOrDefault(d => d.KeyName == "ApiKey");
            var apiSecretDetail = exchangeDetailsList.FirstOrDefault(d => d.KeyName == "ApiSecret");

            if (apiKeyDetail == null || apiSecretDetail == null)
            {
                _logger.LogWarning("Incomplete Kraken credentials for user {UserId}", userId);
                return new HoldingsSummary
                {
                    HasCredentials = false,
                    ErrorMessage = "Incomplete Kraken API credentials. Please update your credentials in Settings."
                };
            }

            var apiKey = apiKeyDetail.IsEncrypted ? _encryptionService.Decrypt(apiKeyDetail.KeyValue) : apiKeyDetail.KeyValue;
            var apiSecret = apiSecretDetail.IsEncrypted ? _encryptionService.Decrypt(apiSecretDetail.KeyValue) : apiSecretDetail.KeyValue;

            _krakenApiService.SetCredentials(apiKey, apiSecret);

            try
            {
                // Get account balances
                var balances = await _krakenApiService.GetAccountBalanceAsync();

                if (balances == null || balances.Count == 0)
                {
                    _logger.LogInformation("No balances found for user {UserId}", userId);
                    return new HoldingsSummary
                    {
                        HasCredentials = true,
                        TotalUsdValue = 0
                    };
                }

                // Filter out dust and build holdings list
                var holdings = new List<HoldingModel>();
                decimal totalUsdValue = 0;

                foreach (var balance in balances.Where(b => b.Value > 0))
                {
                    var asset = CleanAssetName(balance.Key);
                    var quantity = balance.Value;

                    // Get USD price for this asset
                    var usdPrice = await GetUsdPriceAsync(asset);
                    var usdValue = quantity * usdPrice;

                    // Filter out holdings worth less than $1
                    if (usdValue >= 1.0m)
                    {
                        holdings.Add(new HoldingModel
                        {
                            Asset = asset,
                            Quantity = quantity,
                            UsdPrice = usdPrice,
                            UsdValue = usdValue,
                            Change24h = 0
                        });

                        totalUsdValue += usdValue;
                    }
                }

                // Calculate percentage of total for each holding
                foreach (var holding in holdings)
                {
                    holding.PercentageOfTotal = totalUsdValue > 0
                        ? (holding.UsdValue / totalUsdValue) * 100
                        : 0;
                }

                // Sort by USD value descending
                holdings = holdings.OrderByDescending(h => h.UsdValue).ToList();

                _logger.LogInformation("Found {Count} holdings worth ${TotalValue} for user {UserId}",
                    holdings.Count, totalUsdValue, userId);

                return new HoldingsSummary
                {
                    Holdings = holdings,
                    TotalUsdValue = totalUsdValue,
                    HasCredentials = true
                };
            }
            finally
            {
                // Always clear credentials when done
                _krakenApiService.ClearCredentials();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Kraken holdings for user {UserId}", userId);
            return new HoldingsSummary
            {
                HasCredentials = true,
                ErrorMessage = $"Error fetching holdings: {ex.Message}"
            };
        }
    }

    private async Task<decimal> GetUsdPriceAsync(string asset)
    {
        try
        {
            // Special case for USD/USDT/USDC
            if (asset == "USD" || asset == "USDT" || asset == "USDC")
                return 1.0m;

            // Get ticker from Kraken for this asset against USD
            var pair = $"{asset}USD";
            var ticker = await _krakenApiService.GetTickerAsync(pair);

            if (ticker != null && ticker.LastPrice > 0)
                return ticker.LastPrice;

            // Try USDT pair if USD doesn't work
            pair = $"{asset}USDT";
            ticker = await _krakenApiService.GetTickerAsync(pair);

            if (ticker != null && ticker.LastPrice > 0)
                return ticker.LastPrice;

            _logger.LogWarning("Could not get USD price for asset: {Asset}", asset);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting USD price for asset: {Asset}", asset);
            return 0;
        }
    }

    private string CleanAssetName(string krakenAsset)
    {
        // Remove staking suffixes (.S for staked, .F for flexible staking, .P for parachain)
        var asset = krakenAsset;

        if (asset.EndsWith(".S") || asset.EndsWith(".F") || asset.EndsWith(".P") || asset.EndsWith(".M"))
        {
            asset = asset.Substring(0, asset.LastIndexOf('.'));
        }

        // Kraken prefixes assets with X or Z
        // Remove these prefixes for display
        if (asset.StartsWith("X") || asset.StartsWith("Z"))
        {
            var cleaned = asset.Substring(1);

            // Special cases
            if (cleaned == "XBT") return "BTC";
            if (cleaned == "XDG") return "DOGE";

            return cleaned;
        }

        return asset;
    }
}