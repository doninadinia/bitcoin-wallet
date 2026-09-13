using System.Text.Json;
using System.Text.Json.Nodes;

namespace BtcWallet.Wallet.Storage;

public static class Migrations
{
    private const int CurrentVersion = 2;

    public static async Task<bool> MigrateIfNeededAsync(
        string storageDir, CancellationToken ct = default)
    {
        Directory.CreateDirectory(storageDir);
        var versionFile = Path.Combine(storageDir, ".version");
        int version = 0;

        if (File.Exists(versionFile))
        {
            var text = await File.ReadAllTextAsync(versionFile, ct);
            int.TryParse(text.Trim(), out version);
        }

        if (version >= CurrentVersion) return false;

        bool migrated = false;

        if (version < 1)
        {
            await MigrateV0ToV1Async(storageDir, ct);
            migrated = true;
        }
        if (version < 2)
        {
            await MigrateV1ToV2Async(storageDir, ct);
            migrated = true;
        }

        await File.WriteAllTextAsync(
            versionFile, CurrentVersion.ToString(), ct);
        return migrated;
    }

    private static async Task MigrateV0ToV1Async(
        string dir, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(dir, "*.vault"))
        {
            ct.ThrowIfCancellationRequested();
            var json = await File.ReadAllTextAsync(file, ct);
            var node = JsonNode.Parse(json);
            if (node is null) continue;

            var accounts = node["accounts"]?.AsArray();
            if (accounts is null) continue;

            bool modified = false;
            foreach (var acc in accounts)
            {
                if (acc is JsonObject obj && !obj.ContainsKey("network"))
                {
                    obj["network"] = "mainnet";
                    modified = true;
                }
            }

            if (modified)
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                await File.WriteAllTextAsync(
                    file, node.ToJsonString(opts), ct);
            }
        }
    }

    private static async Task MigrateV1ToV2Async(
        string dir, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(dir, "*.wallet"))
        {
            ct.ThrowIfCancellationRequested();
            var newPath = Path.ChangeExtension(file, ".vault");
            if (!File.Exists(newPath))
                File.Move(file, newPath);
        }
    }
}
