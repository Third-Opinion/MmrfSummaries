using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using ClinicalTrialsDataFetcher.Models;

namespace ClinicalTrialsDataFetcher.Services;

public class CsvExportService
{
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(ILogger<CsvExportService> logger)
    {
        _logger = logger;
    }

    public async Task ExportToCsvAsync(List<ClinicalTrialData> trials, string filePath)
    {
        try
        {
            _logger.LogInformation("Exporting {Count} trials to CSV: {FilePath}", trials.Count, filePath);

            await using var writer = new StringWriter();
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.Context.RegisterClassMap<ClinicalTrialDataMap>();
            
            await csv.WriteRecordsAsync(trials);
            
            await File.WriteAllTextAsync(filePath, writer.ToString());
            
            _logger.LogInformation("Successfully exported CSV file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export CSV file: {FilePath}", filePath);
            throw;
        }
    }
}

public class ClinicalTrialDataMap : ClassMap<ClinicalTrialData>
{
    public ClinicalTrialDataMap()
    {
        Map(m => m.NctId).Name("nct_id");
        Map(m => m.BriefTitle).Name("brief_title");
        Map(m => m.BriefSummary).Name("brief_summary");
        Map(m => m.Conditions).Name("conditions");
        Map(m => m.Interventions).Name("interventions");
        Map(m => m.Age).Name("age");
        Map(m => m.Genders).Name("genders");
    }
}