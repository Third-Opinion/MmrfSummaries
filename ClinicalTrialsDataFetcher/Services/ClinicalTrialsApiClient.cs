using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using ClinicalTrialsDataFetcher.Models;

namespace ClinicalTrialsDataFetcher.Services;

public class ClinicalTrialsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClinicalTrialsApiClient> _logger;
    private const string BaseUrl = "https://clinicaltrials.gov/api/v2/studies";

    public ClinicalTrialsApiClient(ILogger<ClinicalTrialsApiClient> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<List<ClinicalTrialData>> GetTrialsAsync(List<string> nctIds)
    {
        var trials = new List<ClinicalTrialData>();

        foreach (var nctId in nctIds)
        {
            try
            {
                _logger.LogInformation("Fetching data for NCT ID: {NctId}", nctId);
                var trial = await GetSingleTrialAsync(nctId);
                if (trial != null)
                {
                    trials.Add(trial);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch data for NCT ID: {NctId}", nctId);
            }
        }

        return trials;
    }

    private async Task<ClinicalTrialData?> GetSingleTrialAsync(string nctId)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["query.cond"] = "";
        queryParams["query.term"] = "";
        queryParams["query.locn"] = "";
        queryParams["filter.ids"] = nctId;
        queryParams["format"] = "json";
        queryParams["fields"] = "NCTId,BriefTitle,BriefSummary,Condition,InterventionName,InterventionType,MinimumAge,MaximumAge,Sex";

        var url = $"{BaseUrl}?{queryParams}";
        
        _logger.LogDebug("API Request URL: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API request failed with status: {StatusCode}", response.StatusCode);
            return null;
        }

        var jsonContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("API Response: {Response}", jsonContent);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var apiResponse = JsonSerializer.Deserialize<ClinicalTrialsApiResponse>(jsonContent, options);
        
        if (apiResponse?.Studies == null || !apiResponse.Studies.Any())
        {
            _logger.LogWarning("No study data found for NCT ID: {NctId}", nctId);
            return null;
        }

        var study = apiResponse.Studies.First();
        return MapStudyToTrialData(study);
    }

    private ClinicalTrialData MapStudyToTrialData(Study study)
    {
        var protocol = study.ProtocolSection;
        if (protocol == null) return new ClinicalTrialData();

        var trial = new ClinicalTrialData
        {
            NctId = protocol.IdentificationModule?.NctId ?? string.Empty,
            BriefTitle = protocol.IdentificationModule?.BriefTitle ?? string.Empty,
            BriefSummary = protocol.DescriptionModule?.BriefSummary ?? string.Empty
        };

        // Map conditions
        if (protocol.ConditionsModule?.Conditions != null)
        {
            trial.Conditions = string.Join("; ", protocol.ConditionsModule.Conditions);
        }

        // Map interventions
        if (protocol.ArmsInterventionsModule?.Interventions != null)
        {
            var interventionStrings = protocol.ArmsInterventionsModule.Interventions
                .Select(i => $"{i.Type}: {i.Name}")
                .Where(s => !string.IsNullOrWhiteSpace(s));
            trial.Interventions = string.Join("; ", interventionStrings);
        }

        // Map age eligibility
        if (protocol.EligibilityModule != null)
        {
            var minAge = protocol.EligibilityModule.MinimumAge ?? "N/A";
            var maxAge = protocol.EligibilityModule.MaximumAge ?? "N/A";
            trial.Age = $"{minAge} to {maxAge}";
            
            trial.Genders = protocol.EligibilityModule.Sex ?? "All";
        }

        return trial;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}