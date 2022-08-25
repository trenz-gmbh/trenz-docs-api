using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ISourcesProvider _sourcesProvider;
    private readonly INavTreeProvider _navTreeProvider;
    private readonly IAuthAdapter? _authAdapter;

    public FileController(
        ISourcesProvider sourcesProvider,
        INavTreeProvider navTreeProvider,
        IAuthAdapter? authAdapter = null
    )
    {
        _sourcesProvider = sourcesProvider;
        _navTreeProvider = navTreeProvider;
        _authAdapter = authAdapter;
    }

    [HttpGet("{**location}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60 * 60 * 24 * 7)]
    public async Task<IActionResult> Get(string location)
    {
        IEnumerable<string> claims = Array.Empty<string>();
        if (_authAdapter != null)
            claims = await _authAdapter.GetClaimsAsync(HttpContext) ?? Array.Empty<string>();

        var effectiveNavTree = _navTreeProvider.Tree.FilterByGroups(claims);

        var normalizedLocation = Uri.UnescapeDataString(location).TrimStart(NavNode.Separator);
        var file = _sourcesProvider
            .GetSources()
            .SelectMany(source => source.FindFiles(new(".*\\.(png|jpe?g|gif|json)$")))
            .FirstOrDefault(sf => sf.RelativePath == Path.GetRelativePath(".", normalizedLocation));

        if (file == null)
            return NotFound();

        var parentNodeLocation = file.RelativePath.Split(Path.DirectorySeparatorChar).SkipLast(1).ToArray();
        if (parentNodeLocation.Length > 0) // length == 0 if file is in root
        {
            var parentNode = effectiveNavTree.FindNodeByLocationParts(parentNodeLocation);
            if (parentNode == null)
                return NotFound();
        }

        var typeProvider = new FileExtensionContentTypeProvider();
        if (!typeProvider.TryGetContentType(file.RelativePath, out var contentType))
            contentType = "application/octet-stream";

        // MAYBE: restrict allowed content types?

        return File(await file.GetBytesAsync(), contentType);
    }
}