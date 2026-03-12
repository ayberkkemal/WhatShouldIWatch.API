using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhatShouldIWatch.Business.Algorithms;
using WhatShouldIWatch.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var baseDir = builder.Environment.ContentRootPath ?? AppContext.BaseDirectory;
var contentsPath = Path.Combine(baseDir, "Contents");
try
{
    if (!Directory.Exists(contentsPath))
    {
        var alt = Path.Combine(AppContext.BaseDirectory, "Contents");
        if (Directory.Exists(alt))
            contentsPath = alt;
        else
        {
            var dataContents = Path.Combine(baseDir, "..", "WhatShouldIWatch.Data", "Contents");
            if (Directory.Exists(dataContents))
                contentsPath = Path.GetFullPath(dataContents);
        }
    }
}
catch (UnauthorizedAccessException)
{
    contentsPath = Path.GetTempPath();
}

builder.Services.AddSingleton<IContentRepository>(new ExcelContentRepository(contentsPath));
builder.Services.AddSingleton<IKeyboardFuzzySearch, TurkishKeyboardFuzzySearch>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WhatShouldIWatch.Business.Suggestion.Requests.GetSuggestionsRequest).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Uygulama baslatilirken hata: {Message}", ex.Message);
    throw;
}
