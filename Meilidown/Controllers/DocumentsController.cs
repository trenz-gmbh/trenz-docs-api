using Meilidown.Interfaces;
using Meilidown.Models.Index;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IIndexingService _indexingService;
    private readonly ILogger<DocumentsController> _logger;
    private readonly ITreeOrderService _orderService;

    public DocumentsController(IIndexingService indexingService,
                               ITreeOrderService orderService,
                               ILogger<DocumentsController> logger)
    {
        _indexingService = indexingService;
        _orderService = orderService;
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

        await _orderService.ReorderTree(tree);

        return tree;
    }

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation(string location)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;
        var doc = await _indexingService.GetIndexedFile(location);

        return doc is not null ? Ok(doc) : NotFound();
    }
}
