using Xunit;
using BtcWallet.Wallet.Models;
using BtcWallet.Wallet.Services;
using BtcWallet.Wallet.Storage;

namespace BtcWallet.Wallet.Tests;

public class WalletManagerTests
{
    private static WalletManager CreateManager()
    {
        var dir = Path.Combine(
            Path.GetTempPath(), $"wallet-test-{Guid.NewGuid():N}");
        return new WalletManager(new VaultRepository(dir));
    }

    [Fact]
    public async Task CreateVault_ReturnsVaultWithOneAccount()
    {
        var mgr = CreateManager();
        var vault = await mgr.CreateVaultAsync("test", "password");

        Assert.NotNull(vault);
        Assert.Equal("test", vault.Name);
        Assert.Single(vault.Accounts);
        Assert.NotEmpty(vault.Accounts[0].Address);
    }

    [Fact]
    public async Task CreateVault_SetsActiveVault()
    {
        var mgr = CreateManager();
        await mgr.CreateVaultAsync("test", "password");
        Assert.NotNull(mgr.ActiveVault);
    }

    [Fact]
    public async Task OpenVault_WithCorrectPassphrase_Succeeds()
    {
        var mgr = CreateManager();
        var created = await mgr.CreateVaultAsync("test", "password");

        var opened = await mgr.OpenVaultAsync(created.Id, "password");
        Assert.NotNull(opened);
        Assert.Equal(created.Id, opened!.Id);
    }

    [Fact]
    public async Task OpenVault_NotFound_ReturnsNull()
    {
        var mgr = CreateManager();
        var result = await mgr.OpenVaultAsync("nonexistent", "pass");
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAccount_IncreasesCount()
    {
        var mgr = CreateManager();
        await mgr.CreateVaultAsync("test", "password");

        var account = await mgr.AddAccountAsync("password", "Savings");

        Assert.Equal(1, account.Index);
        Assert.Equal("Savings", account.Label);
        Assert.Equal(2, mgr.ActiveVault!.Accounts.Count);
    }

    [Fact]
    public async Task AddAccount_WithNoVault_Throws()
    {
        var mgr = CreateManager();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mgr.AddAccountAsync("password", "Fail"));
    }

    [Fact]
    public async Task ListVaults_AfterCreate_ContainsVault()
    {
        var mgr = CreateManager();
        await mgr.CreateVaultAsync("test", "password");

        var list = await mgr.ListVaultsAsync();
        Assert.Single(list);
        Assert.Equal("test", list[0].Name);
    }

    [Fact]
    public async Task MultipleAccounts_HaveUniqueAddresses()
    {
        var mgr = CreateManager();
        await mgr.CreateVaultAsync("test", "password");
        await mgr.AddAccountAsync("password", "A");
        await mgr.AddAccountAsync("password", "B");

        var addresses = mgr.ActiveVault!.Accounts
            .Select(a => a.Address).ToHashSet();
        Assert.Equal(3, addresses.Count);
    }
}
