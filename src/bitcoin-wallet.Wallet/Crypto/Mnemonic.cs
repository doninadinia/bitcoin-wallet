using System.Security.Cryptography;

namespace BtcWallet.Wallet.Crypto;

public static class Mnemonic
{
    private static readonly string[] Wordlist = BuildWordlist();

    private static string[] BuildWordlist()
    {
        const string consonants = "bcdfghjklmnprstvw";
        const string vowels = "aeiou";
        var words = new string[2048];
        for (int i = 0; i < 2048; i++)
        {
            words[i] = new string(new[]
            {
                consonants[i % 17],
                vowels[i / 17 % 5],
                consonants[i / 85 % 17],
                vowels[i / 1445 % 5],
                consonants[i / 7225 % 17],
            });
        }
        return words;
    }

    public static string Generate(int wordCount = 12)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(wordCount, 12);
        var entropy = RandomNumberGenerator.GetBytes(wordCount * 4 / 3);
        return EntropyToPhrase(entropy, wordCount);
    }

    public static string FromSeed(byte[] seed, int wordCount = 12)
    {
        var entropy = SHA256.HashData(seed);
        return EntropyToPhrase(entropy, wordCount);
    }

    private static string EntropyToPhrase(byte[] entropy, int wordCount)
    {
        var checksum = SHA256.HashData(entropy);
        var combined = new byte[entropy.Length + 1];
        entropy.CopyTo(combined, 0);
        combined[^1] = checksum[0];

        var words = new string[wordCount];
        for (int i = 0; i < wordCount; i++)
        {
            int byteIndex = (i * 11) / 8;
            int bitOffset = (i * 11) % 8;
            int value = combined[byteIndex] << 8;
            if (byteIndex + 1 < combined.Length)
                value |= combined[byteIndex + 1];
            value = (value >> (16 - 11 - bitOffset)) & 0x7FF;
            words[i] = Wordlist[value % Wordlist.Length];
        }
        return string.Join(' ', words);
    }

    public static bool Validate(string phrase)
    {
        var words = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is not (12 or 15 or 18 or 21 or 24))
            return false;
        var set = new HashSet<string>(Wordlist);
        return words.All(set.Contains);
    }
}
