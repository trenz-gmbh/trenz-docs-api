namespace TRENZ.Docs.API.Models.Sources;

public enum SourceType
{
    Unknown = -1,
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

    public static SourceType FromString(string sourceType)
    {
        return sourceType switch
        {
            "Git" => SourceType.Git,
            "Local" => SourceType.Local,
            _ => SourceType.Unknown,
        };
    }
}