namespace OpenSourceHub.Domain.Entities;

public class ContributorInfo
{
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? HtmlUrl { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public int Contributions { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public int PublicRepos { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<MonthlyContribution> MonthlyActivity { get; set; } = [];
}

public class MonthlyContribution
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int CommitCount { get; set; }
}
