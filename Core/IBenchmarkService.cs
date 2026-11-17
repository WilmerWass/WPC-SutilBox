using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IBenchmarkService
    {
        Task<BenchmarkResult> RunAsync(CancellationToken cancellationToken);
    }
}
