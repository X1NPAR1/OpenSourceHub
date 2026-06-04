namespace OpenSourceHub.Domain.Enums;

public enum AppTheme { System, Light, Dark }

public enum AppLanguage { English, Turkish, Russian, German, Dutch }

public enum RepositoryHealthLevel { Critical, Poor, Fair, Good, Excellent }

public enum SecurityRiskLevel { None, Low, Medium, High, Critical }

// Values are persisted as int — append new providers, never reorder.
public enum AiProvider { OpenAI = 0, Ollama = 1, Claude = 2, Gemini = 3, DeepSeek = 4 }

public enum ReportFormat { PDF, Markdown, HTML }

public enum NotificationType { Success, Warning, Error, Information }

public enum TrendPeriod { Daily, Weekly, Monthly }

public enum SortOrder { Ascending, Descending }

public enum RepositorySortBy { Stars, Forks, Updated, Created, Issues, Name }
