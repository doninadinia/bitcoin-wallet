using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BtcWallet.Wallet.Chain;

public sealed class RpcClient
{
    private readonly Models.NetworkConfig _config;
    private long _blockHeight = 850_000;

    public RpcClient(Models.NetworkConfig config) => _config = config;

    public string NetworkName => _config.Name;

    public Task<long> GetBlockHeightAsync(CancellationToken ct = default)
    {
        _blockHeight += DeterministicInt(_blockHeight.ToString(), 0, 3);
        return Task.FromResult(_blockHeight);
    }

    public Task<string> GetBlockHashAsync(long height, CancellationToken ct = default)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"block:{_config.Name}:{height}"));
        return Task.FromResult(Convert.ToHexString(hash).ToLowerInvariant());
    }

    public Task<long> GetBalanceSatoshisAsync(string address, CancellationToken ct = default)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"balance:{address}:{_config.Name}"));
        long sat = Math.Abs(BitConverter.ToInt64(hash, 0)) % 10_000_000_000;
        return Task.FromResult(sat);
    }

    public Task<string> BroadcastTransactionAsync(string txHex, CancellationToken ct = default)
    {
        var txid = SHA256.HashData(
            Encoding.UTF8.GetBytes($"tx:{txHex}:{DateTime.UtcNow.Ticks}"));
        return Task.FromResult(Convert.ToHexString(txid).ToLowerInvariant());
    }

    public Task<JsonElement> GetTransactionAsync(string txId, CancellationToken ct = default)
    {
        var obj = new Dictionary<string, object>
        {
            ["txid"] = txId,
            ["confirmations"] = DeterministicInt(txId, 0, 100),
            ["blockHeight"] = _blockHeight - DeterministicInt(txId, 0, 10),
            ["fee"] = DeterministicInt(txId, 200, 5000),
        };
        var json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);
        return Task.FromResult(doc.RootElement.Clone());
    }

    private static int DeterministicInt(string seed, int min, int max)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return min + (int)((uint)BitConverter.ToInt32(hash, 0) % (uint)(max - min + 1));
    }
}
