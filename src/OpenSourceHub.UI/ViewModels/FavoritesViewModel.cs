using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace OpenSourceHub.UI.ViewModels;

public partial class FavoritesViewModel : BaseViewModel
{
    private readonly IFavoriteService _favorites;
    private List<FavoriteRepository> _all = [];

    [ObservableProperty] private ObservableCollection<FavoriteRepository> _items = [];
    [ObservableProperty] private FavoriteRepository? _selectedItem;
    [ObservableProperty] private string _editNote = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _sortBy = "Recently Added";
    [ObservableProperty] private string _selectedCategory = AllCategories;
    [ObservableProperty] private bool _isEditingNote;
    [ObservableProperty] private string _editCategory = "General";
    [ObservableProperty] private ObservableCollection<string> _categories = [];

    private const string AllCategories = "All Categories";

    public IReadOnlyList<string> SortOptions { get; } =
        new[] { "Recently Added", "Name (A-Z)", "Stars (High-Low)" };

    public int TotalCount => _all.Count;
    public bool HasItems => Items.Count > 0;

    public FavoritesViewModel(IFavoriteService favorites) => _favorites = favorites;

    [RelayCommand]
    public async Task LoadAsync()
    {
        SetLoading(true, "Loading favorites...");
        try
        {
            _all = (await _favorites.GetFavoritesAsync()).ToList();
            RebuildCategories();
            ApplyFilterAndSort();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load favorites: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilterAndSort();
    partial void OnSortByChanged(string value) => ApplyFilterAndSort();
    partial void OnSelectedCategoryChanged(string value) => ApplyFilterAndSort();

    private void RebuildCategories()
    {
        var cats = _all.Select(f => string.IsNullOrWhiteSpace(f.Category) ? "General" : f.Category)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                       .ToList();
        Categories = new ObservableCollection<string>(new[] { AllCategories }.Concat(cats));
        if (!Categories.Contains(SelectedCategory)) SelectedCategory = AllCategories;
    }

    private void ApplyFilterAndSort()
    {
        IEnumerable<FavoriteRepository> query = _all;

        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != AllCategories)
            query = query.Where(r => (string.IsNullOrWhiteSpace(r.Category) ? "General" : r.Category) == SelectedCategory);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim();
            query = query.Where(r =>
                r.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (r.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Language?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Note?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        query = SortBy switch
        {
            "Name (A-Z)" => query.OrderBy(r => r.FullName, StringComparer.OrdinalIgnoreCase),
            "Stars (High-Low)" => query.OrderByDescending(r => r.Stars),
            _ => query.OrderByDescending(r => r.AddedAt)
        };

        Items = new ObservableCollection<FavoriteRepository>(query);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasItems));
    }

    [RelayCommand]
    private async Task RemoveAsync(FavoriteRepository? item)
    {
        if (item == null) return;

        var confirm = MessageBox.Show(
            $"Remove \"{item.FullName}\" from your favorites?",
            "Remove Favorite",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        await _favorites.RemoveFavoriteAsync(item.FullName);
        _all.Remove(item);
        Items.Remove(item);
        if (SelectedItem == item) SelectedItem = null;
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasItems));
        Notifications.Info($"Removed {item.Name} from favorites.");
    }

    [RelayCommand]
    private void BeginEditNote(FavoriteRepository? item)
    {
        if (item == null) return;
        SelectedItem = item;
        EditNote = item.Note ?? string.Empty;
        EditCategory = string.IsNullOrWhiteSpace(item.Category) ? "General" : item.Category;
        IsEditingNote = true;
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        if (SelectedItem == null) return;
        var category = string.IsNullOrWhiteSpace(EditCategory) ? "General" : EditCategory.Trim();
        await _favorites.UpdateNoteAsync(SelectedItem.FullName, EditNote);
        await _favorites.UpdateCategoryAsync(SelectedItem.FullName, category);
        SelectedItem.Note = EditNote;
        SelectedItem.Category = category;
        IsEditingNote = false;
        RebuildCategories();
        ApplyFilterAndSort();
        Notifications.Success("Saved.");
    }

    [RelayCommand]
    private void CancelNote() => IsEditingNote = false;

    [RelayCommand]
    private void OpenRepository(FavoriteRepository? item)
    {
        if (item == null) return;
        try
        {
            Process.Start(new ProcessStartInfo($"https://github.com/{item.FullName}") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Notifications.Error($"Could not open browser: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CopyCloneUrl(FavoriteRepository? item)
    {
        if (item == null) return;
        try
        {
            Clipboard.SetText($"https://github.com/{item.FullName}.git");
            Notifications.Success("Clone URL copied to clipboard.");
        }
        catch (Exception ex)
        {
            Notifications.Error($"Copy failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();
}
