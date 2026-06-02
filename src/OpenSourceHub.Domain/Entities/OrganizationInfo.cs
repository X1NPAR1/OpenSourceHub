namespace OpenSourceHub.Domain.Entities;

public class OrganizationInfo
{
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? HtmlUrl { get; set; }
    public string? Blog { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public int PublicRepos { get; set; }
    public int PublicGists { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public int MembersCount { get; set; }
    public int TeamsCount { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RepositoryInfo> TopRepositories { get; set; } = [];
    public List<ContributorInfo> TopMembers { get; set; } = [];
}
