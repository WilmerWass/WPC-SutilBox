using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public interface IGameLibraryService
{
    Task<IReadOnlyList<GameInstallation>> DetectInstalledGamesAsync(CancellationToken cancellationToken = default);
}
