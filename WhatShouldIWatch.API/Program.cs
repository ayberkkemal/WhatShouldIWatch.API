using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using WhatShouldIWatch.Business.Algorithms;
using WhatShouldIWatch.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection gerekli.");

var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddSingleton<IContentRepository, PgContentRepository>();
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
