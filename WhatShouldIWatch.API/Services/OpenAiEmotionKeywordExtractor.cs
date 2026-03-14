using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhatShouldIWatch.Business.Suggestion.Services;

namespace WhatShouldIWatch.API.Services;

public class OpenAiEmotionKeywordExtractor : IEmotionKeywordExtractor
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiEmotionKeywordExtractor> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenAiEmotionKeywordExtractor(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiEmotionKeywordExtractor> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> ExtractKeywordsAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("OpenAI ApiKey yapılandırılmamış; anahtar kelime çıkarımı atlanıyor.");
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(userText))
            return Array.Empty<string>();

        var baseUrl = _options.BaseUrl?.TrimEnd('/') ?? "https://api.openai.com";
        var url = $"{baseUrl}/v1/chat/completions";

        var requestBody = new
        {
            model = _options.Model,
            max_tokens = 300,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = GetSystemPrompt()
                },
                new
                {
                    role = "user",
                    content = userText.Trim()
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", "Bearer " + _options.ApiKey!.Trim());
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API çağrısı başarısız.");
            return Array.Empty<string>();
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI API hata: {StatusCode} - {Content}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            return Array.Empty<string>();
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var keywords = ParseKeywordsFromResponse(json);
        _logger.LogDebug("AI'dan {Count} anahtar kelime çıkarıldı: {Keywords}", keywords.Count, string.Join(", ", keywords));
        return keywords;
    }

    private static string GetSystemPrompt()
    {
        return """
            Sen bir duygu ve içerik analiz asistanısın. Kullanıcının yazdığı cümleyi (Türkçe) analiz edip, o anki duygu durumuna ve ihtiyacına uygun içerik önermek için kullanılabilecek anahtar kelimeler çıkar.

            Kurallar:
            - Sadece Türkçe anahtar kelimeler üret (duygu: üzüntü, öfke, mutluluk, rahatlama, stres; hedef: aile, arkadaş, yalnız; tür: komedi, drama, romantik vb.).
            - Cümledeki duyguyu (kavga → öfke/üzüntü, karnım aç → rahatlama/komedi isteği vb.) ve izleyecek içerik türünü düşün.
            - Yanıtı SADECE geçerli bir JSON array olarak ver, başka metin yazma. Örnek: ["üzgün", "aile", "komedi", "rahatlama"]
            - En fazla 10-12 kelime döndür.
            """;
    }

    private static IReadOnlyList<string> ParseKeywordsFromResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                return Array.Empty<string>();

            var first = choices[0];
            if (!first.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var contentEl))
                return Array.Empty<string>();

            var content = contentEl.GetString();
            if (string.IsNullOrWhiteSpace(content))
                return Array.Empty<string>();

            content = content.Trim();
            var list = JsonSerializer.Deserialize<List<string>>(content, JsonOptions);
            if (list == null) return Array.Empty<string>();
            return list
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
