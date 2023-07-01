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
    private readonly Worker _worker;

    public SearchController(IIndexingService indexingService, IConfiguration configuration, Worker worker)
    {
        _indexingService = indexingService;
        _configuration = configuration;
        _worker = worker;
    }

    [HttpGet]
    public async Task<IEnumerable<SearchResult>> Query([FromQuery] string q)
    {
        return await _indexingService.Search(q);
    }

    [HttpGet]
    public async Task<IndexStats> Stats()
    {
        return await _indexingService.GetStats();
    }

    [HttpGet]
    public async Task Reindex([FromQuery] string? key, CancellationToken cancellationToken = default)
    {
        if (_configuration["ReindexPassword"] != key)
            return;

#if !DEBUG
        if (DateTime.Now - _lastReindex < TimeSpan.FromMinutes(30))
            return;

        _lastReindex = DateTime.Now;
#endif

        await _worker.DoReindex(cancellationToken);
    }
}
