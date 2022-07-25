using System.Security.Cryptography;
using System.Text;

namespace TRENZ.Docs.API.Models.Sources;

public record SourceFile(ISource Source, string RelativePath)
{
    private const char Separator = '#';

    private static string ToMd5(string text) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));

    public string AbsolutePath => Path.Combine(Source.Root, Source.Path, RelativePath).Replace('/', Path.DirectorySeparatorChar);

    public string Location => string.Join('.', RelativePath.Split('.').SkipLast(1)).Replace(Path.DirectorySeparatorChar, '/');

    public string Name => Path.GetFileNameWithoutExtension(RelativePath);

    public string Uid => ToMd5(Source.Name + Separator + RelativePath);
}
