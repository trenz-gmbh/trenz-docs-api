using System.Security.Cryptography;
using System.Text;

namespace Meilidown;

public record RepositoryFile(RepositoryConfiguration Config, string RelativePath)
{
    public const char Separator = '_';

    private static string ToMd5(string text) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));

    public string AbsolutePath => Path.Combine(Config.Root, Config.Path, RelativePath);

    public string Uid => ToMd5(Config.Name + Separator + Path.GetFileNameWithoutExtension(AbsolutePath));

    public string? ParentUid => Path.GetRelativePath(Directory.GetParent(AbsolutePath)?.FullName ?? "", Path.Combine(Config.Root, Config.Path)) != "." ? ToMd5(Config.Name + Separator + Directory.GetParent(AbsolutePath)!.Name) : null;
}
