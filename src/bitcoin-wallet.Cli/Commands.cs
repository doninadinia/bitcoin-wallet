using BtcWallet.Wallet.Services;

namespace BtcWallet.Cli;

public static class Commands
{
    public static async Task<int> RunAsync(
        string[] args, WalletManager mgr, SyncEngine sync, PortfolioTracker port)
    {
        var cmd = args[0].ToLowerInvariant();
        var opts = ParseOptions(args[1..]);

        switch (cmd)
        {
            case "create-vault":
                return await CreateVaultAsync(mgr, opts);
            case "list-vaults":
            case "ls":
                return await ListVaultsAsync(mgr);
            case "open-vault":
                return await OpenVaultAsync(mgr, opts);
            case "add-account":
                return await AddAccountAsync(mgr, opts);
            case "sync":
                return await SyncAsync(mgr, sync);
            case "balance":
            case "bal":
                return ShowBalance(mgr);
            case "portfolio":
                return ShowPortfolio(mgr, port);
            case "help":
            case "--help":
            case "-h":
                PrintUsage();
                return 0;
            default:
                Console.Error.WriteLine($"Unknown command: {cmd}");
                PrintUsage();
                return 1;
        }
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var opts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--") && i + 1 < args.Length)
                opts[args[i][2..]] = args[++i];
        }
        return opts;
    }

    private static async Task<int> CreateVaultAsync(
        WalletManager mgr, Dictionary<string, string> opts)
    {
        var name = opts.GetValueOrDefault("name", "default");
        Console.Write("Passphrase: ");
        var pass = Console.ReadLine() ?? "";

        var vault = await mgr.CreateVaultAsync(name, pass);
        Console.WriteLine($"Vault created: {vault.Id}");
        Console.WriteLine($"  Name:     {vault.Name}");
        Console.WriteLine($"  Accounts: {vault.Accounts.Count}");
        Console.WriteLine($"  Address:  {vault.Accounts[0].Address}");
        return 0;
    }

    private static async Task<int> ListVaultsAsync(WalletManager mgr)
    {
        var vaults = await mgr.ListVaultsAsync();
        if (vaults.Count == 0)
        {
            Console.WriteLine("No vaults found. Use 'create-vault' to create one.");
            return 0;
        }
        Console.WriteLine(
            $"{"ID",-34} {"Name",-20} {"Accounts",8} {"Created",20}");
        Console.WriteLine(new string('─', 84));
        foreach (var v in vaults)
            Console.WriteLine(
                $"{v.Id,-34} {v.Name,-20} {v.Accounts.Count,8} " +
                $"{v.CreatedUtc:yyyy-MM-dd HH:mm,20}");
        return 0;
    }

    private static async Task<int> OpenVaultAsync(
        WalletManager mgr, Dictionary<string, string> opts)
    {
        if (!opts.TryGetValue("id", out var id))
        {
            Console.Error.WriteLine("Missing --id option.");
            return 1;
        }
        Console.Write("Passphrase: ");
        var pass = Console.ReadLine() ?? "";

        var vault = await mgr.OpenVaultAsync(id, pass);
        if (vault is null)
        {
            Console.Error.WriteLine("Vault not found or passphrase incorrect.");
            return 1;
        }
        Console.WriteLine(
            $"Opened vault: {vault.Name} ({vault.Accounts.Count} accounts)");
        return 0;
    }

    private static async Task<int> AddAccountAsync(
        WalletManager mgr, Dictionary<string, string> opts)
    {
        var label = opts.GetValueOrDefault(
            "label", $"Account-{DateTime.UtcNow.Ticks % 1000}");
        Console.Write("Passphrase: ");
        var pass = Console.ReadLine() ?? "";

        var account = await mgr.AddAccountAsync(pass, label);
        Console.WriteLine($"Account added: [{account.Index}] {account.Label}");
        Console.WriteLine($"  Address: {account.Address}");
        return 0;
    }

    private static async Task<int> SyncAsync(WalletManager mgr, SyncEngine sync)
    {
        if (mgr.ActiveVault is null)
        {
            Console.Error.WriteLine("No vault is open. Use 'open-vault' first.");
            return 1;
        }
        Console.Write("Syncing...");
        var result = await sync.SyncVaultAsync(mgr.ActiveVault);
        Console.WriteLine(" done.");
        Console.WriteLine($"  Accounts synced: {result.AccountsSynced}");
        Console.WriteLine(
            $"  Total balance:   " +
            $"{result.TotalBalanceSatoshis / 100_000_000m:F8} BTC");
        Console.WriteLine($"  Block height:    {result.BlockHeight}");
        return 0;
    }

    private static int ShowBalance(WalletManager mgr)
    {
        if (mgr.ActiveVault is null)
        {
            Console.Error.WriteLine("No vault is open. Use 'open-vault' first.");
            return 1;
        }
        Console.WriteLine(
            $"{"#",3} {"Label",-20} {"Address",-36} {"Balance (BTC)",15}");
        Console.WriteLine(new string('─', 76));
        foreach (var a in mgr.ActiveVault.Accounts)
        {
            var addr = a.Address.Length > 14
                ? $"{a.Address[..6]}...{a.Address[^6..]}"
                : a.Address;
            Console.WriteLine(
                $"{a.Index,3} {a.Label,-20} {addr,-36} {a.BalanceBtc,15:F8}");
        }
        var total = mgr.ActiveVault.Accounts.Sum(a => a.BalanceSatoshis);
        Console.WriteLine(new string('─', 76));
        Console.WriteLine(
            $"{"",3} {"Total",-20} {"",-36} {total / 100_000_000m,15:F8}");
        return 0;
    }

    private static int ShowPortfolio(WalletManager mgr, PortfolioTracker port)
    {
        if (mgr.ActiveVault is null)
        {
            Console.Error.WriteLine("No vault is open. Use 'open-vault' first.");
            return 1;
        }
        Console.Write(port.FormatSummary(mgr.ActiveVault));
        return 0;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
            Usage: btcwallet <command> [options]

            Commands:
              create-vault  --name <name>      Create a new wallet vault
              list-vaults                      List all vaults
              open-vault    --id <vault-id>    Open an existing vault
              add-account   --label <label>    Add account to active vault
              sync                             Synchronize balances
              balance                          Show account balances
              portfolio                        Show portfolio summary
              help                             Show this help message
            """);
    }
}
