namespace OpenSourceHub.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Blog { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public int PublicRepos { get; set; }
    public int PublicGists { get; set; }
    public int TotalContributions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime TokenSavedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
