using Meilidown.API;
using Meilisearch;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

// Add services to the container.
builder.Services.AddSingleton<MeilisearchClient>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();

    return new(configuration["Meilisearch:Url"], configuration["Meilisearch:ApiKey"]);
});
builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();