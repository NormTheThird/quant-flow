//namespace QuantFlow.Test.Unit.Repositories;

///// <summary>
///// Unit tests for SymbolRepository using in-memory database
///// </summary>
//public class SymbolRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
//{
//    private readonly Mock<ILogger<SymbolRepository>> _mockLogger;
//    private readonly SymbolRepository _repository;

//    public SymbolRepositoryUnitTests()
//    {
//        _mockLogger = new Mock<ILogger<SymbolRepository>>();
//        _repository = new SymbolRepository(Context, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingSymbol_ReturnsSymbolModel()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateBTCUSDT();
//        var symbolEntity = symbolModel.ToEntity();

//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(symbolEntity.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(symbolEntity.Id, result.Id);
//        Assert.Equal("BTCUSDT", result.Symbol);
//        Assert.Equal("BTC", result.BaseAsset);
//        Assert.Equal("USDT", result.QuoteAsset);
//        Assert.True(result.IsActive);
//        Assert.Equal(0.001m, result.MinTradeAmount);
//        Assert.Equal(2, result.PricePrecision);
//        Assert.Equal(8, result.QuantityPrecision);
//    }

//    [Fact]
//    public async Task GetByIdAsync_NonExistentSymbol_ReturnsNull()
//    {
//        // Arrange
//        var symbolId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetByIdAsync(symbolId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolModel()
//    {
//        // Arrange
//        var symbolName = "ETHUSDT";
//        var symbolModel = SymbolModelFixture.CreateETHUSDT();
//        var symbolEntity = symbolModel.ToEntity();

//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetBySymbolAsync(symbolName);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(symbolName, result.Symbol);
//        Assert.Equal("ETH", result.BaseAsset);
//        Assert.Equal("USDT", result.QuoteAsset);
//        Assert.True(result.IsActive);
//        Assert.Equal(0.01m, result.MinTradeAmount);
//        Assert.Equal(2, result.PricePrecision);
//        Assert.Equal(6, result.QuantityPrecision);
//    }

//    [Fact]
//    public async Task GetBySymbolAsync_NonExistentSymbol_ReturnsNull()
//    {
//        // Arrange
//        var symbolName = "NONEXISTENT";

//        // Act
//        var result = await _repository.GetBySymbolAsync(symbolName);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetActiveAsync_WithActiveSymbols_ReturnsActiveSymbolsOnly()
//    {
//        // Arrange
//        var mixedSymbols = SymbolModelFixture.CreateMixedStatusSymbolBatch();
//        var symbolEntities = mixedSymbols.Select(s => s.ToEntity());

//        Context.Symbols.AddRange(symbolEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetActiveAsync();

//        // Assert
//        var symbolList = result.ToList();
//        Assert.Equal(3, symbolList.Count); // Should only return active symbols
//        Assert.All(symbolList, s => Assert.True(s.IsActive));
//        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "ADAUSDT");
//        Assert.DoesNotContain(symbolList, s => s.Symbol == "DOGEUSDT");
//        Assert.DoesNotContain(symbolList, s => s.Symbol == "SHIBUSDT");
//    }

//    [Fact]
//    public async Task GetByBaseAssetAsync_ExistingBaseAsset_ReturnsMatchingSymbols()
//    {
//        // Arrange
//        var baseAsset = "BTC";
//        var btcSymbols = SymbolModelFixture.CreateSymbolBatchByBaseAsset(baseAsset, new[] { "USDT", "EUR", "BNB" });
//        var otherSymbol = SymbolModelFixture.CreateETHUSDT(); // Different base asset

//        var allSymbols = btcSymbols.Concat(new[] { otherSymbol });
//        var symbolEntities = allSymbols.Select(s => s.ToEntity());

//        Context.Symbols.AddRange(symbolEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByBaseAssetAsync(baseAsset);

//        // Assert
//        var symbolList = result.ToList();
//        Assert.Equal(3, symbolList.Count);
//        Assert.All(symbolList, s => Assert.Equal("BTC", s.BaseAsset));
//        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "BTCEUR");
//        Assert.Contains(symbolList, s => s.Symbol == "BTCBNB");
//        Assert.DoesNotContain(symbolList, s => s.Symbol == "ETHUSDT");
//    }

//    [Fact]
//    public async Task GetByQuoteAssetAsync_ExistingQuoteAsset_ReturnsMatchingSymbols()
//    {
//        // Arrange
//        var quoteAsset = "USDT";
//        var usdtSymbols = SymbolModelFixture.CreateSymbolBatchByQuoteAsset(quoteAsset, new[] { "BTC", "ETH", "ADA" });
//        var otherSymbol = SymbolModelFixture.CreateCustomSymbol("BTC", "EUR"); // Different quote asset

//        var allSymbols = usdtSymbols.Concat(new[] { otherSymbol });
//        var symbolEntities = allSymbols.Select(s => s.ToEntity());

//        Context.Symbols.AddRange(symbolEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByQuoteAssetAsync(quoteAsset);

//        // Assert
//        var symbolList = result.ToList();
//        Assert.Equal(3, symbolList.Count);
//        Assert.All(symbolList, s => Assert.Equal("USDT", s.QuoteAsset));
//        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "ADAUSDT");
//        Assert.DoesNotContain(symbolList, s => s.Symbol == "BTCEUR");
//    }

//    [Fact]
//    public async Task CreateAsync_ValidSymbol_CallsAddAndSaveChanges()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateADAUSDT();

//        // Act
//        var result = await _repository.CreateAsync(symbolModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(symbolModel.Symbol, result.Symbol);
//        Assert.Equal(symbolModel.BaseAsset, result.BaseAsset);
//        Assert.Equal(symbolModel.QuoteAsset, result.QuoteAsset);
//        Assert.Equal(symbolModel.IsActive, result.IsActive);
//        Assert.Equal(symbolModel.MinTradeAmount, result.MinTradeAmount);
//        Assert.Equal(symbolModel.PricePrecision, result.PricePrecision);
//        Assert.Equal(symbolModel.QuantityPrecision, result.QuantityPrecision);

//        // Verify it was actually saved to the database
//        var savedEntity = await Context.Symbols.FindAsync(result.Id);
//        Assert.NotNull(savedEntity);
//        Assert.Equal(result.Symbol, savedEntity.Symbol);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingSymbol_UpdatesAndSaves()
//    {
//        // Arrange
//        var originalSymbol = SymbolModelFixture.CreateBTCUSDT();
//        var symbolEntity = originalSymbol.ToEntity();

//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Get the saved symbol and create update model
//        var savedSymbol = await _repository.GetByIdAsync(symbolEntity.Id);
//        var updatedModel = SymbolModelFixture.CreateSymbolForUpdate(
//            savedSymbol.Id,
//            "BTCUSDT",
//            isActive: false,
//            minTradeAmount: 0.01m,
//            pricePrecision: 4,
//            quantityPrecision: 6
//        );

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("BTCUSDT", result.Symbol);
//        Assert.Equal("BTC", result.BaseAsset);
//        Assert.Equal("USDT", result.QuoteAsset);
//        Assert.False(result.IsActive);
//        Assert.Equal(0.01m, result.MinTradeAmount);
//        Assert.Equal(4, result.PricePrecision);
//        Assert.Equal(6, result.QuantityPrecision);

//        // Verify the changes were persisted
//        var updatedEntity = await Context.Symbols.FindAsync(result.Id);
//        Assert.NotNull(updatedEntity);
//        Assert.False(updatedEntity.IsActive);
//        Assert.Equal(0.01m, updatedEntity.MinTradeAmount);
//    }

//    [Fact]
//    public async Task UpdateAsync_NonExistentSymbol_ThrowsNotFoundException()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateDefault("NONEXISTENT", "NON", "EXISTENT");

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(symbolModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingSymbol_SoftDeletesSymbol()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateBTCUSDT();
//        var symbolEntity = symbolModel.ToEntity();

//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(symbolEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete was applied
//        var deletedEntity = await Context.Symbols.FindAsync(symbolEntity.Id);
//        Assert.NotNull(deletedEntity);
//        Assert.True(deletedEntity.IsDeleted);
//        Assert.NotNull(deletedEntity.UpdatedAt);
//    }

//    [Fact]
//    public async Task DeleteAsync_NonExistentSymbol_ReturnsFalse()
//    {
//        // Arrange
//        var symbolId = Guid.NewGuid();

//        // Act
//        var result = await _repository.DeleteAsync(symbolId);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public async Task GetAllAsync_WithSymbols_ReturnsAllActiveSymbols()
//    {
//        // Arrange
//        var symbols = new List<SymbolModel>
//        {
//            SymbolModelFixture.CreateBTCUSDT(),
//            SymbolModelFixture.CreateETHUSDT()
//        };
//        var symbolEntities = symbols.Select(s => s.ToEntity());

//        Context.Symbols.AddRange(symbolEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var symbolList = result.ToList();
//        Assert.Equal(2, symbolList.Count);
//        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
//    }

//    [Fact]
//    public async Task GetAllAsync_NoSymbols_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }
//}