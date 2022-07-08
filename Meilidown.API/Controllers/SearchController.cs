using Meilisearch;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly MeilisearchClient _client;

    public SearchController(MeilisearchClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IEnumerable<IndexedFile>> Query([FromQuery] string q)
    {
        var result = await _client.Index("files").SearchAsync<IndexedFile>(q);

        return result.Hits;
    }
}