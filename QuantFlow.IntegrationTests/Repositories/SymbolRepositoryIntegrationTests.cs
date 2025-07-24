namespace QuantFlow.Test.Integration.Repositories;

/// <summary>
/// Integration tests for SymbolRepository with in-memory database
/// </summary>
public class SymbolRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly SymbolRepository _repository;

    public SymbolRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<SymbolRepository>>();
        _repository = new SymbolRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSymbol_ReturnsSymbolModel()
    {
        // Arrange
        var symbolId = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");

        // Act
        var result = await _repository.GetByIdAsync(symbolId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbolId, result.Id);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal("BTC", result.BaseAsset);
        Assert.Equal("USDT", result.QuoteAsset);
        Assert.True(result.IsActive);
        Assert.Equal(0.001m, result.MinTradeAmount);
        Assert.Equal(2, result.PricePrecision);
        Assert.Equal(8, result.QuantityPrecision);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentSymbol_ReturnsNull()
    {
        // Arrange
        var symbolId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(symbolId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedSymbol_ReturnsNull()
    {
        // Arrange
        var symbolId = await SeedTestSymbolAsync();

        // Soft delete the symbol
        var symbol = await Context.Symbols.FindAsync(symbolId);
        symbol!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(symbolId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolModel()
    {
        // Arrange
        var symbolName = "ETHUSDT";
        await SeedTestSymbolAsync(symbolName, "ETH", "USDT");

        // Act
        var result = await _repository.GetBySymbolAsync(symbolName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbolName, result.Symbol);
        Assert.Equal("ETH", result.BaseAsset);
        Assert.Equal("USDT", result.QuoteAsset);
    }

    [Fact]
    public async Task GetBySymbolAsync_NonExistentSymbol_ReturnsNull()
    {
        // Arrange
        var symbolName = "NONEXISTENT";

        // Act
        var result = await _repository.GetBySymbolAsync(symbolName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_WithActiveAndInactiveSymbols_ReturnsActiveSymbolsOnly()
    {
        // Arrange
        var activeSymbolId1 = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        var activeSymbolId2 = await SeedTestSymbolAsync("ETHUSDT", "ETH", "USDT");
        var inactiveSymbolId = await SeedTestSymbolAsync("ADAUSDT", "ADA", "USDT");

        // Make one symbol inactive
        var inactiveSymbol = await Context.Symbols.FindAsync(inactiveSymbolId);
        inactiveSymbol!.IsActive = false;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        var symbols = result.ToList();
        Assert.Equal(2, symbols.Count);
        Assert.All(symbols, s => Assert.True(s.IsActive));
        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
        Assert.DoesNotContain(symbols, s => s.Symbol == "ADAUSDT");
    }

    [Fact]
    public async Task GetByBaseAssetAsync_ExistingBaseAsset_ReturnsMatchingSymbols()
    {
        // Arrange
        await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        await SeedTestSymbolAsync("BTCEUR", "BTC", "EUR");
        await SeedTestSymbolAsync("ETHUSDT", "ETH", "USDT");

        // Act
        var result = await _repository.GetByBaseAssetAsync("BTC");

        // Assert
        var symbols = result.ToList();
        Assert.Equal(2, symbols.Count);
        Assert.All(symbols, s => Assert.Equal("BTC", s.BaseAsset));
        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbols, s => s.Symbol == "BTCEUR");
        Assert.DoesNotContain(symbols, s => s.Symbol == "ETHUSDT");
    }

    [Fact]
    public async Task GetByQuoteAssetAsync_ExistingQuoteAsset_ReturnsMatchingSymbols()
    {
        // Arrange
        await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        await SeedTestSymbolAsync("ETHUSDT", "ETH", "USDT");
        await SeedTestSymbolAsync("BTCEUR", "BTC", "EUR");

        // Act
        var result = await _repository.GetByQuoteAssetAsync("USDT");

        // Assert
        var symbols = result.ToList();
        Assert.Equal(2, symbols.Count);
        Assert.All(symbols, s => Assert.Equal("USDT", s.QuoteAsset));
        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
        Assert.DoesNotContain(symbols, s => s.Symbol == "BTCEUR");
    }

    [Fact]
    public async Task CreateAsync_ValidSymbol_ReturnsCreatedSymbol()
    {
        // Arrange
        var symbolModel = new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "ADAUSDT",
            BaseAsset = "ADA",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 10.0m,
            PricePrecision = 4,
            QuantityPrecision = 2
        };

        // Act
        var result = await _repository.CreateAsync(symbolModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbolModel.Symbol, result.Symbol);
        Assert.Equal(symbolModel.BaseAsset, result.BaseAsset);
        Assert.Equal(symbolModel.QuoteAsset, result.QuoteAsset);
        Assert.Equal(symbolModel.IsActive, result.IsActive);
        Assert.Equal(symbolModel.MinTradeAmount, result.MinTradeAmount);
        Assert.Equal(symbolModel.PricePrecision, result.PricePrecision);
        Assert.Equal(symbolModel.QuantityPrecision, result.QuantityPrecision);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbSymbol = await Context.Symbols.FindAsync(result.Id);
        Assert.NotNull(dbSymbol);
        Assert.Equal(symbolModel.Symbol, dbSymbol.Symbol);
        Assert.Equal(symbolModel.BaseAsset, dbSymbol.BaseAsset);
        Assert.Equal(symbolModel.QuoteAsset, dbSymbol.QuoteAsset);
    }

    [Fact]
    public async Task UpdateAsync_ExistingSymbol_ReturnsUpdatedSymbol()
    {
        // Arrange
        var symbolId = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");

        var updatedModel = new SymbolModel
        {
            Id = symbolId,
            Symbol = "BTCUSDT",
            BaseAsset = "BTC",
            QuoteAsset = "USDT",
            IsActive = false, // Updated to inactive
            MinTradeAmount = 0.01m, // Updated minimum trade amount  
            PricePrecision = 4, // Updated precision
            QuantityPrecision = 6 // Updated precision
        };

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal("BTC", result.BaseAsset);
        Assert.Equal("USDT", result.QuoteAsset);
        Assert.False(result.IsActive);
        Assert.Equal(0.01m, result.MinTradeAmount);
        Assert.Equal(4, result.PricePrecision);
        Assert.Equal(6, result.QuantityPrecision);
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbSymbol = await Context.Symbols.FindAsync(symbolId);
        Assert.NotNull(dbSymbol);
        Assert.False(dbSymbol.IsActive);
        Assert.Equal(0.01m, dbSymbol.MinTradeAmount);
        Assert.Equal(4, dbSymbol.PricePrecision);
        Assert.Equal(6, dbSymbol.QuantityPrecision);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentSymbol_ThrowsNotFoundException()
    {
        // Arrange
        var symbolModel = new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "NONEXISTENT",
            BaseAsset = "NON",
            QuoteAsset = "EXISTENT",
            IsActive = true,
            MinTradeAmount = 1.0m,
            PricePrecision = 2,
            QuantityPrecision = 8
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(symbolModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingSymbol_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var symbolId = await SeedTestSymbolAsync();

        // Act
        var result = await _repository.DeleteAsync(symbolId);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbSymbol = await Context.Symbols.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == symbolId);
        Assert.NotNull(dbSymbol);
        Assert.True(dbSymbol.IsDeleted);
        Assert.NotNull(dbSymbol.UpdatedAt);

        // Verify symbol is not returned by normal queries
        var symbolModel = await _repository.GetByIdAsync(symbolId);
        Assert.Null(symbolModel);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentSymbol_ReturnsFalse()
    {
        // Arrange
        var symbolId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(symbolId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_WithSymbols_ReturnsAllActiveSymbols()
    {
        // Arrange
        var symbol1Id = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        var symbol2Id = await SeedTestSymbolAsync("ETHUSDT", "ETH", "USDT");
        var deletedSymbolId = await SeedTestSymbolAsync("ADAUSDT", "ADA", "USDT");

        // Soft delete one symbol
        var deletedSymbol = await Context.Symbols.FindAsync(deletedSymbolId);
        deletedSymbol!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var symbols = result.ToList();
        Assert.Equal(2, symbols.Count); // Only active symbols
        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
        Assert.DoesNotContain(symbols, s => s.Symbol == "ADAUSDT");
    }

    [Fact]
    public async Task GetAllAsync_NoSymbols_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSymbol_ThrowsException()
    {
        // Arrange
        await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");

        var duplicateSymbol = new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "BTCUSDT", // Same symbol name
            BaseAsset = "BTC",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 0.001m,
            PricePrecision = 2,
            QuantityPrecision = 8
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicateSymbol));
    }

    [Fact]
    public async Task GetByBaseAssetAsync_InactiveSymbols_ReturnsOnlyActiveSymbols()
    {
        // Arrange
        var activeSymbolId = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        var inactiveSymbolId = await SeedTestSymbolAsync("BTCEUR", "BTC", "EUR");

        // Make one symbol inactive
        var inactiveSymbol = await Context.Symbols.FindAsync(inactiveSymbolId);
        inactiveSymbol!.IsActive = false;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBaseAssetAsync("BTC");

        // Assert
        var symbols = result.ToList();
        Assert.Single(symbols); // Only active BTC symbols
        Assert.Equal("BTCUSDT", symbols[0].Symbol);
        Assert.True(symbols[0].IsActive);
    }

    [Fact]
    public async Task GetByQuoteAssetAsync_InactiveSymbols_ReturnsOnlyActiveSymbols()
    {
        // Arrange
        var activeSymbolId = await SeedTestSymbolAsync("BTCUSDT", "BTC", "USDT");
        var inactiveSymbolId = await SeedTestSymbolAsync("ETHUSDT", "ETH", "USDT");

        // Make one symbol inactive
        var inactiveSymbol = await Context.Symbols.FindAsync(inactiveSymbolId);
        inactiveSymbol!.IsActive = false;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByQuoteAssetAsync("USDT");

        // Assert
        var symbols = result.ToList();
        Assert.Single(symbols); // Only active USDT symbols
        Assert.Equal("BTCUSDT", symbols[0].Symbol);
        Assert.True(symbols[0].IsActive);
    }
}