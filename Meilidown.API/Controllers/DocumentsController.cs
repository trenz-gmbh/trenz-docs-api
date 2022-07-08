using Meilidown.API.Models;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly MeilisearchClient _client;

    public DocumentsController(MeilisearchClient client)
    {
        _client = client;
    }

    // TODO: replace All() with GetNavTree()
    [HttpGet]
    public async Task<IEnumerable<IndexedFile>> All()
    {
        return await _client.Index("files").GetDocumentsAsync<IndexedFile>(new()
        {
            Limit = 10000,
        });
    }

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation(string location)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;
        var result = await _client.Index("files").SearchAsync<IndexedFile>("", new()
        {
            Filter = $"location = \"{location}\"",
            Limit = 1,
        });

        var doc = result.Hits.FirstOrDefault();

        return doc is not null ? Ok(doc) : NotFound();
    }
}