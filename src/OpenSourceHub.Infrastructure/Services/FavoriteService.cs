using Microsoft.EntityFrameworkCore;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.Data;

namespace OpenSourceHub.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _db;

    public FavoriteService(AppDbContext db) => _db = db;

    public async Task<List<FavoriteRepository>> GetFavoritesAsync()
        => await _db.FavoriteRepositories.OrderByDescending(f => f.AddedAt).ToListAsync();

    public async Task AddFavoriteAsync(FavoriteRepository favorite)
    {
        if (!await IsFavoriteAsync(favorite.FullName))
        {
            _db.FavoriteRepositories.Add(favorite);
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveFavoriteAsync(string fullName)
    {
        var item = await _db.FavoriteRepositories.FirstOrDefaultAsync(f => f.FullName == fullName);
        if (item != null)
        {
            _db.FavoriteRepositories.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsFavoriteAsync(string fullName)
        => await _db.FavoriteRepositories.AnyAsync(f => f.FullName == fullName);

    public async Task UpdateNoteAsync(string fullName, string note)
    {
        var item = await _db.FavoriteRepositories.FirstOrDefaultAsync(f => f.FullName == fullName);
        if (item != null)
        {
            item.Note = note;
            await _db.SaveChangesAsync();
        }
    }
}
