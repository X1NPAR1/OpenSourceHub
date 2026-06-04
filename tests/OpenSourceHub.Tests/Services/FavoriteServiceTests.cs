using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Infrastructure.Data;
using OpenSourceHub.Infrastructure.Services;

namespace OpenSourceHub.Tests.Services;

public class FavoriteServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task AddFavorite_AddsToDatabase()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository
        {
            FullName = "microsoft/vscode",
            Owner = "microsoft",
            Name = "vscode",
            Stars = 150000
        });

        var favorites = await service.GetFavoritesAsync();
        favorites.Should().HaveCount(1);
        favorites[0].FullName.Should().Be("microsoft/vscode");
    }

    [Fact]
    public async Task AddFavorite_DoesNotDuplicate()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        var fav = new FavoriteRepository { FullName = "torvalds/linux", Owner = "torvalds", Name = "linux" };
        await service.AddFavoriteAsync(fav);
        await service.AddFavoriteAsync(fav);

        var favorites = await service.GetFavoritesAsync();
        favorites.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveFavorite_RemovesFromDatabase()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });
        await service.RemoveFavoriteAsync("test/repo");

        var favorites = await service.GetFavoritesAsync();
        favorites.Should().BeEmpty();
    }

    [Fact]
    public async Task IsFavorite_ReturnsTrue_WhenExists()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });
        var result = await service.IsFavoriteAsync("test/repo");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFavorite_ReturnsFalse_WhenNotExists()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        var result = await service.IsFavoriteAsync("nonexistent/repo");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNote_SetsNoteOnFavorite()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });
        await service.UpdateNoteAsync("test/repo", "Great project!");

        var favorites = await service.GetFavoritesAsync();
        favorites[0].Note.Should().Be("Great project!");
    }

    [Fact]
    public async Task UpdateCategory_SetsCategory()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });
        await service.UpdateCategoryAsync("test/repo", "Work");

        var favorites = await service.GetFavoritesAsync();
        favorites[0].Category.Should().Be("Work");
    }

    [Fact]
    public async Task UpdateCategory_BlankFallsBackToGeneral()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });
        await service.UpdateCategoryAsync("test/repo", "   ");

        var favorites = await service.GetFavoritesAsync();
        favorites[0].Category.Should().Be("General");
    }

    [Fact]
    public async Task NewFavorite_DefaultsToGeneralCategory()
    {
        using var ctx = CreateContext();
        var service = new FavoriteService(ctx);

        await service.AddFavoriteAsync(new FavoriteRepository { FullName = "test/repo", Owner = "test", Name = "repo" });

        var favorites = await service.GetFavoritesAsync();
        favorites[0].Category.Should().Be("General");
    }
}
