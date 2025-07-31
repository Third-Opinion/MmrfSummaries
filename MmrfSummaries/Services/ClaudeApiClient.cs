using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MmrfSummaries.Models;

namespace MmrfSummaries.Services;

public class ClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeApiSettings _settings;
    private readonly ILogger<ClaudeApiClient> _logger;

    public ClaudeApiClient(ClaudeApiSettings settings, ILogger<ClaudeApiClient> logger)
    {
        _settings = settings;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _settings.ApiVersion);
    }

    public async Task<string> GenerateSummaryAsync(string prompt, int maxTokens, decimal temperature)
    {
        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = maxTokens,
            temperature = temperature,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("=== CLAUDE API REQUEST ===");
        _logger.LogInformation("URL: {Url}", $"{_settings.BaseUrl}/v1/messages");
        _logger.LogInformation("Request Body:\n{RequestBody}", json);

        var maxRetries = 3;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Sending request to Claude API (attempt {Attempt})", attempt);
                
                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/v1/messages", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogInformation("=== CLAUDE API RESPONSE ===");
                    _logger.LogInformation("Status: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    _logger.LogInformation("Response Body:\n{ResponseBody}", responseContent);
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    };

                    _logger.LogDebug("Attempting to deserialize response...");
                    var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, options);
                    _logger.LogDebug("Deserialization completed. Content array length: {Length}", claudeResponse?.Content?.Length ?? -1);
                    
                    if (claudeResponse?.Content != null && claudeResponse.Content.Length > 0)
                    {
                        var resultText = claudeResponse.Content[0].Text ?? string.Empty;
                        _logger.LogInformation("=== EXTRACTED SUMMARY ===");
                        _logger.LogInformation("Summary Text:\n{SummaryText}", resultText);
                        
                        if (claudeResponse.Usage != null)
                        {
                            _logger.LogInformation("Token Usage - Input: {InputTokens}, Output: {OutputTokens}", 
                                claudeResponse.Usage.InputTokens, claudeResponse.Usage.OutputTokens);
                        }
                        
                        _logger.LogDebug("Successfully received response from Claude API");
                        return resultText;
                    }
                    
                    _logger.LogWarning("Received empty response from Claude API");
                    return string.Empty;
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    _logger.LogWarning("Rate limited by Claude API. Waiting {Delay}ms before retry", delay.TotalMilliseconds);
                    await Task.Delay(delay);
                    continue;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("=== CLAUDE API ERROR RESPONSE ===");
                _logger.LogError("Status: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                _logger.LogError("Error Response Body:\n{ErrorBody}", errorContent);
                _logger.LogError("Claude API request failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                
                if (attempt == maxRetries)
                {
                    throw new HttpRequestException($"Claude API request failed after {maxRetries} attempts. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                _logger.LogWarning("Network error occurred. Retrying in {Delay}ms", delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
        }

        throw new HttpRequestException($"Failed to get response from Claude API after {maxRetries} attempts");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class ClaudeResponse
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Role { get; set; }
    public string? Model { get; set; }
    public ClaudeContent[]? Content { get; set; }
    public string? StopReason { get; set; }
    public ClaudeUsage? Usage { get; set; }
}

public class ClaudeContent
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

public class ClaudeUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}