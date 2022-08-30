using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services;

public class MarkdownFileProcessingService : IFileProcessingService
{
    private static string ToMd5(string text) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));

    private readonly ILogger<MarkdownFileProcessingService> _logger;

    public MarkdownFileProcessingService(ILogger<MarkdownFileProcessingService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IndexFile> ProcessAsync(IEnumerable<ISourceFile> files, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var markdownPipelineBuilder = new MarkdownPipelineBuilder();
        var markdownPipeline = markdownPipelineBuilder.Build();

        foreach (var f in files)
        {
            _logger.LogInformation("Processing {File}", f.RelativePath);

            var content = await f.GetTextAsync(cancellationToken);
            var document = Markdown.Parse(content, markdownPipeline);

            foreach (var child in document.Descendants())
            {
                if (child is not LinkInline link)
                    continue;

                if (link.Url == null)
                    continue;

                link.Url = RewriteLinks(link.Url, link.IsImage, f.RelativePath);
                if (link.Reference != null) link.Reference.Url = null;
            }

            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var renderer = new NormalizeRenderer(writer);
            renderer.Render(document);

            yield return new(
                ToMd5(f.Location),
                f.Name,
                builder.ToString(),
                f.Location
            );
        }
    }

    public static string RewriteLinks(string original, bool isImage, string relativePath)
    {
        var parent = string.Join('/', relativePath.Split(Path.DirectorySeparatorChar).SkipLast(1));
        parent = string.IsNullOrWhiteSpace(parent) ? "" : $"{parent}/";

        var path = HttpUtility.UrlDecode(original ?? "");
        var location = parent + path;
        if (isImage)
            return RewriteImageLink(location);

        return Uri.TryCreate(path, UriKind.Absolute, out _) ? original! : RewriteInlineLink(location);
    }

    private static string RewriteImageLink(string location)
    {
        location = CanonicalizeUrl(location);
        return $"%API_HOST%/File/{location}";
    }

    private static string RewriteInlineLink(string location)
    {
        location = NavNode.PathToLocation(location.EndsWith(".md") ? location[..^3] : location);
        location = "/wiki/" + CanonicalizeUrl(location);
        return HttpUtility.UrlPathEncode(location);
    }

    private static string CanonicalizeUrl(string location)
    {
        if (location.Contains(".."))
        {
            var parts = location.Split('/');
            var newParts = new List<string>();
            foreach (var part in parts)
            {
                if (part == "..")
                {
                    if (newParts.Count - 1 >= 0)
                        newParts.RemoveAt(newParts.Count - 1);
                    else
                        throw new ArgumentException("Cannot resolve relative paths outside of source paths.");
                }
                else
                    newParts.Add(part);
            }

            location = string.Join('/', newParts);
        }

        location = location.Replace("./", "");

        return location;
    }
}