using Meilidown.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public ImageController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("{location}")]
    public IActionResult Png(string location)
    {
        var path = Uri.UnescapeDataString(location);
        var imageFile = RepositoryRepository
            .GetRepositories(_configuration)
            .SelectMany(r => r.FindFiles("**.png"))
            .FirstOrDefault(f => Path.GetFullPath(Path.Combine(f.Config.Root, f.Config.Path, path)) == f.AbsolutePath);

        if (imageFile == null)
            return NotFound();
        
        return PhysicalFile(imageFile.AbsolutePath, "image/png");
    }
}