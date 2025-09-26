//namespace QuantFlow.Test.Integration.Repositories;

///// <summary>
///// Integration tests for SymbolRepository with in-memory database
///// </summary>
//public class SymbolRepositoryIntegrationTests : BaseRepositoryIntegrationTest
//{
//    private readonly SymbolRepository _repository;

//    public SymbolRepositoryIntegrationTests()
//    {
//        var logger = Substitute.For<ILogger<SymbolRepository>>();
//        _repository = new SymbolRepository(Context, logger);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingSymbol_ReturnsSymbolModel()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
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
//        Assert.Equal(symbolModel.MinTradeAmount, result.MinTradeAmount);
//        Assert.Equal(symbolModel.PricePrecision, result.PricePrecision);
//        Assert.Equal(symbolModel.QuantityPrecision, result.QuantityPrecision);
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
//    public async Task GetByIdAsync_DeletedSymbol_ReturnsNull()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateDefault();
//        var symbolEntity = symbolModel.ToEntity();
//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Soft delete the symbol
//        var symbol = await Context.Symbols.FindAsync(symbolEntity.Id);
//        symbol!.IsDeleted = true;
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(symbolEntity.Id);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolModel()
//    {
//        // Arrange
//        var symbolName = "ETHUSDT";
//        var symbolModel = SymbolModelFixture.CreateDefault(symbolName, "ETH", "USDT");
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
//    public async Task GetActiveAsync_WithActiveAndInactiveSymbols_ReturnsActiveSymbolsOnly()
//    {
//        // Arrange
//        var activeSymbol1 = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var activeSymbol1Entity = activeSymbol1.ToEntity();

//        var activeSymbol2 = SymbolModelFixture.CreateDefault("ETHUSDT", "ETH", "USDT");
//        var activeSymbol2Entity = activeSymbol2.ToEntity();

//        var inactiveSymbol = SymbolModelFixture.CreateInactiveSymbol("ADAUSDT");
//        var inactiveSymbolEntity = inactiveSymbol.ToEntity();

//        Context.Symbols.AddRange(activeSymbol1Entity, activeSymbol2Entity, inactiveSymbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetActiveAsync();

//        // Assert
//        var symbols = result.ToList();
//        Assert.Equal(2, symbols.Count);
//        Assert.All(symbols, s => Assert.True(s.IsActive));
//        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
//        Assert.DoesNotContain(symbols, s => s.Symbol == "ADAUSDT");
//    }

//    [Fact]
//    public async Task GetByBaseAssetAsync_ExistingBaseAsset_ReturnsMatchingSymbols()
//    {
//        // Arrange
//        var btcUsdt = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var btcEur = SymbolModelFixture.CreateDefault("BTCEUR", "BTC", "EUR");
//        var ethUsdt = SymbolModelFixture.CreateDefault("ETHUSDT", "ETH", "USDT");

//        var btcUsdtEntity = btcUsdt.ToEntity();
//        var btcEurEntity = btcEur.ToEntity();
//        var ethUsdtEntity = ethUsdt.ToEntity();

//        Context.Symbols.AddRange(btcUsdtEntity, btcEurEntity, ethUsdtEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByBaseAssetAsync("BTC");

//        // Assert
//        var symbols = result.ToList();
//        Assert.Equal(2, symbols.Count);
//        Assert.All(symbols, s => Assert.Equal("BTC", s.BaseAsset));
//        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbols, s => s.Symbol == "BTCEUR");
//        Assert.DoesNotContain(symbols, s => s.Symbol == "ETHUSDT");
//    }

//    [Fact]
//    public async Task GetByQuoteAssetAsync_ExistingQuoteAsset_ReturnsMatchingSymbols()
//    {
//        // Arrange
//        var btcUsdt = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var ethUsdt = SymbolModelFixture.CreateDefault("ETHUSDT", "ETH", "USDT");
//        var btcEur = SymbolModelFixture.CreateDefault("BTCEUR", "BTC", "EUR");

//        var btcUsdtEntity = btcUsdt.ToEntity();
//        var ethUsdtEntity = ethUsdt.ToEntity();
//        var btcEurEntity = btcEur.ToEntity();

//        Context.Symbols.AddRange(btcUsdtEntity, ethUsdtEntity, btcEurEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByQuoteAssetAsync("USDT");

//        // Assert
//        var symbols = result.ToList();
//        Assert.Equal(2, symbols.Count);
//        Assert.All(symbols, s => Assert.Equal("USDT", s.QuoteAsset));
//        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
//        Assert.DoesNotContain(symbols, s => s.Symbol == "BTCEUR");
//    }

//    [Fact]
//    public async Task CreateAsync_ValidSymbol_ReturnsCreatedSymbol()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateDefault("ADAUSDT", "ADA", "USDT");
//        symbolModel.MinTradeAmount = 10.0m;
//        symbolModel.PricePrecision = 4;
//        symbolModel.QuantityPrecision = 2;

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
//        Assert.True(result.CreatedAt > DateTime.MinValue);

//        // Verify in database
//        var dbSymbol = await Context.Symbols.FindAsync(result.Id);
//        Assert.NotNull(dbSymbol);
//        Assert.Equal(symbolModel.Symbol, dbSymbol.Symbol);
//        Assert.Equal(symbolModel.BaseAsset, dbSymbol.BaseAsset);
//        Assert.Equal(symbolModel.QuoteAsset, dbSymbol.QuoteAsset);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingSymbol_ReturnsUpdatedSymbol()
//    {
//        // Arrange
//        var originalSymbol = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var symbolEntity = originalSymbol.ToEntity();
//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        var updatedModel = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        updatedModel.Id = symbolEntity.Id;
//        updatedModel.IsActive = false;
//        updatedModel.MinTradeAmount = 0.01m;
//        updatedModel.PricePrecision = 4;
//        updatedModel.QuantityPrecision = 6;
//        updatedModel.UpdatedAt = DateTime.UtcNow;
//        updatedModel.UpdatedBy = "NormTheThird";

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
//        Assert.NotNull(result.UpdatedAt);

//        // Verify in database
//        var dbSymbol = await Context.Symbols.FindAsync(symbolEntity.Id);
//        Assert.NotNull(dbSymbol);
//        Assert.False(dbSymbol.IsActive);
//        Assert.Equal(0.01m, dbSymbol.MinTradeAmount);
//        Assert.Equal(4, dbSymbol.PricePrecision);
//        Assert.Equal(6, dbSymbol.QuantityPrecision);
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
//    public async Task DeleteAsync_ExistingSymbol_ReturnsTrueAndSoftDeletes()
//    {
//        // Arrange
//        var symbolModel = SymbolModelFixture.CreateDefault();
//        var symbolEntity = symbolModel.ToEntity();
//        Context.Symbols.Add(symbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(symbolEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete
//        var dbSymbol = await Context.Symbols.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == symbolEntity.Id);
//        Assert.NotNull(dbSymbol);
//        Assert.True(dbSymbol.IsDeleted);
//        Assert.NotNull(dbSymbol.UpdatedAt);

//        // Verify symbol is not returned by normal queries
//        var symbolModelResult = await _repository.GetByIdAsync(symbolEntity.Id);
//        Assert.Null(symbolModelResult);
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
//        var symbol1 = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var symbol1Entity = symbol1.ToEntity();

//        var symbol2 = SymbolModelFixture.CreateDefault("ETHUSDT", "ETH", "USDT");
//        var symbol2Entity = symbol2.ToEntity();

//        var deletedSymbol = SymbolModelFixture.CreateDefault("ADAUSDT", "ADA", "USDT");
//        var deletedSymbolEntity = deletedSymbol.ToEntity();

//        Context.Symbols.AddRange(symbol1Entity, symbol2Entity, deletedSymbolEntity);
//        await Context.SaveChangesAsync();

//        // Soft delete one symbol
//        var symbolToDelete = await Context.Symbols.FindAsync(deletedSymbolEntity.Id);
//        symbolToDelete!.IsDeleted = true;
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var symbols = result.ToList();
//        Assert.Equal(2, symbols.Count); // Only active symbols
//        Assert.Contains(symbols, s => s.Symbol == "BTCUSDT");
//        Assert.Contains(symbols, s => s.Symbol == "ETHUSDT");
//        Assert.DoesNotContain(symbols, s => s.Symbol == "ADAUSDT");
//    }

//    [Fact]
//    public async Task GetAllAsync_NoSymbols_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }

//    // TODO: Trey: The test is failing because EF Core's In-Memory database provider doesn't enforce foreign key
//    // constraints. This is the same issue I mentioned earlier - the In-Memory provider is designed for simple
//    // testing and doesn't validate referential integrity.
//    //[Fact]
//    //public async Task CreateAsync_DuplicateSymbol_ThrowsException()
//    //{
//    //    // Arrange
//    //    var originalSymbol = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//    //    var originalEntity = originalSymbol.ToEntity();
//    //    Context.Symbols.Add(originalEntity);
//    //    await Context.SaveChangesAsync();

//    //    var duplicateSymbol = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");

//    //    // Act & Assert
//    //    await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicateSymbol));
//    //}

//    [Fact]
//    public async Task GetByBaseAssetAsync_InactiveSymbols_ReturnsOnlyActiveSymbols()
//    {
//        // Arrange
//        var activeSymbol = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var activeSymbolEntity = activeSymbol.ToEntity();

//        var inactiveSymbol = SymbolModelFixture.CreateInactiveSymbol("BTCEUR");
//        var inactiveSymbolEntity = inactiveSymbol.ToEntity();

//        Context.Symbols.AddRange(activeSymbolEntity, inactiveSymbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByBaseAssetAsync("BTC");

//        // Assert
//        var symbols = result.ToList();
//        Assert.Single(symbols); // Only active BTC symbols
//        Assert.Equal("BTCUSDT", symbols[0].Symbol);
//        Assert.True(symbols[0].IsActive);
//    }

//    [Fact]
//    public async Task GetByQuoteAssetAsync_InactiveSymbols_ReturnsOnlyActiveSymbols()
//    {
//        // Arrange
//        var activeSymbol = SymbolModelFixture.CreateDefault("BTCUSDT", "BTC", "USDT");
//        var activeSymbolEntity = activeSymbol.ToEntity();

//        var inactiveSymbol = SymbolModelFixture.CreateInactiveSymbol("ETHUSDT");
//        var inactiveSymbolEntity = inactiveSymbol.ToEntity();

//        Context.Symbols.AddRange(activeSymbolEntity, inactiveSymbolEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByQuoteAssetAsync("USDT");

//        // Assert
//        var symbols = result.ToList();
//        Assert.Single(symbols); // Only active USDT symbols
//        Assert.Equal("BTCUSDT", symbols[0].Symbol);
//        Assert.True(symbols[0].IsActive);
//    }
//}