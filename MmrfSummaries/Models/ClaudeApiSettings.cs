namespace MmrfSummaries.Models;

public class ClaudeApiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public string ApiVersion { get; set; } = "2023-06-01";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
}