using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Meilidown.Interfaces;
using Meilidown.Models.Index;
using Meilidown.Models.Sources;

namespace Meilidown.Services;

public class MarkdownFileProcessingService : IFileProcessingService
{
    private readonly ILogger<MarkdownFileProcessingService> _logger;
    
    public MarkdownFileProcessingService(ILogger<MarkdownFileProcessingService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async IAsyncEnumerable<IndexFile> ProcessAsync(IAsyncEnumerable<SourceFile> files, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
            var markdownPipeline = new MarkdownPipelineBuilder()
                .UseAbbreviations()
                // .UseAutoIdentifiers() // causes random "[]:" at the end
                .UseCitations()
                .UseCustomContainers()
                .UseDefinitionLists()
                .UseEmphasisExtras()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                .UseDiagrams()
                .UseAutoLinks()
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseDiagrams()
                .UseGenericAttributes()
                .Build();

            await foreach (var f in files.WithCancellation(cancellationToken))
            {
                _logger.LogInformation("Processing {File}", f.RelativePath);

                var content = await File.ReadAllTextAsync(f.AbsolutePath, cancellationToken);
                var document = Markdown.Parse(content, markdownPipeline);

                UpdateImageLinks(f, document);
                
                var builder = new StringBuilder();
                var writer = new StringWriter(builder);
                var renderer = new NormalizeRenderer(writer);
                renderer.Render(document);

                yield return new(
                    f.Uid,
                    f.Name,
                    builder.ToString(),
                    0,
                    f.Location
                );
            }
    }

    private static void UpdateImageLinks(SourceFile file, MarkdownObject markdownObject)
    {
        foreach (var child in markdownObject.Descendants())
        {
            if (child is not LinkInline { IsImage: true } link) continue;

            var parent = string.Join('/', file.RelativePath.Split(Path.DirectorySeparatorChar).SkipLast(1));
            parent = string.IsNullOrWhiteSpace(parent) ? "" : $"{parent}/";

            var location = parent + link.Url;
            if (location.StartsWith("."))
            {
                // only encoding when necessary to get cleaner URLs by default
                location = HttpUtility.UrlEncode(location);
            }

            link.Url = $"%API_HOST%/File/{location}";
            if (link.Reference != null) link.Reference.Url = null;
        }
    }
}