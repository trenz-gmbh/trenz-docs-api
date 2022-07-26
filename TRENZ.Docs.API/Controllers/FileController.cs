using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Index;

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
    public IActionResult Get(string location)
    {
        var path = Uri.UnescapeDataString(location).TrimStart(NavNode.Separator);
        var imageFile = _sourcesProvider
            .GetSources()
            .SelectMany(source => source.FindFiles(new(".*\\.(png|jpe?g|gif|json)$")))
            .FirstOrDefault(f => Path.GetFullPath(Path.Combine(f.Source.Root, f.Source.Path, path)) == f.AbsolutePath);

        if (imageFile == null)
            return NotFound();

        return PhysicalFile(imageFile.AbsolutePath, Path.GetExtension(imageFile.AbsolutePath) switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".json" => "application/json",
            _ => "application/octet-stream",
        });
    }
}