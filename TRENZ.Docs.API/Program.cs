using Meilisearch;
using TRENZ.Docs.API;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Services;
using TRENZ.Docs.API.Services.Auth;

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
builder.Services.AddSingleton<INavTreeProvider, NavTreeProvider>();
builder.Services.AddSingleton<INavNodeOrderingService, NavNodeOrderingService>();
builder.Services.AddSingleton<INavNodeFlaggingService, NavNodeFlaggingService>();
builder.Services.AddSingleton<INavNodeAuthorizationService, NavNodeAuthorizationService>();

if (builder.Configuration.GetSection("Auth") != null)
{
    builder.Services.AddAuthAdapter();
}

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

        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .GetChildren()
            .Select(c => c.Value)
            .ToArray();

        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.AllowCredentials();
            policy.WithOrigins(allowedOrigins);
        }
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
