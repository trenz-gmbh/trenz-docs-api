using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IIndexingService _indexingService;
    private readonly ITreeBuildingService _buildingService;
    private readonly ITreeOrderService _orderService;
    private readonly INodeFlaggingService _flaggingService;

    public DocumentsController(
        IIndexingService indexingService,
        ITreeBuildingService buildingService,
        ITreeOrderService orderService,
        INodeFlaggingService flaggingService
    )
    {
        _indexingService = indexingService;
        _buildingService = buildingService;
        _orderService = orderService;
        _flaggingService = flaggingService;
    }

    [HttpGet]
    public async Task<Dictionary<string, NavNode>> NavTree()
    {
        var indexFiles = (await _indexingService.GetIndexedFiles()).ToList();
        var tree = await _buildingService.BuildTreeAsync(indexFiles);

        await _orderService.ReorderTree(tree);
        await _flaggingService.UpdateHasContentFlag(tree, indexFiles);

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
