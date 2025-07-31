namespace MmrfSummaries.Models;

public class SummarySettings
{
    public PromptConfiguration ShortSummary { get; set; } = new();
    public PromptConfiguration LongSummary { get; set; } = new();
}