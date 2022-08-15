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
    public async IAsyncEnumerable<IndexFile> ProcessAsync(IAsyncEnumerable<ISourceFile> files, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

            await foreach (var f in files.WithCancellation(cancellationToken))
            {
                _logger.LogInformation("Processing {File}", f.RelativePath);

                var content = await f.GetTextAsync(cancellationToken);
                var document = Markdown.Parse(content, markdownPipeline);

                RewriteLinks(f, document);

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

    private static void RewriteLinks(ISourceFile file, MarkdownObject markdownObject)
    {
        foreach (var child in markdownObject.Descendants())
        {
            if (child is not LinkInline link)
                continue;

            var path = HttpUtility.UrlDecode(link.Url ?? "");
            if (link.IsImage)
            {
                path = RewriteImageLink(path, file);
            }
            else
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out _))
                {
                    continue;
                }

                path = RewriteLink(path);
            }

            link.Url = HttpUtility.UrlPathEncode(NavNode.PathToLocation(path));
            if (link.Reference != null) link.Reference.Url = null;
        }
    }

    private static string RewriteImageLink(string? orignalUrl, ISourceFile file)
    {
        var parent = string.Join('/', file.RelativePath.Split(Path.DirectorySeparatorChar).SkipLast(1));
        parent = string.IsNullOrWhiteSpace(parent) ? "" : $"{parent}/";

        var location = parent + orignalUrl;
        return $"%API_HOST%/File/{location}";
    }

    private static string RewriteLink(string originalUrl)
    {
        return originalUrl.EndsWith(".md") ? originalUrl[..^3] : originalUrl;
    }
}