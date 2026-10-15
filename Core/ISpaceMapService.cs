using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;
using System;

namespace Wpc_SutilBox.Core;

public interface ISpaceMapService
{
    Task<IReadOnlyList<SpaceMapEntry>> AnalyzeAsync(IEnumerable<string> roots, CancellationToken cancellationToken = default);
    Task<SpaceMapDeleteResult> DeleteSelectedAsync(IReadOnlyCollection<SpaceMapEntry> entries, IEnumerable<string> authorizedRoots, CancellationToken cancellationToken = default);
}

public sealed record SpaceMapDeleteResult(int Deleted, int Skipped, int Failed, long EstimatedBytes);
