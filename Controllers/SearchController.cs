using Meilidown.Models;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.Controllers;

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
    public async Task<IEnumerable<SearchResult>> Query([FromQuery] string q)
    {
        var result = await _client.Index("files").SearchAsync<SearchResult>(q, new()
        {
            AttributesToHighlight = new[] { "name", "location", "content" },
            HighlightPreTag = "<mark>",
            HighlightPostTag = "</mark>",
            AttributesToCrop = new[] { "content" },
            CropLength = 25,
            AttributesToRetrieve = new[] { "name", "location", "content" },
        });

        return result.Hits;
    }
}