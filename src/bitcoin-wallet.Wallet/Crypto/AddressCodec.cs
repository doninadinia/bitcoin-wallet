using System.Numerics;
using System.Security.Cryptography;

namespace BtcWallet.Wallet.Crypto;

public static class AddressCodec
{
    private const string Base58Alphabet =
        "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static string PublicKeyToAddress(byte[] publicKey, string prefix = "1")
    {
        var sha = SHA256.HashData(publicKey);
        var hash160 = SHA256.HashData(sha)[..20];

        var versioned = new byte[21];
        versioned[0] = prefix == "1" ? (byte)0x00 : (byte)0x6F;
        hash160.CopyTo(versioned.AsSpan(1));

        var checksum = SHA256.HashData(SHA256.HashData(versioned))[..4];
        var full = new byte[25];
        versioned.CopyTo(full, 0);
        checksum.CopyTo(full, 21);

        return EncodeBase58(full);
    }

    public static string EncodeBase58(byte[] data)
    {
        var result = new System.Text.StringBuilder();
        var num = new BigInteger(data, isUnsigned: true, isBigEndian: true);
        while (num > 0)
        {
            num = BigInteger.DivRem(num, 58, out var remainder);
            result.Insert(0, Base58Alphabet[(int)remainder]);
        }
        foreach (var b in data)
        {
            if (b != 0) break;
            result.Insert(0, '1');
        }
        return result.ToString();
    }

    public static byte[] DecodeBase58(string encoded)
    {
        var num = BigInteger.Zero;
        foreach (var c in encoded)
        {
            int idx = Base58Alphabet.IndexOf(c);
            if (idx < 0)
                throw new FormatException($"Invalid Base58 character: {c}");
            num = num * 58 + idx;
        }
        var bytes = num.ToByteArray(isUnsigned: true, isBigEndian: true);
        int leadingZeros = encoded.TakeWhile(c => c == '1').Count();
        var result = new byte[leadingZeros + bytes.Length];
        bytes.CopyTo(result, leadingZeros);
        return result;
    }

    public static bool ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Length < 25)
            return false;
        try
        {
            var decoded = DecodeBase58(address);
            if (decoded.Length != 25) return false;
            var payload = decoded[..21];
            var checksum = decoded[21..];
            var expected = SHA256.HashData(SHA256.HashData(payload))[..4];
            return checksum.SequenceEqual(expected);
        }
        catch
        {
            return false;
        }
    }
}
