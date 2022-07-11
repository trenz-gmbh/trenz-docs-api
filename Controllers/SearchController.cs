using Meilidown.Interfaces;
using Meilidown.Models.Index;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IIndexingService _indexingService;

    public SearchController(IIndexingService indexingService)
    {
        _indexingService = indexingService;
    }

    [HttpGet]
    public async Task<IEnumerable<SearchResult>> Query([FromQuery] string q)
    {
        return await _indexingService.Search(q);
    }
}