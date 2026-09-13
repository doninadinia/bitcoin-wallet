using Xunit;
using BtcWallet.Wallet.Crypto;

namespace BtcWallet.Wallet.Tests;

public class KeyDerivationTests
{
    [Fact]
    public void SeedFromMnemonic_SameInput_SameOutput()
    {
        var seed1 = KeyDerivation.SeedFromMnemonic("test mnemonic phrase");
        var seed2 = KeyDerivation.SeedFromMnemonic("test mnemonic phrase");
        Assert.Equal(seed1, seed2);
    }

    [Fact]
    public void SeedFromMnemonic_DifferentPassphrase_DifferentSeed()
    {
        var seed1 = KeyDerivation.SeedFromMnemonic("test", "pass1");
        var seed2 = KeyDerivation.SeedFromMnemonic("test", "pass2");
        Assert.NotEqual(seed1, seed2);
    }

    [Fact]
    public void SeedFromMnemonic_ReturnsExpectedLength()
    {
        var seed = KeyDerivation.SeedFromMnemonic("hello world");
        Assert.Equal(64, seed.Length);
    }

    [Fact]
    public void DeriveKeyPair_ProducesConsistentResults()
    {
        var seed = KeyDerivation.SeedFromMnemonic("test mnemonic");
        var (priv1, pub1) = KeyDerivation.DeriveKeyPair(seed, 0);
        var (priv2, pub2) = KeyDerivation.DeriveKeyPair(seed, 0);
        Assert.Equal(priv1, priv2);
        Assert.Equal(pub1, pub2);
    }

    [Fact]
    public void DeriveKeyPair_DifferentIndices_DifferentKeys()
    {
        var seed = KeyDerivation.SeedFromMnemonic("test mnemonic");
        var (priv1, _) = KeyDerivation.DeriveKeyPair(seed, 0);
        var (priv2, _) = KeyDerivation.DeriveKeyPair(seed, 1);
        Assert.NotEqual(priv1, priv2);
    }

    [Fact]
    public void DeriveKeyPair_KeysAre32Bytes()
    {
        var seed = KeyDerivation.SeedFromMnemonic("test");
        var (priv, pub) = KeyDerivation.DeriveKeyPair(seed, 0);
        Assert.Equal(32, priv.Length);
        Assert.Equal(32, pub.Length);
    }

    [Fact]
    public void Fingerprint_IsConsistent()
    {
        var key = new byte[] { 1, 2, 3, 4 };
        var fp1 = KeyDerivation.Fingerprint(key);
        var fp2 = KeyDerivation.Fingerprint(key);
        Assert.Equal(fp1, fp2);
        Assert.Equal(8, fp1.Length);
    }

    [Fact]
    public void AddressCodec_RoundTrip()
    {
        var seed = KeyDerivation.SeedFromMnemonic("round trip test");
        var (_, pub) = KeyDerivation.DeriveKeyPair(seed, 0);
        var address = AddressCodec.PublicKeyToAddress(pub);
        Assert.True(AddressCodec.ValidateAddress(address));
    }

    [Fact]
    public void Mnemonic_Generate_Returns12Words()
    {
        var phrase = Mnemonic.Generate(12);
        var words = phrase.Split(' ');
        Assert.Equal(12, words.Length);
    }

    [Fact]
    public void Mnemonic_Validate_AcceptsGenerated()
    {
        var phrase = Mnemonic.Generate(12);
        Assert.True(Mnemonic.Validate(phrase));
    }
}
