using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhatShouldIWatch.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Stdout log (IIS 500 hatasında gerçek hatayı görmek için)
builder.Logging.AddConsole();

// Excel Contents klasörü: IIS/Kestrel için ContentRootPath veya BaseDirectory kullan
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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WhatShouldIWatch.Business.Suggestion.Requests.GetSuggestionsRequest).Assembly));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
