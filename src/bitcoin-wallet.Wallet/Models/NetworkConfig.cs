namespace BtcWallet.Wallet.Models;

public sealed class NetworkConfig
{
    public string Name { get; init; } = "mainnet";
    public string RpcEndpoint { get; init; } = "https://localhost:8332";
    public string AddressPrefix { get; init; } = "1";
    public int ConfirmationsRequired { get; init; } = 6;
    public long DustThresholdSatoshis { get; init; } = 546;
    public int BlockTimeSeconds { get; init; } = 600;

    public static NetworkConfig Mainnet => new()
    {
        Name = "mainnet",
        RpcEndpoint = "https://localhost:8332",
        AddressPrefix = "1",
        ConfirmationsRequired = 6,
        DustThresholdSatoshis = 546,
        BlockTimeSeconds = 600,
    };

    public static NetworkConfig Testnet => new()
    {
        Name = "testnet",
        RpcEndpoint = "https://localhost:18332",
        AddressPrefix = "m",
        ConfirmationsRequired = 3,
        DustThresholdSatoshis = 546,
        BlockTimeSeconds = 600,
    };

    public static NetworkConfig Regtest => new()
    {
        Name = "regtest",
        RpcEndpoint = "https://localhost:18443",
        AddressPrefix = "m",
        ConfirmationsRequired = 1,
        DustThresholdSatoshis = 546,
        BlockTimeSeconds = 0,
    };

    public static NetworkConfig ByName(string name) => name.ToLowerInvariant() switch
    {
        "mainnet" or "main" => Mainnet,
        "testnet" or "test" => Testnet,
        "regtest" or "reg" => Regtest,
        _ => throw new ArgumentException($"Unknown network: {name}", nameof(name)),
    };

    public override string ToString() => $"{Name} ({RpcEndpoint})";
}
