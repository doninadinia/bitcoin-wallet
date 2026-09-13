using BtcWallet.Wallet.Chain;
using BtcWallet.Wallet.Models;
using BtcWallet.Wallet.Storage;

namespace BtcWallet.Wallet.Services;

public sealed class SyncEngine
{
    private readonly BalanceProvider _balance;
    private readonly RpcClient _rpc;
    private readonly VaultRepository _repo;

    public SyncEngine(BalanceProvider balance, RpcClient rpc, VaultRepository repo)
    {
        _balance = balance;
        _rpc = rpc;
        _repo = repo;
    }

    public record SyncResult(
        int AccountsSynced, long TotalBalanceSatoshis, long BlockHeight);

    public async Task<SyncResult> SyncVaultAsync(
        VaultModel vault, CancellationToken ct = default)
    {
        var height = await _rpc.GetBlockHeightAsync(ct);
        long total = 0;
        int synced = 0;

        foreach (var account in vault.Accounts)
        {
            ct.ThrowIfCancellationRequested();
            var bal = await _balance.GetBalanceAsync(
                account.Address, forceRefresh: true, ct: ct);
            account.BalanceSatoshis = bal;
            account.LastSyncUtc = DateTime.UtcNow;
            total += bal;
            synced++;
        }

        await _repo.SaveAsync(vault, ct);
        return new SyncResult(synced, total, height);
    }

    public async Task<SyncResult> SyncAccountAsync(
        VaultModel vault, int accountIndex, CancellationToken ct = default)
    {
        if (accountIndex < 0 || accountIndex >= vault.Accounts.Count)
            throw new ArgumentOutOfRangeException(nameof(accountIndex));

        var height = await _rpc.GetBlockHeightAsync(ct);
        var account = vault.Accounts[accountIndex];
        var bal = await _balance.GetBalanceAsync(
            account.Address, forceRefresh: true, ct: ct);
        account.BalanceSatoshis = bal;
        account.LastSyncUtc = DateTime.UtcNow;

        await _repo.SaveAsync(vault, ct);
        long total = vault.Accounts.Sum(a => a.BalanceSatoshis);
        return new SyncResult(1, total, height);
    }
}
