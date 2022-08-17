using System.Runtime.CompilerServices;
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
    private readonly ILogger<MarkdownFileProcessingService> _logger;
    
    public MarkdownFileProcessingService(ILogger<MarkdownFileProcessingService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async IAsyncEnumerable<IndexFile> ProcessAsync(IEnumerable<ISourceFile> files, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
            var markdownPipeline = new MarkdownPipelineBuilder()
                // .UseAbbreviations()
                // .UseAutoIdentifiers() // causes random "[]:" at the end
                // .UseCitations()
                // .UseCustomContainers()
                // .UseDefinitionLists()
                // .UseEmphasisExtras()
                // .UseFigures()
                // .UseFooters()
                // .UseFootnotes()
                // .UseGridTables()
                // .UseMathematics()
                // .UseMediaLinks()
                // .UsePipeTables()
                // .UseListExtras()
                // .UseTaskLists()
                // .UseDiagrams()
                // .UseAutoLinks()
                // .UseEmojiAndSmiley()
                // .UseYamlFrontMatter()
                // .UseDiagrams()
                // .UseGenericAttributes()
                .Build();

            foreach (var f in files)
            {
                _logger.LogInformation("Processing {File}", f.RelativePath);

                var content = await f.GetTextAsync(cancellationToken);
                var document = Markdown.Parse(content, markdownPipeline);

                foreach (var child in document.Descendants())
                {
                    if (child is not LinkInline link)
                        continue;

                    link.Url = RewriteLinks(link.Url, link.IsImage, f.RelativePath);
                    if (link.Reference != null) link.Reference.Url = null;
                }

                var builder = new StringBuilder();
                var writer = new StringWriter(builder);
                var renderer = new NormalizeRenderer(writer);
                renderer.Render(document);

                yield return new(
                    f.Uid,
                    f.Name,
                    builder.ToString(),
                    f.Location
                );
            }
    }

    public static string? RewriteLinks(string? original, bool isImage, string relativePath)
    {
        var path = HttpUtility.UrlDecode(original ?? "");
        if (isImage)
            return RewriteImageLink(path, relativePath);

        return Uri.TryCreate(path, UriKind.Absolute, out _) ? original : RewriteInlineLink(path);
    }

    private static string RewriteImageLink(string? decodedUrl, string relativePath)
    {
        var parent = string.Join('/', relativePath.Split(Path.DirectorySeparatorChar).SkipLast(1));
        parent = string.IsNullOrWhiteSpace(parent) ? "" : $"{parent}/";

        var location = parent + decodedUrl;
        if (location.StartsWith("."))
        {
            // only encoding when necessary to get cleaner URLs by default
            location = HttpUtility.UrlPathEncode(location);
        }

        return $"%API_HOST%/File/{location}";
    }

    private static string RewriteInlineLink(string decodedUrl)
    {
        var location = NavNode.PathToLocation(decodedUrl.EndsWith(".md") ? decodedUrl[..^3] : decodedUrl);

        return HttpUtility.UrlPathEncode(location);
    }
}