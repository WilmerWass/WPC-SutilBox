using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public interface IStorageScanService
{
    IReadOnlyList<StorageVolumeInfo> GetVolumes();
    Task<StorageScanResult> ScanAsync(string rootPath, IProgress<StorageScanProgress>? progress = null, CancellationToken cancellationToken = default);
}
