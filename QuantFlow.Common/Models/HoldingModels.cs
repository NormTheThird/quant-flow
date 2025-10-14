namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a single asset holding
/// </summary>
public class HoldingModel
{
    public string Asset { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UsdValue { get; set; }
    public decimal UsdPrice { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal Change24h { get; set; }
}

/// <summary>
/// Summary of all holdings
/// </summary>
public class HoldingsSummary
{
    public List<HoldingModel> Holdings { get; set; } = [];
    public decimal TotalUsdValue { get; set; }
    public bool HasCredentials { get; set; }
    public string? ErrorMessage { get; set; }
}