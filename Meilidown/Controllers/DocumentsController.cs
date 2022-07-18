using Meilidown.Interfaces;
using Meilidown.Models.Index;
using Meilidown.Models.Sources;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IIndexingService _indexingService;
    private readonly ILogger<DocumentsController> _logger;
    private readonly ISourcesProvider _sourcesProvider;

    public DocumentsController(IIndexingService indexingService,
                               ISourcesProvider sourcesProvider,
                               ILogger<DocumentsController> logger)
    {
        _indexingService = indexingService;
        _sourcesProvider = sourcesProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<Dictionary<string, NavNode>> NavTree()
    {
        var allDocs = await _indexingService.GetIndexedFiles();
        var tree = new Dictionary<string, NavNode>();

        foreach (var doc in allDocs)
        {
            var path = doc.location.Split('/');
            var currentPath = new List<string>();
            var node = tree;
            for (var i = 0; i < path.Length; i++)
            {
                var part = path[i];
                currentPath.Add(part);

                if (!node.ContainsKey(part))
                {
                    node[part] = new(
                        doc.uid,
                        part,
                        string.Join('/', currentPath)
                    );
                }

                if (i + 1 < path.Length)
                {
                    node = node[part].children ??= new();
                }
            }
        }


        var orderFiles = _sourcesProvider.GetSources()
                                         .SelectMany(source => source.FindFiles(new("\\.order")))
                                         .ToDictionary(sf => sf.RelativePath.Split('/')[0..^1],
                                                       sf => sf);

        await SetOrder(tree, orderFiles);

        return tree;
    }

    private async Task SetOrder(Dictionary<string, NavNode> tree, Dictionary<string[], SourceFile> orderFiles)
    {
        int i = 0;

        foreach (var treeNode in tree)
            await SetOrderByParent(treeNode.Value, orderFiles, i++);
    }

    private async Task SetOrderByParent(NavNode node, Dictionary<string[], SourceFile> orderFiles, int index)
    {
        node.order = index;

        if (node.children != null)
        {
            int childIndex = 0;

            // recurse all children
            foreach (var treeNode in node.children)
                await SetOrderByParent(treeNode.Value, orderFiles, childIndex++);

            var path = node.location.Split('/');

            // if this particular folder has a .order, override the order
            var orderFile = orderFiles.SingleOrDefault(of => of.Key.SequenceEqual(path));

            if (orderFile.Value != null)
            {
                var lines = await System.IO.File.ReadAllLinesAsync(orderFile.Value.AbsolutePath);

                foreach (var item in node.children)
                {
                    int newIndex = Array.IndexOf(lines, item.Key);
                    item.Value.order = newIndex;

                    if (newIndex < 0)
                        _logger.LogDebug($"Hiding {item.Value.location}, according to `.order`");
                    else
                        _logger.LogDebug($"Moving {item.Value.location} to {newIndex}, according to `.order`");
                }
            }
        }
    }

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation(string location)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;
        var doc = await _indexingService.GetIndexedFile(location);

        return doc is not null ? Ok(doc) : NotFound();
    }
}