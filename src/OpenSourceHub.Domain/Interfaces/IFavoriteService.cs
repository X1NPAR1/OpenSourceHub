using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface IFavoriteService
{
    Task<List<FavoriteRepository>> GetFavoritesAsync();
    Task AddFavoriteAsync(FavoriteRepository favorite);
    Task RemoveFavoriteAsync(string fullName);
    Task<bool> IsFavoriteAsync(string fullName);
    Task UpdateNoteAsync(string fullName, string note);
    Task UpdateCategoryAsync(string fullName, string category);
}
