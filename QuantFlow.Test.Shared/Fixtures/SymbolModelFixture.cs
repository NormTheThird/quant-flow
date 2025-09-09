namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture class for creating SymbolModel test data
/// </summary>
public static class SymbolModelFixture
{
    /// <summary>
    /// Creates a default BTCUSDT symbol model for testing
    /// </summary>
    public static SymbolModel CreateBTCUSDT()
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            BaseAsset = "BTC",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 0.001m,
            PricePrecision = 2,
            QuantityPrecision = 8,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default ETHUSDT symbol model for testing
    /// </summary>
    public static SymbolModel CreateETHUSDT()
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "ETHUSDT",
            BaseAsset = "ETH",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 0.01m,
            PricePrecision = 2,
            QuantityPrecision = 6,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default ADAUSDT symbol model for testing
    /// </summary>
    public static SymbolModel CreateADAUSDT()
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "ADAUSDT",
            BaseAsset = "ADA",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 1.0m,
            PricePrecision = 4,
            QuantityPrecision = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default symbol model with custom parameters
    /// </summary>
    public static SymbolModel CreateDefault(string symbol = "BTCUSDT", string baseAsset = "BTC", string quoteAsset = "USDT",
        bool isActive = true, decimal minTradeAmount = 0.001m, int pricePrecision = 2, int quantityPrecision = 8)
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = symbol,
            BaseAsset = baseAsset,
            QuoteAsset = quoteAsset,
            IsActive = isActive,
            MinTradeAmount = minTradeAmount,
            PricePrecision = pricePrecision,
            QuantityPrecision = quantityPrecision,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates an inactive symbol model for testing
    /// </summary>
    public static SymbolModel CreateInactiveSymbol(string symbol = "DELISTEDCOIN")
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = symbol,
            BaseAsset = symbol.Replace("USDT", ""),
            QuoteAsset = "USDT",
            IsActive = false,
            MinTradeAmount = 1.0m,
            PricePrecision = 4,
            QuantityPrecision = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            CreatedBy = "system",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "admin"
        };
    }

    /// <summary>
    /// Creates a custom symbol model with specific parameters
    /// </summary>
    public static SymbolModel CreateCustomSymbol(string baseAsset, string quoteAsset, bool isActive = true,
        decimal minTradeAmount = 0.001m, int pricePrecision = 2, int quantityPrecision = 8)
    {
        return new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = $"{baseAsset}{quoteAsset}",
            BaseAsset = baseAsset,
            QuoteAsset = quoteAsset,
            IsActive = isActive,
            MinTradeAmount = minTradeAmount,
            PricePrecision = pricePrecision,
            QuantityPrecision = quantityPrecision,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a batch of symbols with the same quote asset for testing
    /// </summary>
    public static List<SymbolModel> CreateSymbolBatchByQuoteAsset(string quoteAsset = "USDT", string[]? baseAssets = null, bool allActive = true)
    {
        baseAssets ??= new[] { "BTC", "ETH", "ADA", "DOT", "LINK" };
        var symbols = new List<SymbolModel>();

        foreach (var baseAsset in baseAssets)
        {
            symbols.Add(CreateCustomSymbol(
                baseAsset: baseAsset,
                quoteAsset: quoteAsset,
                isActive: allActive,
                minTradeAmount: GetDefaultMinTradeAmount(baseAsset),
                pricePrecision: GetDefaultPricePrecision(baseAsset),
                quantityPrecision: GetDefaultQuantityPrecision(baseAsset)
            ));
        }

        return symbols;
    }

    /// <summary>
    /// Creates a batch of symbols with the same base asset for testing
    /// </summary>
    public static List<SymbolModel> CreateSymbolBatchByBaseAsset(string baseAsset = "BTC", string[]? quoteAssets = null, bool allActive = true)
    {
        quoteAssets ??= new[] { "USDT", "EUR", "BNB", "ETH" };
        var symbols = new List<SymbolModel>();

        foreach (var quoteAsset in quoteAssets)
        {
            symbols.Add(CreateCustomSymbol(
                baseAsset: baseAsset,
                quoteAsset: quoteAsset,
                isActive: allActive,
                minTradeAmount: GetDefaultMinTradeAmount(baseAsset),
                pricePrecision: GetDefaultPricePrecision(baseAsset),
                quantityPrecision: GetDefaultQuantityPrecision(baseAsset)
            ));
        }

        return symbols;
    }

    /// <summary>
    /// Creates a mixed batch of active and inactive symbols for testing
    /// </summary>
    public static List<SymbolModel> CreateMixedStatusSymbolBatch()
    {
        return new List<SymbolModel>
        {
            CreateBTCUSDT(), // Active
            CreateETHUSDT(), // Active
            CreateCustomSymbol("ADA", "USDT", isActive: true), // Active
            CreateCustomSymbol("DOGE", "USDT", isActive: false), // Inactive
            CreateCustomSymbol("SHIB", "USDT", isActive: false) // Inactive
        };
    }

    /// <summary>
    /// Creates a comprehensive symbol batch for extensive testing
    /// </summary>
    public static List<SymbolModel> CreateComprehensiveSymbolBatch()
    {
        var symbols = new List<SymbolModel>();

        // Major cryptocurrencies with USDT
        symbols.AddRange(CreateSymbolBatchByQuoteAsset("USDT", new[] { "BTC", "ETH", "BNB", "ADA", "DOT" }));

        // BTC pairs with different quote assets
        symbols.AddRange(CreateSymbolBatchByBaseAsset("BTC", new[] { "EUR", "GBP" }));

        // ETH pairs with different quote assets
        symbols.AddRange(CreateSymbolBatchByBaseAsset("ETH", new[] { "BTC", "BNB" }));

        // Some inactive symbols
        symbols.Add(CreateInactiveSymbol("OLDCOINUSDT"));
        symbols.Add(CreateInactiveSymbol("DELISTEDUSDT"));

        return symbols;
    }

    /// <summary>
    /// Creates a symbol for update testing
    /// </summary>
    public static SymbolModel CreateSymbolForUpdate(Guid symbolId, string symbol = "BTCUSDT", bool isActive = false,
        decimal minTradeAmount = 0.01m, int pricePrecision = 4, int quantityPrecision = 6)
    {
        // Parse base and quote assets properly
        var (baseAsset, quoteAsset) = ParseSymbolAssets(symbol);

        return new SymbolModel
        {
            Id = symbolId,
            Symbol = symbol,
            BaseAsset = baseAsset,
            QuoteAsset = quoteAsset,
            IsActive = isActive,
            MinTradeAmount = minTradeAmount,
            PricePrecision = pricePrecision,
            QuantityPrecision = quantityPrecision,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }

    /// <summary>
    /// Parses a symbol string to extract base and quote assets
    /// </summary>
    private static (string baseAsset, string quoteAsset) ParseSymbolAssets(string symbol)
    {
        // Common quote assets ordered by length (longest first to avoid partial matches)
        var commonQuoteAssets = new[] { "USDT", "BUSD", "USDC", "BTC", "ETH", "BNB", "EUR", "GBP", "AUD", "USD" };

        foreach (var quoteAsset in commonQuoteAssets)
        {
            if (symbol.EndsWith(quoteAsset, StringComparison.OrdinalIgnoreCase))
            {
                var baseAsset = symbol.Substring(0, symbol.Length - quoteAsset.Length);
                if (!string.IsNullOrEmpty(baseAsset))
                {
                    return (baseAsset.ToUpperInvariant(), quoteAsset.ToUpperInvariant());
                }
            }
        }

        // Fallback - assume it's a USDT pair if we can't parse it
        return (symbol.Replace("USDT", "").ToUpperInvariant(), "USDT");
    }

    /// <summary>
    /// Gets default minimum trade amount based on base asset
    /// </summary>
    private static decimal GetDefaultMinTradeAmount(string baseAsset)
    {
        return baseAsset switch
        {
            "BTC" => 0.001m,
            "ETH" => 0.01m,
            "BNB" => 0.1m,
            "ADA" => 1.0m,
            "DOT" => 0.1m,
            "LINK" => 0.1m,
            "DOGE" => 100.0m,
            "SHIB" => 10000.0m,
            _ => 0.001m
        };
    }

    /// <summary>
    /// Gets default price precision based on base asset
    /// </summary>
    private static int GetDefaultPricePrecision(string baseAsset)
    {
        return baseAsset switch
        {
            "BTC" => 2,
            "ETH" => 2,
            "BNB" => 3,
            "ADA" => 4,
            "DOT" => 3,
            "LINK" => 3,
            "DOGE" => 6,
            "SHIB" => 8,
            _ => 2
        };
    }

    /// <summary>
    /// Gets default quantity precision based on base asset
    /// </summary>
    private static int GetDefaultQuantityPrecision(string baseAsset)
    {
        return baseAsset switch
        {
            "BTC" => 8,
            "ETH" => 6,
            "BNB" => 4,
            "ADA" => 2,
            "DOT" => 4,
            "LINK" => 4,
            "DOGE" => 1,
            "SHIB" => 0,
            _ => 8
        };
    }
}