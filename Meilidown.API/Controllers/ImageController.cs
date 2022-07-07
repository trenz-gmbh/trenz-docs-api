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
            .SelectMany(r => r.FindFiles(path))
            .FirstOrDefault(f => f.RelativePath.Replace('\\', '/') == path);

        if (imageFile == null)
            return NotFound();
        
        return PhysicalFile(imageFile.AbsolutePath, "image/png");
    }
}