using Meilidown.Models;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;

namespace Meilidown.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly MeilisearchClient _client;

    public DocumentsController(MeilisearchClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<Dictionary<string, NavNode>> NavTree()
    {
        var allDocs = await _client.Index("files").GetDocumentsAsync<IndexedFile>(new()
        {
            Limit = 10000,
        });
        var tree = new Dictionary<string, NavNode>();

        foreach (var doc in allDocs)
        {
            var path = doc.location.Split('/');
            var currentPath = new List<string>();
            var node = tree;
            for (var i = 0; i < path.Length; i++)
            {
                var part = path[i];
                currentPath.Add(part);

                if (!node.ContainsKey(part))
                {
                    node[part] = new(
                        doc.uid,
                        part,
                        doc.order,
                        string.Join('/', currentPath)
                    );
                }

                if (i + 1 < path.Length)
                {
                    node = node[part].children ??= new();
                }
            }
        }

        return tree;
    }

    [HttpGet("{**location}")]
    public async Task<IActionResult> ByLocation(string location)
    {
        location = location.EndsWith(".md") ? location[..^3] : location;
        var result = await _client.Index("files").SearchAsync<IndexedFile>("", new()
        {
            Filter = $"location = \"{location}\"",
            Limit = 1,
        });

        var doc = result.Hits.FirstOrDefault();

        return doc is not null ? Ok(doc) : NotFound();
    }
}