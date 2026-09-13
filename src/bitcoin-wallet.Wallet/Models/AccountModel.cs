using System.Text.Json.Serialization;

namespace BtcWallet.Wallet.Models;

public sealed class AccountModel
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("publicKey")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonPropertyName("network")]
    public string Network { get; set; } = "mainnet";

    [JsonPropertyName("lastSyncUtc")]
    public DateTime? LastSyncUtc { get; set; }

    [JsonPropertyName("balanceSatoshis")]
    public long BalanceSatoshis { get; set; }

    public decimal BalanceBtc => BalanceSatoshis / 100_000_000m;

    public bool IsStale(TimeSpan maxAge)
        => LastSyncUtc is null || DateTime.UtcNow - LastSyncUtc.Value > maxAge;

    public override string ToString()
    {
        var addr = Address.Length > 14
            ? $"{Address[..8]}...{Address[^6..]}"
            : Address;
        return $"[{Index}] {Label}: {addr} ({BalanceBtc:F8} BTC)";
    }
}
