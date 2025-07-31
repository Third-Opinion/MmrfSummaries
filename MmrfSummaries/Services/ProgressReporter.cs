using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MmrfSummaries.Services;

public class ProgressReporter
{
    private readonly ILogger<ProgressReporter> _logger;
    private readonly Stopwatch _stopwatch;
    private int _totalRows;
    private int _processedRows;
    private int _successfulRows;
    private int _failedRows;
    private readonly DateTime _startTime;

    public ProgressReporter(ILogger<ProgressReporter> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
        _startTime = DateTime.Now;
    }

    public void Initialize(int totalRows, string inputFile, string outputFile)
    {
        _totalRows = totalRows;
        _processedRows = 0;
        _successfulRows = 0;
        _failedRows = 0;
        
        Console.WriteLine("Clinical Trial Summarizer v1.0");
        Console.WriteLine("================================");
        Console.WriteLine($"Input File: {Path.GetFileName(inputFile)} ({totalRows} rows)");
        Console.WriteLine($"Output File: {Path.GetFileName(outputFile)}");
        Console.WriteLine($"Processing: {totalRows} rows");
        Console.WriteLine();
        
        _stopwatch.Start();
    }

    public void ReportProgress(int currentRow, string nctId, string status, double elapsedSeconds)
    {
        _processedRows = currentRow;
        
        if (status == "SUCCESS")
        {
            _successfulRows++;
        }
        else
        {
            _failedRows++;
        }

        var progressPercentage = (double)currentRow / _totalRows * 100;
        var progressBar = CreateProgressBar(progressPercentage);
        
        Console.Clear();
        Console.WriteLine("Clinical Trial Summarizer v1.0");
        Console.WriteLine("================================");
        Console.WriteLine($"Input File: Processing... ({_totalRows} rows)");
        Console.WriteLine($"Output File: Writing...");
        Console.WriteLine($"Processing: {currentRow}/{_totalRows} rows");
        Console.WriteLine();
        Console.WriteLine($"Progress: {progressBar} {progressPercentage:F1}%");
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"│ Row {currentRow}/{_totalRows}: Processing Trial {nctId}...{new string(' ', Math.Max(0, 20 - nctId.Length))}│");
        Console.WriteLine($"│ ✓ Summary generation completed ({elapsedSeconds:F1}s){new string(' ', Math.Max(0, 26 - elapsedSeconds.ToString("F1").Length))}│");
        Console.WriteLine($"│ Status: {status}{new string(' ', Math.Max(0, 52 - status.Length))}│");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        
        _logger.LogInformation("Processed row {CurrentRow}/{TotalRows}: {NctId} - {Status} ({ElapsedSeconds:F1}s)", 
            currentRow, _totalRows, nctId, status, elapsedSeconds);
    }

    public void ReportCompletion()
    {
        _stopwatch.Stop();
        var totalTime = _stopwatch.Elapsed.TotalSeconds;
        var avgTimePerRow = _processedRows > 0 ? totalTime / _processedRows : 0;
        
        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"- Total Processed: {_processedRows}/{_totalRows} rows");
        Console.WriteLine($"- Successful: {_successfulRows}");
        Console.WriteLine($"- Failed: {_failedRows}");
        Console.WriteLine($"- Total Time: {totalTime:F1} seconds");
        Console.WriteLine($"- Average Time per Row: {avgTimePerRow:F1} seconds");
        
        _logger.LogInformation("Processing completed. Total: {ProcessedRows}, Successful: {SuccessfulRows}, Failed: {FailedRows}, Time: {TotalTime:F1}s", 
            _processedRows, _successfulRows, _failedRows, totalTime);
    }

    public void ReportError(int currentRow, string nctId, string error)
    {
        _failedRows++;
        Console.WriteLine($"ERROR processing row {currentRow} ({nctId}): {error}");
        _logger.LogError("Failed to process row {CurrentRow} ({NctId}): {Error}", currentRow, nctId, error);
    }

    private static string CreateProgressBar(double percentage)
    {
        const int barLength = 40;
        var filledLength = (int)(barLength * percentage / 100);
        var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
        return $"[{bar}]";
    }
}