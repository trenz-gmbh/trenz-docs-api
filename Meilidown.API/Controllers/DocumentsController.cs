using Meilisearch;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly MeilisearchClient _client;

    public DocumentsController(MeilisearchClient client)
    {
        _client = client;
    }

    [HttpGet("[action]")]
    public async Task<IEnumerable<IndexedFile>> All()
    {
        return await _client.Index("files").GetDocumentsAsync<IndexedFile>();
    }
}