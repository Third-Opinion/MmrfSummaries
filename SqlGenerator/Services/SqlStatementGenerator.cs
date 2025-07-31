using Microsoft.Extensions.Logging;
using SqlGenerator.Models;
using System.Text;

namespace SqlGenerator.Services;

public class SqlStatementGenerator
{
    private readonly ILogger<SqlStatementGenerator> _logger;

    public SqlStatementGenerator(ILogger<SqlStatementGenerator> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> GenerateUpdateStatements(IEnumerable<SummaryRecord> records)
    {
        _logger.LogInformation("Generating SQL UPDATE statements");
        
        var statements = new List<string>();
        var processedCount = 0;
        
        foreach (var record in records)
        {
            try
            {
                var statement = GenerateUpdateStatement(record);
                if (!string.IsNullOrEmpty(statement))
                {
                    statements.Add(statement);
                    processedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate SQL statement for NCT ID: {NctId}", record.NctId);
            }
        }
        
        _logger.LogInformation("Generated {Count} SQL UPDATE statements", processedCount);
        return statements;
    }
    
    private string GenerateUpdateStatement(SummaryRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.NctId))
        {
            _logger.LogWarning("Skipping record with empty NCT ID");
            return string.Empty;
        }
        
        var setParts = new List<string>();
        
        // Add long_summary as 'summary' column
        if (!string.IsNullOrWhiteSpace(record.LongSummary))
        {
            setParts.Add($"summary = {EscapeSqlString(record.LongSummary)}");
        }
        
        // Add short_summary column
        if (!string.IsNullOrWhiteSpace(record.ShortSummary))
        {
            setParts.Add($"short_summary = {EscapeSqlString(record.ShortSummary)}");
        }
        
        if (!setParts.Any())
        {
            _logger.LogWarning("Skipping record {NctId} - no summary data to update", record.NctId);
            return string.Empty;
        }
        
        var sql = new StringBuilder();
        sql.AppendLine($"UPDATE nlp_extensions");
        sql.AppendLine($"SET {string.Join(", ", setParts)}");
        sql.AppendLine($"WHERE nct_id = {EscapeSqlString(record.NctId)};");
        
        return sql.ToString();
    }
    
    private static string EscapeSqlString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "NULL";
        
        // Escape single quotes by doubling them
        var escaped = input.Replace("'", "''");
        
        // Handle other potential SQL injection characters
        // Note: This is a basic implementation. For production, consider using parameterized queries
        return $"'{escaped}'";
    }
    
    public string GenerateFileHeader(string sourceFile, int recordCount)
    {
        var header = new StringBuilder();
        header.AppendLine("-- ================================================================");
        header.AppendLine("-- SQL UPDATE Statements for nlp_extensions Database");
        header.AppendLine("-- ================================================================");
        header.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        header.AppendLine($"-- Source: {Path.GetFileName(sourceFile)}");
        header.AppendLine($"-- Records: {recordCount}");
        header.AppendLine("-- ================================================================");
        header.AppendLine();
        header.AppendLine("-- Execute these statements to update the nlp_extensions table");
        header.AppendLine("-- with AI-generated summaries from the clinical trial data.");
        header.AppendLine();
        
        return header.ToString();
    }
}