using TRENZ.Docs.API;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Services;
using Meilisearch;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddSingleton<MeilisearchClient>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();

    return new(configuration["Meilisearch:Url"], configuration["Meilisearch:ApiKey"]);
});

builder.Services.AddSingleton<IIndexingService, MeilisearchIndexingService>();
builder.Services.AddSingleton<ISourcesProvider, ConfigurationSourcesProvider>();
builder.Services.AddSingleton<IFileProcessingService, MarkdownFileProcessingService>();
builder.Services.AddSingleton<ITreeBuildingService, TreeBuildingService>();
builder.Services.AddSingleton<ITreeOrderService, TreeOrderService>();
builder.Services.AddSingleton<INodeFlaggingService, NodeFlaggingService>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithMethods("GET");
        policy.AllowAnyOrigin();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
