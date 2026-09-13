using BtcWallet.Wallet.Crypto;
using BtcWallet.Wallet.Models;
using BtcWallet.Wallet.Storage;

namespace BtcWallet.Wallet.Services;

public sealed class WalletManager
{
    private readonly VaultRepository _repo;
    private VaultModel? _activeVault;

    public WalletManager(VaultRepository repo) => _repo = repo;

    public VaultModel? ActiveVault => _activeVault;

    public async Task<VaultModel> CreateVaultAsync(
        string name, string passphrase, CancellationToken ct = default)
    {
        var mnemonic = Mnemonic.Generate(12);
        var seed = KeyDerivation.SeedFromMnemonic(mnemonic, passphrase);

        var vault = new VaultModel
        {
            Name = name,
            EncryptedSeed = Convert.ToHexString(seed[..32]).ToLowerInvariant(),
        };
        vault.Lock(passphrase);

        var account = DeriveAccount(seed, 0, "Default");
        vault.Accounts.Add(account);

        await _repo.SaveAsync(vault, ct);
        _activeVault = vault;
        return vault;
    }

    public async Task<VaultModel?> OpenVaultAsync(
        string id, string passphrase, CancellationToken ct = default)
    {
        var vault = await _repo.LoadAsync(id, ct);
        if (vault is null) return null;
        _ = vault.Unlock(passphrase);
        _activeVault = vault;
        return vault;
    }

    public AccountModel DeriveAccount(byte[] seed, int index, string label)
    {
        var (_, pubKey) = KeyDerivation.DeriveKeyPair(seed, index);
        var address = AddressCodec.PublicKeyToAddress(pubKey);

        return new AccountModel
        {
            Index = index,
            Label = label,
            Address = address,
            PublicKey = Convert.ToHexString(pubKey).ToLowerInvariant(),
        };
    }

    public async Task<AccountModel> AddAccountAsync(
        string passphrase, string label, CancellationToken ct = default)
    {
        if (_activeVault is null)
            throw new InvalidOperationException("No vault is open.");

        var seed = _activeVault.Unlock(passphrase);
        int nextIndex = _activeVault.Accounts.Count;
        var account = DeriveAccount(seed, nextIndex, label);
        _activeVault.Accounts.Add(account);
        await _repo.SaveAsync(_activeVault, ct);
        return account;
    }

    public async Task<List<VaultModel>> ListVaultsAsync(CancellationToken ct = default)
        => await _repo.ListAsync(ct);
}
