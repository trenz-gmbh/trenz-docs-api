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
    
    public FileController(ISourcesProvider sourcesProvider)
    {
        _sourcesProvider = sourcesProvider;
    }

    [HttpGet("{**location}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60 * 60 * 24 * 7)]
    public async Task<IActionResult> Get(string location)
    {
        var normalizedLocation = Uri.UnescapeDataString(location).TrimStart(NavNode.Separator);
        var imageFile = _sourcesProvider
            .GetSources()
            .SelectMany(source => source.FindFiles(new(".*\\.(png|jpe?g|gif|json)$")))
            .FirstOrDefault(sf => sf.RelativePath == Path.GetRelativePath(".", normalizedLocation));

        if (imageFile == null)
            return NotFound();

        var typeProvider = new FileExtensionContentTypeProvider();
        if (!typeProvider.TryGetContentType(imageFile.RelativePath, out var contentType))
            contentType = "application/octet-stream";

        // MAYBE: restrict allowed content types?

        return File(await imageFile.GetBytesAsync(), contentType);
    }
}