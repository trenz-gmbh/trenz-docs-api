using System.Security.Cryptography;
using System.Text;

namespace Meilidown.Models;

public record RepositoryFile(RepositoryConfiguration Config, string RelativePath)
{
    private const char Separator = '#';

    private static string ToMd5(string text) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));

    public string AbsolutePath => Path.Combine(Config.Root, Config.Path, RelativePath).Replace('/', Path.DirectorySeparatorChar);

    public string Location => string.Join('.', RelativePath.Split('.').SkipLast(1)).Replace(Path.DirectorySeparatorChar, '/');

    public string Name => Path.GetFileNameWithoutExtension(RelativePath);

    public string Uid => ToMd5(Config.Name + Separator + RelativePath);
}
