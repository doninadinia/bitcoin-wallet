using System.Security.Cryptography;
using System.Text;

namespace BtcWallet.Wallet.Crypto;

public static class KeyDerivation
{
    private const int ChildKeyLength = 32;

    public static byte[] SeedFromMnemonic(string mnemonic, string passphrase = "")
    {
        var salt = Encoding.UTF8.GetBytes("mnemonic" + passphrase);
        var password = Encoding.UTF8.GetBytes(mnemonic);
        return Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 2048, HashAlgorithmName.SHA512, 64);
    }

    public static byte[] DeriveChildKey(byte[] parentKey, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        var data = new byte[parentKey.Length + 4];
        parentKey.CopyTo(data, 0);
        BitConverter.GetBytes(index).CopyTo(data, parentKey.Length);
        return HMACSHA512.HashData(parentKey[..32], data)[..ChildKeyLength];
    }

    public static (byte[] PrivateKey, byte[] PublicKey) DeriveKeyPair(
        byte[] seed, int accountIndex)
    {
        var masterKey = HMACSHA512.HashData("Bitcoin seed"u8, seed);
        var purpose  = DeriveChildKey(masterKey[..32], 44);
        var coinType = DeriveChildKey(purpose, 0);
        var account  = DeriveChildKey(coinType, accountIndex);
        var change   = DeriveChildKey(account, 0);
        var privKey  = DeriveChildKey(change, 0);
        var pubKey   = SHA256.HashData(privKey);
        return (privKey, pubKey);
    }

    public static string Fingerprint(byte[] key)
    {
        var hash = SHA256.HashData(key);
        return Convert.ToHexString(hash[..4]).ToLowerInvariant();
    }
}
