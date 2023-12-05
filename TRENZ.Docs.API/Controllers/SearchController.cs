using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SearchController : ControllerBase
{
    private static DateTime? _lastReindex;

    private readonly IIndexingService _indexingService;
    private readonly IConfiguration _configuration;
    private readonly IndexWorker _indexWorker;

    public SearchController(IIndexingService indexingService, IConfiguration configuration, IndexWorker indexWorker)
    {
        _indexingService = indexingService;
        _configuration = configuration;
        _indexWorker = indexWorker;
    }

    [HttpGet]
    public async Task<SearchResults> Query(string q, int? limit, int? offset)
    {
        return await _indexingService.Search(q, limit, offset);
    }

    [HttpGet]
    public async Task<IndexStats> Stats()
    {
        return await _indexingService.GetStats();
    }

    [HttpGet]
    public async Task<IActionResult> Reindex([FromQuery] string? key, CancellationToken cancellationToken = default)
    {
        if (_configuration["ReindexPassword"] != key)
            return Unauthorized();

#if !DEBUG
        if (DateTime.Now - _lastReindex < TimeSpan.FromSeconds(_configuration.GetValue<int>("ReindexThrottling")))
            return new StatusCodeResult(429);

        _lastReindex = DateTime.Now;
#endif

        await _indexWorker.DoReindex(cancellationToken);

        return Ok();
    }
}
