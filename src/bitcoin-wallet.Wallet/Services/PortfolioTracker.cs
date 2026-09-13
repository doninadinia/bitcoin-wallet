using System.Security.Cryptography;
using System.Text;
using BtcWallet.Wallet.Models;

namespace BtcWallet.Wallet.Services;

public sealed class PortfolioTracker
{
    private readonly List<PortfolioSnapshot> _history = [];

    public record PortfolioSnapshot(
        DateTime Timestamp, long TotalSatoshis, decimal BtcPrice, decimal UsdValue);

    public IReadOnlyList<PortfolioSnapshot> History => _history;

    public PortfolioSnapshot TakeSnapshot(VaultModel vault)
    {
        long total = vault.Accounts.Sum(a => a.BalanceSatoshis);
        decimal btcPrice = SimulatePrice(DateTime.UtcNow);
        decimal btc = total / 100_000_000m;
        var snap = new PortfolioSnapshot(
            DateTime.UtcNow, total, btcPrice, btc * btcPrice);
        _history.Add(snap);
        return snap;
    }

    public decimal CalculateChangePercent()
    {
        if (_history.Count < 2) return 0m;
        var first = _history[0].UsdValue;
        var last = _history[^1].UsdValue;
        return first == 0 ? 0m : (last - first) / first * 100m;
    }

    public (decimal High, decimal Low, decimal Average) GetPriceStats()
    {
        if (_history.Count == 0) return (0, 0, 0);
        var prices = _history.Select(s => s.BtcPrice).ToList();
        return (prices.Max(), prices.Min(), prices.Average());
    }

    public string FormatSummary(VaultModel vault)
    {
        var snap = TakeSnapshot(vault);
        var change = CalculateChangePercent();
        var sb = new StringBuilder();
        sb.AppendLine(
            $"Portfolio Summary — {snap.Timestamp:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine(new string('─', 50));
        sb.AppendLine($"  Accounts:    {vault.Accounts.Count}");
        sb.AppendLine($"  Balance:     {snap.TotalSatoshis / 100_000_000m:F8} BTC");
        sb.AppendLine($"  BTC Price:   ${snap.BtcPrice:N2}");
        sb.AppendLine($"  USD Value:   ${snap.UsdValue:N2}");
        if (_history.Count >= 2)
            sb.AppendLine($"  Change:      {change:+0.00;-0.00}%");
        return sb.ToString();
    }

    private static decimal SimulatePrice(DateTime dt)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"price:{dt:yyyy-MM-dd-HH}"));
        var raw = Math.Abs(BitConverter.ToInt32(hash, 0));
        return 40_000m + raw % 30_000;
    }
}
