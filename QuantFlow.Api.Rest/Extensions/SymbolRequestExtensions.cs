namespace QuantFlow.Api.Rest.Extensions;

public static class SymbolRequestExtensions
{
    public static SymbolModel ToModel(this CreateSymbolRequest request)
    {
        return new SymbolModel
        {
            Symbol = request.Symbol,
            BaseAsset = request.BaseAsset,
            QuoteAsset = request.QuoteAsset,
            IsActive = request.IsActive,
            MinTradeAmount = request.MinTradeAmount,
            PricePrecision = request.PricePrecision,
            QuantityPrecision = request.QuantityPrecision
        };
    }

    public static SymbolModel ToModelWithId(this UpdateSymbolRequest request, Guid id)
    {
        return new SymbolModel
        {
            Id = id,
            Symbol = request.Symbol,
            BaseAsset = request.BaseAsset,
            QuoteAsset = request.QuoteAsset,
            IsActive = request.IsActive,
            MinTradeAmount = request.MinTradeAmount,
            PricePrecision = request.PricePrecision,
            QuantityPrecision = request.QuantityPrecision
        };
    }

    public static void UpdateModel(this UpdateSymbolRequest request, SymbolModel model)
    {
        model.Symbol = request.Symbol;
        model.BaseAsset = request.BaseAsset;
        model.QuoteAsset = request.QuoteAsset;
        model.IsActive = request.IsActive;
        model.MinTradeAmount = request.MinTradeAmount;
        model.PricePrecision = request.PricePrecision;
        model.QuantityPrecision = request.QuantityPrecision;
    }
}