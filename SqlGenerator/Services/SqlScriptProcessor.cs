using Microsoft.Extensions.Logging;
using SqlGenerator.Models;
using System.Diagnostics;

namespace SqlGenerator.Services;

public class SqlScriptProcessor
{
    private readonly CsvReaderService _csvReader;
    private readonly SqlStatementGenerator _sqlGenerator;
    private readonly SqlFileWriter _sqlWriter;
    private readonly ILogger<SqlScriptProcessor> _logger;

    public SqlScriptProcessor(
        CsvReaderService csvReader,
        SqlStatementGenerator sqlGenerator,
        SqlFileWriter sqlWriter,
        ILogger<SqlScriptProcessor> logger)
    {
        _csvReader = csvReader;
        _sqlGenerator = sqlGenerator;
        _sqlWriter = sqlWriter;
        _logger = logger;
    }

    public async Task ProcessAsync(string inputFile, string outputFile, int batchSize = 1000)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting SQL script processing");
        _logger.LogInformation("Input: {InputFile}", inputFile);
        _logger.LogInformation("Output: {OutputFile}", outputFile);
        _logger.LogInformation("Batch size: {BatchSize}", batchSize);
        
        try
        {
            // Step 1: Validate CSV file
            _logger.LogInformation("Step 1: Validating CSV file structure");
            await _csvReader.ValidateAsync(inputFile);
            
            // Step 2: Read CSV data
            _logger.LogInformation("Step 2: Reading CSV data");
            var records = await _csvReader.ReadAsync(inputFile);
            var recordsList = records.ToList();
            
            if (!recordsList.Any())
            {
                _logger.LogWarning("No valid records found in CSV file");
                return;
            }
            
            // Step 3: Generate SQL statements
            _logger.LogInformation("Step 3: Generating SQL statements for {Count} records", recordsList.Count);
            var sqlStatements = _sqlGenerator.GenerateUpdateStatements(recordsList);
            var statementsList = sqlStatements.ToList();
            
            if (!statementsList.Any())
            {
                _logger.LogWarning("No SQL statements generated");
                return;
            }
            
            // Step 4: Generate file header
            _logger.LogInformation("Step 4: Generating file header");
            var header = _sqlGenerator.GenerateFileHeader(inputFile, statementsList.Count);
            
            // Step 5: Write SQL file
            _logger.LogInformation("Step 5: Writing SQL file");
            if (statementsList.Count > batchSize)
            {
                await _sqlWriter.WriteBatchAsync(outputFile, header, ToAsyncEnumerable(statementsList), batchSize);
            }
            else
            {
                await _sqlWriter.WriteAsync(outputFile, header, statementsList);
            }
            
            stopwatch.Stop();
            
            _logger.LogInformation("Processing completed successfully");
            _logger.LogInformation("Total processing time: {ElapsedTime:F2} seconds", stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Records processed: {RecordCount}", recordsList.Count);
            _logger.LogInformation("SQL statements generated: {StatementCount}", statementsList.Count);
            _logger.LogInformation("Average time per record: {AvgTime:F3} seconds", 
                stopwatch.Elapsed.TotalSeconds / recordsList.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Processing failed after {ElapsedTime:F2} seconds", stopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }
    
    public async Task ProcessLargeFileAsync(string inputFile, string outputFile, int batchSize = 1000)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting large file SQL script processing with streaming");
        _logger.LogInformation("Input: {InputFile}", inputFile);
        _logger.LogInformation("Output: {OutputFile}", outputFile);
        _logger.LogInformation("Batch size: {BatchSize}", batchSize);
        
        try
        {
            // Step 1: Validate CSV file
            await _csvReader.ValidateAsync(inputFile);
            
            // Step 2: Process in streaming mode for memory efficiency
            var header = _sqlGenerator.GenerateFileHeader(inputFile, 0); // Record count will be updated later
            
            var processedRecords = await ProcessInBatchesAsync(inputFile, outputFile, header, batchSize);
            
            stopwatch.Stop();
            
            _logger.LogInformation("Large file processing completed successfully");
            _logger.LogInformation("Total processing time: {ElapsedTime:F2} seconds", stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Records processed: {RecordCount}", processedRecords);
            
            if (processedRecords > 0)
            {
                _logger.LogInformation("Average time per record: {AvgTime:F3} seconds", 
                    stopwatch.Elapsed.TotalSeconds / processedRecords);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Large file processing failed after {ElapsedTime:F2} seconds", stopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }
    
    private async Task<int> ProcessInBatchesAsync(string inputFile, string outputFile, string header, int batchSize)
    {
        var totalProcessed = 0;
        var batchNumber = 1;
        
        // This is a simplified implementation. In a real scenario, you'd want to implement
        // actual streaming CSV processing to handle truly large files
        var allRecords = await _csvReader.ReadAsync(inputFile);
        var recordsList = allRecords.ToList();
        
        // Update header with actual record count
        header = _sqlGenerator.GenerateFileHeader(inputFile, recordsList.Count);
        
        var batches = recordsList.Chunk(batchSize);
        var allStatements = new List<string>();
        
        foreach (var batch in batches)
        {
            _logger.LogDebug("Processing batch {BatchNumber} ({BatchSize} records)", batchNumber, batch.Length);
            
            var statements = _sqlGenerator.GenerateUpdateStatements(batch);
            allStatements.AddRange(statements);
            
            totalProcessed += batch.Length;
            batchNumber++;
            
            // Optional: Add a small delay to prevent overwhelming the system
            if (batchNumber % 10 == 0)
            {
                await Task.Delay(10); // 10ms delay every 10 batches
            }
        }
        
        await _sqlWriter.WriteAsync(outputFile, header, allStatements);
        
        return totalProcessed;
    }
    
    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            
            // Yield control occasionally to prevent blocking
            if (Random.Shared.Next(100) == 0)
            {
                await Task.Yield();
            }
        }
    }
}