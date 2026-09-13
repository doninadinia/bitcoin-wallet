using System.Text.Json;
using BtcWallet.Wallet.Models;

namespace BtcWallet.Wallet.Storage;

public sealed class VaultRepository
{
    private readonly string _dir;
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public VaultRepository(string storageDir = ".wallets")
    {
        _dir = storageDir;
        Directory.CreateDirectory(_dir);
    }

    public string StorageDir => _dir;

    public async Task SaveAsync(VaultModel vault, CancellationToken ct = default)
    {
        var path = Path.Combine(_dir, $"{vault.Id}.vault");
        var json = JsonSerializer.Serialize(vault, JsonOpts);
        await File.WriteAllTextAsync(path, json, ct);
    }

    public async Task<VaultModel?> LoadAsync(string id, CancellationToken ct = default)
    {
        var path = Path.Combine(_dir, $"{id}.vault");
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<VaultModel>(json, JsonOpts);
    }

    public async Task<List<VaultModel>> ListAsync(CancellationToken ct = default)
    {
        var vaults = new List<VaultModel>();
        if (!Directory.Exists(_dir)) return vaults;

        foreach (var file in Directory.GetFiles(_dir, "*.vault"))
        {
            ct.ThrowIfCancellationRequested();
            var json = await File.ReadAllTextAsync(file, ct);
            var vault = JsonSerializer.Deserialize<VaultModel>(json, JsonOpts);
            if (vault is not null) vaults.Add(vault);
        }
        return vaults.OrderBy(v => v.CreatedUtc).ToList();
    }

    public bool Delete(string id)
    {
        var path = Path.Combine(_dir, $"{id}.vault");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    public bool Exists(string id)
        => File.Exists(Path.Combine(_dir, $"{id}.vault"));
}
