using System.Text.Json;
using BtcWallet.Cli;
using BtcWallet.Wallet.Chain;
using BtcWallet.Wallet.Models;
using BtcWallet.Wallet.Services;
using BtcWallet.Wallet.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var config = new Dictionary<string, string>();
if (File.Exists(configPath))
{
    var json = await File.ReadAllTextAsync(configPath);
    config = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
}

var networkName = config.GetValueOrDefault("Network", "mainnet");
var network = NetworkConfig.ByName(networkName);
var rpc = new RpcClient(network);
var balanceProvider = new BalanceProvider(rpc);
var storageDir = config.GetValueOrDefault("StorageDir", ".wallets");
var repo = new VaultRepository(storageDir);
var manager = new WalletManager(repo);
var sync = new SyncEngine(balanceProvider, rpc, repo);
var portfolio = new PortfolioTracker();

var migrated = await Migrations.MigrateIfNeededAsync(storageDir);
if (migrated)
    Console.WriteLine("[info] Storage migrated to latest version.");

if (args.Length == 0)
{
    Commands.PrintUsage();
    return 1;
}

try
{
    return await Commands.RunAsync(args, manager, sync, portfolio);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    return 1;
}
