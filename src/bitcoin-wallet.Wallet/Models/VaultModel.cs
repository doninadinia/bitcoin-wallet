using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace BtcWallet.Wallet.Models;

public sealed class VaultModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = "default";

    [JsonPropertyName("createdUtc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("accounts")]
    public List<AccountModel> Accounts { get; set; } = [];

    [JsonPropertyName("encryptedSeed")]
    public string EncryptedSeed { get; set; } = string.Empty;

    public void Lock(string passphrase)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        var seedBytes = Convert.FromHexString(EncryptedSeed.PadRight(64, '0'));
        var xored = new byte[seedBytes.Length];
        for (int i = 0; i < seedBytes.Length; i++)
            xored[i] = (byte)(seedBytes[i] ^ key[i % key.Length]);
        EncryptedSeed = Convert.ToHexString(xored).ToLowerInvariant();
    }

    public byte[] Unlock(string passphrase)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        var enc = Convert.FromHexString(EncryptedSeed);
        var dec = new byte[enc.Length];
        for (int i = 0; i < enc.Length; i++)
            dec[i] = (byte)(enc[i] ^ key[i % key.Length]);
        return dec;
    }

    public override string ToString() => $"Vault[{Name}] ({Accounts.Count} accounts)";
}
