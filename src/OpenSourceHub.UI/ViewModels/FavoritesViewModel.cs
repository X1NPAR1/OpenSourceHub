using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace OpenSourceHub.UI.ViewModels;

public partial class FavoritesViewModel : BaseViewModel
{
    private readonly IFavoriteService _favorites;

    [ObservableProperty] private ObservableCollection<FavoriteRepository> _items = [];
    [ObservableProperty] private FavoriteRepository? _selectedItem;
    [ObservableProperty] private string _editNote = string.Empty;

    public FavoritesViewModel(IFavoriteService favorites) => _favorites = favorites;

    [RelayCommand]
    public async Task LoadAsync()
    {
        var list = await _favorites.GetFavoritesAsync();
        Items = new ObservableCollection<FavoriteRepository>(list);
    }

    [RelayCommand]
    private async Task RemoveAsync(FavoriteRepository? item)
    {
        if (item == null) return;
        await _favorites.RemoveFavoriteAsync(item.FullName);
        Items.Remove(item);
        Notifications.Info($"Removed {item.Name} from favorites.");
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        if (SelectedItem == null) return;
        await _favorites.UpdateNoteAsync(SelectedItem.FullName, EditNote);
        SelectedItem.Note = EditNote;
        Notifications.Success("Note saved.");
    }

    [RelayCommand]
    private void OpenRepository(FavoriteRepository? item)
    {
        if (item != null)
            Process.Start(new ProcessStartInfo($"https://github.com/{item.FullName}") { UseShellExecute = true });
    }

    partial void OnSelectedItemChanged(FavoriteRepository? value)
        => EditNote = value?.Note ?? string.Empty;
}
