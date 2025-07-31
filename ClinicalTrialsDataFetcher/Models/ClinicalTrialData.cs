namespace ClinicalTrialsDataFetcher.Models;

public class ClinicalTrialData
{
    public string NctId { get; set; } = string.Empty;
    public string BriefTitle { get; set; } = string.Empty;
    public string BriefSummary { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;
    public string Interventions { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Genders { get; set; } = string.Empty;
}

// API Response Models
public class ClinicalTrialsApiResponse
{
    public List<Study>? Studies { get; set; }
}

public class Study
{
    public ProtocolSection? ProtocolSection { get; set; }
}

public class ProtocolSection
{
    public IdentificationModule? IdentificationModule { get; set; }
    public DescriptionModule? DescriptionModule { get; set; }
    public ConditionsModule? ConditionsModule { get; set; }
    public ArmsInterventionsModule? ArmsInterventionsModule { get; set; }
    public EligibilityModule? EligibilityModule { get; set; }
}

public class IdentificationModule
{
    public string? NctId { get; set; }
    public string? BriefTitle { get; set; }
}

public class DescriptionModule
{
    public string? BriefSummary { get; set; }
}

public class ConditionsModule
{
    public List<string>? Conditions { get; set; }
}

public class ArmsInterventionsModule
{
    public List<Intervention>? Interventions { get; set; }
}

public class Intervention
{
    public string? Type { get; set; }
    public string? Name { get; set; }
}

public class EligibilityModule
{
    public string? MinimumAge { get; set; }
    public string? MaximumAge { get; set; }
    public string? Sex { get; set; }
}