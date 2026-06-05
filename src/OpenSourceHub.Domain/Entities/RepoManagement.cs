namespace OpenSourceHub.Domain.Entities;

public class GitBranch
{
    public string Name { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
}

public class GitFileEntry
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "file"; // "file" | "dir"
    public long Size { get; set; }
    public string? Sha { get; set; }
    public bool IsDirectory => Type == "dir";
}

public class GitFileContent
{
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public bool IsBinary { get; set; }
}

public class PullRequestInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public string BaseBranch { get; set; } = string.Empty;
    public string HeadBranch { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public int Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
