using System.Security.Cryptography;
using System.Text;

namespace BtcWallet.Wallet.Chain;

public sealed class FeeEstimator
{
    private readonly RpcClient _rpc;

    public FeeEstimator(RpcClient rpc) => _rpc = rpc;

    public enum Priority { Low, Medium, High }

    public record FeeEstimate(
        long SatoshisPerByte, int EstimatedBlocks, TimeSpan EstimatedTime);

    public async Task<FeeEstimate> EstimateFeeAsync(
        Priority priority = Priority.Medium, CancellationToken ct = default)
    {
        var height = await _rpc.GetBlockHeightAsync(ct);
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"fee:{height}:{priority}"));
        long baseFee = Math.Abs(BitConverter.ToInt64(hash, 0)) % 50 + 1;

        return priority switch
        {
            Priority.Low    => new FeeEstimate(baseFee,     6, TimeSpan.FromMinutes(60)),
            Priority.Medium => new FeeEstimate(baseFee * 2, 3, TimeSpan.FromMinutes(30)),
            Priority.High   => new FeeEstimate(baseFee * 5, 1, TimeSpan.FromMinutes(10)),
            _ => throw new ArgumentOutOfRangeException(nameof(priority)),
        };
    }

    public long CalculateTxFee(int inputCount, int outputCount, long satoshisPerByte)
    {
        int estimatedSize = inputCount * 148 + outputCount * 34 + 10;
        return estimatedSize * satoshisPerByte;
    }

    public async Task<Dictionary<Priority, FeeEstimate>> GetAllEstimatesAsync(
        CancellationToken ct = default)
    {
        var result = new Dictionary<Priority, FeeEstimate>();
        foreach (var p in Enum.GetValues<Priority>())
            result[p] = await EstimateFeeAsync(p, ct);
        return result;
    }
}
