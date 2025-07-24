namespace QuantFlow.UnitTests.Repositories;

/// <summary>
/// Unit tests for SymbolRepository using mocked dependencies
/// </summary>
public class SymbolRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<SymbolRepository>> _mockLogger;
    private readonly SymbolRepository _repository;

    public SymbolRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<SymbolRepository>>();
        _repository = new SymbolRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSymbol_ReturnsSymbolModel()
    {
        // Arrange
        var symbolId = Guid.NewGuid();
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = symbolId,
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                IsActive = true,
                MinTradeAmount = 0.001m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(symbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

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
        var symbols = new List<SymbolEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(symbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

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
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = symbolName,
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                IsActive = true,
                MinTradeAmount = 0.01m,
                PricePrecision = 2,
                QuantityPrecision = 6,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(symbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetBySymbolAsync(symbolName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbolName, result.Symbol);
        Assert.Equal("ETH", result.BaseAsset);
        Assert.Equal("USDT", result.QuoteAsset);
        Assert.True(result.IsActive);
        Assert.Equal(0.01m, result.MinTradeAmount);
        Assert.Equal(2, result.PricePrecision);
        Assert.Equal(6, result.QuantityPrecision);
    }

    [Fact]
    public async Task GetBySymbolAsync_NonExistentSymbol_ReturnsNull()
    {
        // Arrange
        var symbolName = "NONEXISTENT";
        var symbols = new List<SymbolEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(symbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetBySymbolAsync(symbolName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_WithActiveSymbols_ReturnsActiveSymbolsOnly()
    {
        // Arrange
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "ADAUSDT",
                BaseAsset = "ADA",
                QuoteAsset = "USDT",
                IsActive = false, // Inactive
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var activeSymbols = symbols
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol);
        var mockDbSet = CreateMockDbSetWithAsync(activeSymbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        var symbolList = result.ToList();
        Assert.Equal(2, symbolList.Count);
        Assert.All(symbolList, s => Assert.True(s.IsActive));
        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
        Assert.DoesNotContain(symbolList, s => s.Symbol == "ADAUSDT");
    }

    [Fact]
    public async Task GetByBaseAssetAsync_ExistingBaseAsset_ReturnsMatchingSymbols()
    {
        // Arrange
        var baseAsset = "BTC";
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCEUR",
                BaseAsset = "BTC",
                QuoteAsset = "EUR",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var btcSymbols = symbols
            .Where(s => s.BaseAsset == baseAsset && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol);
        var mockDbSet = CreateMockDbSetWithAsync(btcSymbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByBaseAssetAsync(baseAsset);

        // Assert
        var symbolList = result.ToList();
        Assert.Equal(2, symbolList.Count);
        Assert.All(symbolList, s => Assert.Equal("BTC", s.BaseAsset));
        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbolList, s => s.Symbol == "BTCEUR");
        Assert.DoesNotContain(symbolList, s => s.Symbol == "ETHUSDT");
    }

    [Fact]
    public async Task GetByQuoteAssetAsync_ExistingQuoteAsset_ReturnsMatchingSymbols()
    {
        // Arrange
        var quoteAsset = "USDT";
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCEUR",
                BaseAsset = "BTC",
                QuoteAsset = "EUR",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var usdtSymbols = symbols
            .Where(s => s.QuoteAsset == quoteAsset && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol);
        var mockDbSet = CreateMockDbSetWithAsync(usdtSymbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByQuoteAssetAsync(quoteAsset);

        // Assert
        var symbolList = result.ToList();
        Assert.Equal(2, symbolList.Count);
        Assert.All(symbolList, s => Assert.Equal("USDT", s.QuoteAsset));
        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
        Assert.DoesNotContain(symbolList, s => s.Symbol == "BTCEUR");
    }

    [Fact]
    public async Task CreateAsync_ValidSymbol_CallsAddAndSaveChanges()
    {
        // Arrange
        var symbolModel = new SymbolModel
        {
            Id = Guid.NewGuid(),
            Symbol = "ADAUSDT",
            BaseAsset = "ADA",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 1.0m,
            PricePrecision = 4,
            QuantityPrecision = 2
        };

        var mockDbSet = new Mock<DbSet<SymbolEntity>>();
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

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

        mockDbSet.Verify(m => m.Add(It.IsAny<SymbolEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingSymbol_UpdatesAndSaves()
    {
        // Arrange
        var symbolId = Guid.NewGuid();
        var existingSymbol = new SymbolEntity
        {
            Id = symbolId,
            Symbol = "BTCUSDT",
            BaseAsset = "BTC",
            QuoteAsset = "USDT",
            IsActive = true,
            MinTradeAmount = 0.001m,
            PricePrecision = 2,
            QuantityPrecision = 8,
            CreatedAt = DateTime.UtcNow
        };

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

        var mockDbSet = new Mock<DbSet<SymbolEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingSymbol }, s => s.Id);

        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

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

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
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

        var mockDbSet = new Mock<DbSet<SymbolEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<SymbolEntity>(), s => s.Id);

        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(symbolModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingSymbol_SoftDeletesSymbol()
    {
        // Arrange
        var symbolId = Guid.NewGuid();
        var existingSymbol = new SymbolEntity
        {
            Id = symbolId,
            Symbol = "BTCUSDT",
            BaseAsset = "BTC",
            QuoteAsset = "USDT",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<SymbolEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingSymbol }, s => s.Id);

        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(symbolId);

        // Assert
        Assert.True(result);
        Assert.True(existingSymbol.IsDeleted);
        Assert.NotNull(existingSymbol.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentSymbol_ReturnsFalse()
    {
        // Arrange
        var symbolId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<SymbolEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<SymbolEntity>(), s => s.Id);

        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.DeleteAsync(symbolId);

        // Assert
        Assert.False(result);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WithSymbols_ReturnsAllActiveSymbols()
    {
        // Arrange
        var symbols = new List<SymbolEntity>
        {
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new SymbolEntity
            {
                Id = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(symbols.OrderBy(s => s.Symbol));
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var symbolList = result.ToList();
        Assert.Equal(2, symbolList.Count);
        Assert.Contains(symbolList, s => s.Symbol == "BTCUSDT");
        Assert.Contains(symbolList, s => s.Symbol == "ETHUSDT");
    }

    [Fact]
    public async Task GetAllAsync_NoSymbols_ReturnsEmptyCollection()
    {
        // Arrange
        var symbols = new List<SymbolEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(symbols);
        MockContext.Setup(c => c.Symbols).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }
}