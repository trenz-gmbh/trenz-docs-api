using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SearchController : ControllerBase
{
    private readonly IIndexingService _indexingService;

    public SearchController(IIndexingService indexingService)
    {
        _indexingService = indexingService;
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
}