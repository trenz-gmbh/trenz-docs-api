using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IIndexingService _indexingService;
    private readonly ITreeOrderService _orderService;
    private readonly INodeFlaggingService _flaggingService;

    public DocumentsController(
        IIndexingService indexingService,
        ITreeOrderService orderService,
        INodeFlaggingService flaggingService
    )
    {
        _indexingService = indexingService;
        _orderService = orderService;
        _flaggingService = flaggingService;
    }

    public static string EncodeLocation(string location)
    {
        return location
            .Replace('-', ' ')
            .Replace("%2D", "-");
    }

    public static string DecodeLocation(string location)
    {
        return location
            .Replace("-", "%2D")
            .Replace(' ', '-');
    }

    [HttpGet]
    public async Task<Dictionary<string, NavNode>> NavTree()
    {
        var allDocs = await _indexingService.GetIndexedFiles();
        var tree = new Dictionary<string, NavNode>();

        foreach (var doc in allDocs)
        {
            var path = doc.location.Split(NavNode.Separator);
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
                        EncodeLocation(string.Join(NavNode.Separator, currentPath))
                    );
                }

                if (i + 1 < path.Length)
                {
                    node = node[part].Children ??= new();
                }
            }
        }

        await _orderService.ReorderTree(tree);
        await _flaggingService.UpdateHasContentFlag(tree);

        return tree;
    }

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation(string location)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;
        var doc = await _indexingService.GetIndexedFile(DecodeLocation(location));

        return doc is not null ? Ok(doc) : NotFound();
    }
}
