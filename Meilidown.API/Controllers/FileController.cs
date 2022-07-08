using Meilidown.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.API.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public FileController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("{location}")]
    public IActionResult Get(string location)
    {
        var path = Uri.UnescapeDataString(location).TrimStart('/');
        var imageFile = RepositoryRepository
            .GetRepositories(_configuration)
            .SelectMany(r => r.FindFiles(new(".*\\.(png|jpe?g|gif|json)$")))
            .FirstOrDefault(f => Path.GetFullPath(Path.Combine(f.Config.Root, f.Config.Path, path)) == f.AbsolutePath);

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