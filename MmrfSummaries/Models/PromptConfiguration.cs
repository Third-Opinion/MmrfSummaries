namespace MmrfSummaries.Models;

public class PromptConfiguration
{
    public string Prompt { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public decimal Temperature { get; set; }
}