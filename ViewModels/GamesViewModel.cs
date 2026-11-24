using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels;

public sealed class GamesViewModel : ViewModelBase
{
    private readonly IGameLibraryService _library;
    private bool _isLoading;
    private string _searchText = string.Empty;
    private string _status = "Biblioteca sin analizar.";

    public GamesViewModel(IGameLibraryService library)
    {
        _library = library;
        RefreshCommand = new AsyncRelayCommand(async _ => await RefreshAsync());
    }

    public ObservableCollection<GameInstallation> Games { get; } = new();
    public ICommand RefreshCommand { get; }
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) OnPropertyChanged(nameof(FilteredGames)); } }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public IEnumerable<GameInstallation> FilteredGames => Games.Where(game =>
        string.IsNullOrWhiteSpace(SearchText) || game.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
        || game.Platform.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public async Task RefreshAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        Status = "Buscando instalaciones locales de Steam y Epic Games…";
        try
        {
            var results = await _library.DetectInstalledGamesAsync();
            Games.Clear();
            foreach (var game in results) Games.Add(game);
            OnPropertyChanged(nameof(FilteredGames));
            Status = results.Count == 0
                ? "No se verificaron juegos instalados en Steam o Epic Games en este equipo."
                : $"{results.Count} instalaciones detectadas. No se consultan compras de cuentas ni otros lanzadores.";
        }
        catch (Exception ex) { Status = $"No se pudo analizar la biblioteca: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
