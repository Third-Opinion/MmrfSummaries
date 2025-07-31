namespace MmrfSummaries.Models;

public class TrialRecord
{
    public string NctId { get; set; } = string.Empty;
    public string BriefTitle { get; set; } = string.Empty;
    public string BriefSummary { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;
    public string Interventions { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Genders { get; set; } = string.Empty;
    public string ShortSummary { get; set; } = string.Empty;
    public string LongSummary { get; set; } = string.Empty;
}