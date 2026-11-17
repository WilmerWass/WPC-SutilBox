using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public sealed class BenchmarkService : IBenchmarkService
    {
        public Task<BenchmarkResult> RunAsync(CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                long primeCount = CountPrimes(TimeSpan.FromSeconds(2), cancellationToken);
                double copyRate = MeasureMemoryCopy(TimeSpan.FromMilliseconds(500), cancellationToken);
                return new BenchmarkResult
                {
                    PrimeCount = primeCount,
                    MemoryCopyMegabytesPerSecond = copyRate
                };
            }, cancellationToken);

        private static long CountPrimes(TimeSpan duration, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            long count = 0;
            int candidate = 2;
            while (stopwatch.Elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IsPrime(candidate))
                    count++;
                candidate++;
            }
            return count;
        }

        private static bool IsPrime(int number)
        {
            if (number < 2) return false;
            for (int divisor = 2; divisor <= number / divisor; divisor++)
            {
                if (number % divisor == 0)
                    return false;
            }
            return true;
        }

        private static double MeasureMemoryCopy(TimeSpan duration, CancellationToken cancellationToken)
        {
            const int bufferSize = 16 * 1024 * 1024;
            var source = new byte[bufferSize];
            var destination = new byte[bufferSize];
            var stopwatch = Stopwatch.StartNew();
            long bytesCopied = 0;

            while (stopwatch.Elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Buffer.BlockCopy(source, 0, destination, 0, bufferSize);
                bytesCopied += bufferSize;
            }

            return bytesCopied / 1024d / 1024d / stopwatch.Elapsed.TotalSeconds;
        }
    }
}
