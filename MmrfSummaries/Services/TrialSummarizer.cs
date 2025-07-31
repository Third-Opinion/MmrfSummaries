using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MmrfSummaries.Models;

namespace MmrfSummaries.Services;

public class TrialSummarizer
{
    private readonly ClaudeApiClient _claudeClient;
    private readonly CsvProcessor _csvProcessor;
    private readonly ConfigurationManager _configManager;
    private readonly ProgressReporter _progressReporter;
    private readonly ILogger<TrialSummarizer> _logger;

    public TrialSummarizer(
        ClaudeApiClient claudeClient,
        CsvProcessor csvProcessor,
        ConfigurationManager configManager,
        ProgressReporter progressReporter,
        ILogger<TrialSummarizer> logger)
    {
        _claudeClient = claudeClient;
        _csvProcessor = csvProcessor;
        _configManager = configManager;
        _progressReporter = progressReporter;
        _logger = logger;
    }

    public async Task ProcessTrialsAsync(string inputFile, string outputFile, int? maxRows = null, bool resumeMode = false)
    {
        try
        {
            _logger.LogInformation("Starting trial processing. Input: {InputFile}, Output: {OutputFile}, MaxRows: {MaxRows}, Resume: {Resume}", 
                inputFile, outputFile, maxRows, resumeMode);

            var trials = await _csvProcessor.ReadTrialsAsync(inputFile);
            var summarySettings = _configManager.GetSummarySettings();

            List<TrialRecord> trialsToProcess;
            
            if (resumeMode)
            {
                trialsToProcess = await PrepareResumeProcessingAsync(trials, outputFile);
            }
            else
            {
                var rowsToProcess = maxRows.HasValue ? Math.Min(maxRows.Value, trials.Count) : trials.Count;
                trialsToProcess = trials.Take(rowsToProcess).ToList();
            }

            if (trialsToProcess.Count == 0)
            {
                _logger.LogInformation("No trials need processing. All summaries are complete.");
                Console.WriteLine("No trials need processing. All summaries are complete.");
                return;
            }

            _progressReporter.Initialize(trialsToProcess.Count, inputFile, outputFile);

            for (int i = 0; i < trialsToProcess.Count; i++)
            {
                var trial = trialsToProcess[i];
                var rowNumber = i + 1;
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await ProcessSingleTrialAsync(trial, summarySettings);
                    stopwatch.Stop();
                    
                    _progressReporter.ReportProgress(rowNumber, trial.NctId, "SUCCESS", stopwatch.Elapsed.TotalSeconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _progressReporter.ReportError(rowNumber, trial.NctId, ex.Message);
                    
                    trial.ShortSummary = "ERROR: Failed to generate summary";
                    trial.LongSummary = "ERROR: Failed to generate summary";
                }
            }

            var allTrialsForOutput = await PrepareOutputTrialsAsync(trials, trialsToProcess, outputFile, resumeMode);
            await _csvProcessor.WriteTrialsAsync(outputFile, allTrialsForOutput);
            _progressReporter.ReportCompletion();

            _logger.LogInformation("Trial processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during trial processing");
            throw;
        }
    }

    private async Task ProcessSingleTrialAsync(TrialRecord trial, SummarySettings settings)
    {
        var shortPrompt = _configManager.ProcessPromptTemplate(settings.ShortSummary.Prompt, trial);
        var longPrompt = _configManager.ProcessPromptTemplate(settings.LongSummary.Prompt, trial);

        _logger.LogInformation("=== PROCESSING TRIAL {NctId} ===", trial.NctId);
        _logger.LogInformation("Trial Title: {Title}", trial.BriefTitle);
        _logger.LogInformation("Short Prompt:\n{ShortPrompt}", shortPrompt);
        
        _logger.LogDebug("Processing trial {NctId}: Generating short summary", trial.NctId);
        trial.ShortSummary = await _claudeClient.GenerateSummaryAsync(
            shortPrompt,
            settings.ShortSummary.MaxTokens,
            settings.ShortSummary.Temperature);

        _logger.LogInformation("Long Prompt:\n{LongPrompt}", longPrompt);
        _logger.LogDebug("Processing trial {NctId}: Generating long summary", trial.NctId);
        trial.LongSummary = await _claudeClient.GenerateSummaryAsync(
            longPrompt,
            settings.LongSummary.MaxTokens,
            settings.LongSummary.Temperature);

        if (string.IsNullOrWhiteSpace(trial.ShortSummary) || string.IsNullOrWhiteSpace(trial.LongSummary))
        {
            throw new InvalidOperationException("Claude API returned empty summary");
        }

        _logger.LogDebug("Successfully processed trial {NctId}", trial.NctId);
    }

    private async Task<List<TrialRecord>> PrepareResumeProcessingAsync(List<TrialRecord> inputTrials, string outputFile)
    {
        _logger.LogInformation("Resume mode: Checking existing output file");
        
        var existingOutput = await _csvProcessor.ReadExistingOutputAsync(outputFile);
        
        if (existingOutput == null)
        {
            _logger.LogInformation("No existing output found. Processing all trials from input.");
            return inputTrials;
        }

        var existingDict = existingOutput.ToDictionary(t => t.NctId, t => t);
        var trialsNeedingProcessing = new List<TrialRecord>();

        foreach (var inputTrial in inputTrials)
        {
            if (!existingDict.TryGetValue(inputTrial.NctId, out var existingTrial))
            {
                _logger.LogDebug("Trial {NctId} missing from output - needs processing", inputTrial.NctId);
                trialsNeedingProcessing.Add(inputTrial);
            }
            else if (IsTrialIncomplete(existingTrial))
            {
                _logger.LogDebug("Trial {NctId} has incomplete summaries - needs processing", inputTrial.NctId);
                inputTrial.ShortSummary = existingTrial.ShortSummary;
                inputTrial.LongSummary = existingTrial.LongSummary;
                trialsNeedingProcessing.Add(inputTrial);
            }
            else
            {
                _logger.LogDebug("Trial {NctId} already has complete summaries - skipping", inputTrial.NctId);
            }
        }

        _logger.LogInformation("Resume mode: {ProcessCount} trials need processing out of {TotalCount} total", 
            trialsNeedingProcessing.Count, inputTrials.Count);

        return trialsNeedingProcessing;
    }

    private async Task<List<TrialRecord>> PrepareOutputTrialsAsync(List<TrialRecord> inputTrials, List<TrialRecord> processedTrials, string outputFile, bool resumeMode)
    {
        if (!resumeMode)
        {
            var allTrialsForOutput = inputTrials.ToList();
            for (int i = 0; i < processedTrials.Count; i++)
            {
                allTrialsForOutput[i] = processedTrials[i];
            }
            return allTrialsForOutput;
        }

        var existingOutput = await _csvProcessor.ReadExistingOutputAsync(outputFile);
        if (existingOutput == null)
        {
            return processedTrials;
        }

        var existingDict = existingOutput.ToDictionary(t => t.NctId, t => t);
        var processedDict = processedTrials.ToDictionary(t => t.NctId, t => t);

        var finalOutput = new List<TrialRecord>();
        
        foreach (var inputTrial in inputTrials)
        {
            if (processedDict.TryGetValue(inputTrial.NctId, out var processedTrial))
            {
                finalOutput.Add(processedTrial);
            }
            else if (existingDict.TryGetValue(inputTrial.NctId, out var existingTrial))
            {
                finalOutput.Add(existingTrial);
            }
            else
            {
                finalOutput.Add(inputTrial);
            }
        }

        return finalOutput;
    }

    private static bool IsTrialIncomplete(TrialRecord trial)
    {
        return string.IsNullOrWhiteSpace(trial.ShortSummary) || 
               string.IsNullOrWhiteSpace(trial.LongSummary) ||
               trial.ShortSummary.StartsWith("ERROR:") ||
               trial.LongSummary.StartsWith("ERROR:");
    }
}