using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SqlGenerator.Models;

namespace SqlGenerator.Services;

public class CsvReaderService
{
    private readonly ILogger<CsvReaderService> _logger;

    public CsvReaderService(ILogger<CsvReaderService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<SummaryRecord>> ReadAsync(string filePath)
    {
        _logger.LogInformation("Reading CSV file: {FilePath}", filePath);
        
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            // Configure CSV reader to handle missing columns gracefully
            csv.Context.RegisterClassMap<SummaryRecordMap>();
            
            var records = new List<SummaryRecord>();
            await foreach (var record in csv.GetRecordsAsync<SummaryRecord>())
            {
                // Only process records that have the required fields for SQL generation
                if (!string.IsNullOrWhiteSpace(record.NctId) && 
                    (!string.IsNullOrWhiteSpace(record.ShortSummary) || !string.IsNullOrWhiteSpace(record.LongSummary)))
                {
                    records.Add(record);
                }
                else
                {
                    _logger.LogWarning("Skipping record with missing NctId or empty summaries: {NctId}", record.NctId);
                }
            }
            
            _logger.LogInformation("Successfully read {Count} valid records from CSV", records.Count);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read CSV file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to read CSV file: {ex.Message}", ex);
        }
    }
    
    public async Task ValidateAsync(string filePath)
    {
        _logger.LogDebug("Validating CSV file structure: {FilePath}", filePath);
        
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            await csv.ReadAsync();
            csv.ReadHeader();
            
            var headers = csv.HeaderRecord;
            if (headers == null)
            {
                throw new InvalidOperationException("CSV file has no header row");
            }
            
            var requiredColumns = new[] { "nct_id" };
            var summaryColumns = new[] { "short_summary", "long_summary" };
            
            var missingRequired = requiredColumns.Where(col => 
                !headers.Any(h => string.Equals(h, col, StringComparison.OrdinalIgnoreCase))).ToList();
            
            if (missingRequired.Any())
            {
                throw new InvalidOperationException($"CSV file is missing required columns: {string.Join(", ", missingRequired)}");
            }
            
            var hasSummaryColumns = summaryColumns.Any(col => 
                headers.Any(h => string.Equals(h, col, StringComparison.OrdinalIgnoreCase)));
            
            if (!hasSummaryColumns)
            {
                throw new InvalidOperationException($"CSV file must have at least one summary column: {string.Join(" or ", summaryColumns)}");
            }
            
            _logger.LogDebug("CSV file validation successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV file validation failed: {FilePath}", filePath);
            throw;
        }
    }
}

public class SummaryRecordMap : ClassMap<SummaryRecord>
{
    public SummaryRecordMap()
    {
        Map(m => m.NctId).Name("nct_id");
        Map(m => m.BriefTitle).Name("brief_title").Optional();
        Map(m => m.BriefSummary).Name("brief_summary").Optional();
        Map(m => m.Conditions).Name("conditions").Optional();
        Map(m => m.Interventions).Name("interventions").Optional();
        Map(m => m.Age).Name("age").Optional();
        Map(m => m.Genders).Name("genders").Optional();
        Map(m => m.ShortSummary).Name("short_summary").Optional();
        Map(m => m.LongSummary).Name("long_summary").Optional();
    }
}