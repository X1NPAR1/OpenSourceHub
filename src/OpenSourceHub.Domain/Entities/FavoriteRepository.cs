namespace OpenSourceHub.Domain.Entities;

public class FavoriteRepository
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int Stars { get; set; }
    public string? Note { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
