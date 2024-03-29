﻿using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IIndexingService _indexingService;
    private readonly INavTreeProvider _navTreeProvider;
    private readonly IAuthAdapter? _authAdapter;

    public DocumentsController(
        IIndexingService indexingService,
        INavTreeProvider navTreeProvider,
        IAuthAdapter? authAdapter = null
    )
    {
        _indexingService = indexingService;
        _navTreeProvider = navTreeProvider;
        _authAdapter = authAdapter;
    }

    private async Task<NavTree> GetFilteredTree(CancellationToken cancellationToken)
    {
        IEnumerable<string> claims = Array.Empty<string>();
        if (_authAdapter != null)
            claims = await _authAdapter.GetClaimsAsync(HttpContext, cancellationToken) ?? Array.Empty<string>();

        return _navTreeProvider.Tree
            .WithoutHiddenNodes()
            .FilterByGroups(claims)
            .WithoutChildlessContentlessNodes();
    }

    [HttpGet]
    public async Task<NavTree> NavTree(CancellationToken cancellationToken) => await GetFilteredTree(cancellationToken);

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation([FromRoute] string location, CancellationToken cancellationToken)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;

        var tree = await GetFilteredTree(cancellationToken);
        var node = tree.FindNodeByLocation(location);
        if (node is not { HasContent: true })
            return NotFound();

        var doc = await _indexingService.GetIndexedFile(location, cancellationToken);

        return doc is null ? NotFound() : Ok(doc);
    }
}