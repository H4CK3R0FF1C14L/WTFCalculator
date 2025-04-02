using System.Threading.Channels;
using WTFCalculator.Models;

namespace WTFCalculator.Services
{
    public class OptimizedCombinationFinderService
    {
        private readonly ChannelWriter<Dictionary<string, int>> _resultsWriter;
        private readonly List<Item> _items;
        private readonly decimal _targetSum;

        public OptimizedCombinationFinderService(List<Item> items, decimal targetSum, ChannelWriter<Dictionary<string, int>> resultsWriter)
        {
            _items = items
                .Where(item => item.Cost > 0 && item.Count > 0)
                .OrderByDescending(item => item.Cost)
                .ToList();
            _targetSum = targetSum;
            _resultsWriter = resultsWriter;
        }

        public async Task FindCombinationsAsync()
        {
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(
                GetInitialRanges(),
                parallelOptions,
                async (range, ct) =>
                {
                    int[] quantities = new int[_items.Count];
                    await ProcessFirstItemRangeAsync(range, quantities, ct);
                });

            _resultsWriter.Complete();
        }

        private IEnumerable<(int Start, int End)> GetInitialRanges()
        {
            Item firstItem = _items[0];
            int maxQ = Math.Min(
                (int)(_targetSum / firstItem.Cost!),
                firstItem.Count!.Value);

            int chunkSize = Math.Max(1, maxQ / Environment.ProcessorCount);

            for (int start = 0; start <= maxQ; start += chunkSize)
            {
                int end = Math.Min(start + chunkSize - 1, maxQ);
                yield return (start, end);
            }
        }

        private async ValueTask ProcessFirstItemRangeAsync(
            (int Start, int End) range,
            int[] quantities,
            CancellationToken ct)
        {
            for (int q = range.Start; q <= range.End; q++)
            {
                quantities[0] = q;
                decimal currentSum = q * (decimal)_items[0].Cost!;

                if (currentSum > _targetSum) continue;

                await ProcessItemAsync(1, currentSum, quantities, ct);
            }
        }

        private async ValueTask ProcessItemAsync(int index, decimal currentSum, int[] quantities, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            if (index == _items.Count)
            {
                if (currentSum == _targetSum)
                {
                    Dictionary<string, int> result = new Dictionary<string, int>();
                    for (int i = 0; i < quantities.Length; i++)
                    {
                        if (quantities[i] > 0)
                        {
                            result[_items[i].Name!] = quantities[i];
                        }
                    }
                    await _resultsWriter.WriteAsync(result, ct);
                }
                return;
            }

            Item item = _items[index];
            int maxQ = Math.Min(
                (int)((_targetSum - currentSum) / item.Cost!),
                item.Count!.Value);

            for (int q = 0; q <= maxQ; q++)
            {
                quantities[index] = q;
                decimal newSum = currentSum + q * (decimal)item.Cost!;

                if (newSum > _targetSum) break;

                await ProcessItemAsync(index + 1, newSum, quantities, ct);
            }

            quantities[index] = 0; // Reset for next iteration
        }
    }
}
