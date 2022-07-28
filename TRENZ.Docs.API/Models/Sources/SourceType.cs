namespace TRENZ.Docs.API.Models.Sources;

public enum SourceType
{
    Git,
    Local,
}

public static class SourceTypeExtensions
{
    public static string GetValue(this SourceType sourceType)
    {
        return sourceType switch
        {
            SourceType.Git => "Git",
            SourceType.Local => "Local",
            _ => "Unknown",
        };
    }
}