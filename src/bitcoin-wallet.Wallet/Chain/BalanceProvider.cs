namespace BtcWallet.Wallet.Chain;

public sealed class BalanceProvider
{
    private readonly RpcClient _rpc;
    private readonly Dictionary<string, long> _cache = new();
    private readonly Dictionary<string, DateTime> _cacheTs = new();
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public BalanceProvider(RpcClient rpc) => _rpc = rpc;

    public async Task<long> GetBalanceAsync(
        string address, bool forceRefresh = false, CancellationToken ct = default)
    {
        if (!forceRefresh
            && _cache.TryGetValue(address, out var cached)
            && _cacheTs.TryGetValue(address, out var ts)
            && DateTime.UtcNow - ts < _ttl)
        {
            return cached;
        }
        var balance = await _rpc.GetBalanceSatoshisAsync(address, ct);
        _cache[address] = balance;
        _cacheTs[address] = DateTime.UtcNow;
        return balance;
    }

    public async Task<Dictionary<string, long>> GetBalancesAsync(
        IEnumerable<string> addresses, CancellationToken ct = default)
    {
        var result = new Dictionary<string, long>();
        foreach (var addr in addresses)
        {
            ct.ThrowIfCancellationRequested();
            result[addr] = await GetBalanceAsync(addr, ct: ct);
        }
        return result;
    }

    public long GetCachedBalance(string address)
        => _cache.GetValueOrDefault(address, 0);

    public void InvalidateCache(string? address = null)
    {
        if (address is null)
        {
            _cache.Clear();
            _cacheTs.Clear();
        }
        else
        {
            _cache.Remove(address);
            _cacheTs.Remove(address);
        }
    }

    public static decimal SatoshisToBtc(long satoshis)
        => satoshis / 100_000_000m;

    public static long BtcToSatoshis(decimal btc)
        => (long)(btc * 100_000_000m);
}
