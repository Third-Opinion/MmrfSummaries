using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using MmrfSummaries.Models;

namespace MmrfSummaries.Services;

public class CsvProcessor
{
    private readonly ILogger<CsvProcessor> _logger;

    public CsvProcessor(ILogger<CsvProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<List<TrialRecord>> ReadTrialsAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Reading CSV file: {FilePath}", filePath);
            
            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            csv.Context.RegisterClassMap<TrialRecordInputMap>();
            
            var records = csv.GetRecords<TrialRecord>().ToList();
            
            _logger.LogInformation("Successfully read {Count} trial records", records.Count);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read CSV file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<TrialRecord>?> ReadExistingOutputAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogInformation("Output file does not exist: {FilePath}", filePath);
            return null;
        }

        try
        {
            _logger.LogInformation("Reading existing output file: {FilePath}", filePath);
            
            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            csv.Context.RegisterClassMap<TrialRecordMap>();
            
            var records = csv.GetRecords<TrialRecord>().ToList();
            
            _logger.LogInformation("Successfully read {Count} existing trial records", records.Count);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read existing output file: {FilePath}. Will process from scratch.", filePath);
            return null;
        }
    }

    public async Task WriteTrialsAsync(string filePath, List<TrialRecord> trials)
    {
        try
        {
            _logger.LogInformation("Writing {Count} trial records to: {FilePath}", trials.Count, filePath);
            
            await using var writer = new StringWriter();
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.Context.RegisterClassMap<TrialRecordMap>();
            
            await csv.WriteRecordsAsync(trials);
            await File.WriteAllTextAsync(filePath, writer.ToString());
            
            _logger.LogInformation("Successfully wrote CSV file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write CSV file: {FilePath}", filePath);
            throw;
        }
    }
}

public class TrialRecordInputMap : ClassMap<TrialRecord>
{
    public TrialRecordInputMap()
    {
        Map(m => m.NctId).Name("nct_id");
        Map(m => m.BriefTitle).Name("brief_title");
        Map(m => m.BriefSummary).Name("brief_summary");
        Map(m => m.Conditions).Name("conditions");
        Map(m => m.Interventions).Name("interventions");
        Map(m => m.Age).Name("age");
        Map(m => m.Genders).Name("genders");
        // Summary columns are optional for input files
        Map(m => m.ShortSummary).Name("short_summary").Optional();
        Map(m => m.LongSummary).Name("long_summary").Optional();
    }
}

public class TrialRecordMap : ClassMap<TrialRecord>
{
    public TrialRecordMap()
    {
        Map(m => m.NctId).Name("nct_id");
        Map(m => m.BriefTitle).Name("brief_title");
        Map(m => m.BriefSummary).Name("brief_summary");
        Map(m => m.Conditions).Name("conditions");
        Map(m => m.Interventions).Name("interventions");
        Map(m => m.Age).Name("age");
        Map(m => m.Genders).Name("genders");
        Map(m => m.ShortSummary).Name("short_summary");
        Map(m => m.LongSummary).Name("long_summary");
    }
}