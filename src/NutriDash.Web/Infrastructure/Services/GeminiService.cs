using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;

namespace NutriDash.Infrastructure.Services;

public class GeminiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GeminiService> _logger;
    private readonly IConfiguration _config;

    public GeminiService(IHttpClientFactory httpFactory, IServiceScopeFactory scopeFactory,
        ILogger<GeminiService> logger, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    public async Task<string?> GetApiKeyAsync()
    {
        // Priority: env var > db setting > appsettings.json
        var envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey)) return envKey;

        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var setting = await ctx.AppSettings.FirstOrDefaultAsync(s => s.Key == "GeminiApiKey");
        if (!string.IsNullOrWhiteSpace(setting?.Value)) return setting.Value;

        return _config["Gemini:ApiKey"];
    }

    public async Task<string?> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var apiKey = await GetApiKeyAsync();
        var model = "gemini-3.1-pro-preview"; // El modelo que elegiste
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
        int maxRetries = int.TryParse(_config["Gemini:MaxRetries"], out var r) ? r : 3;

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.5, responseMimeType = "application/json" }
        };

        var json = JsonSerializer.Serialize(requestBody);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var client = _httpFactory.CreateClient("Gemini");
                var url = $"{baseUrl}?key={apiKey}";
                var response = await client.PostAsync(url,
                    new StringContent(json, Encoding.UTF8, "application/json"), ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning("Gemini attempt {A}: HTTP {S} — {E}", attempt, response.StatusCode, err);
                    if (attempt == maxRetries) throw new HttpRequestException($"Gemini API error: {response.StatusCode} — {err}");
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                    continue;
                }

                var responseBody = await response.Content.ReadAsStringAsync(ct);
                var node = JsonNode.Parse(responseBody);
                var text = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();
                return text;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Gemini attempt {A} failed, retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
            }
        }
        return null;
    }
}
