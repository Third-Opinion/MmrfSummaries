using System.CommandLine;
using Microsoft.Extensions.Logging;
using MmrfSummaries.Services;
using Serilog;
using Serilog.Formatting.Compact;

namespace MmrfSummaries;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var inputFileArgument = new Argument<string>("input-file", "Path to input CSV file");
        var outputFileArgument = new Argument<string>("output-file", "Path for output CSV file");
        
        var rowsOption = new Option<int?>("--rows", "Number of rows to process (default: all)");
        var configOption = new Option<string?>("--config", () => "appsettings.json", "Custom config file path");
        var verboseOption = new Option<bool>("--verbose", "Enable detailed logging");
        var resumeOption = new Option<bool>("--resume", "Resume processing by reading existing output file and only process missing/incomplete summaries");
        
        var rootCommand = new RootCommand("Clinical Trial Summarizer - Generate AI-powered summaries for clinical trial data")
        {
            inputFileArgument,
            outputFileArgument,
            rowsOption,
            configOption,
            verboseOption,
            resumeOption
        };

        rootCommand.SetHandler(async (inputFile, outputFile, rows, configFile, verbose, resume) =>
        {
            try
            {
                await ExecuteAsync(inputFile, outputFile, rows, configFile, verbose, resume);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
        }, inputFileArgument, outputFileArgument, rowsOption, configOption, verboseOption, resumeOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ExecuteAsync(string inputFile, string outputFile, int? rows, string? configFile, bool verbose, bool resume)
    {
        // Create logs directory if it doesn't exist
        var logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");
        Directory.CreateDirectory(logsDirectory);
        
        // Generate unique log file names with timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var logFileName = $"clinical-trial-summarizer-{timestamp}.log";
        var jsonLogFileName = $"clinical-trial-summarizer-{timestamp}.json";
        
        // Configure Serilog
        var logLevel = verbose ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information;
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logsDirectory, logFileName),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: Path.Combine(logsDirectory, jsonLogFileName),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Log.Logger);
        });

        var logger = loggerFactory.CreateLogger<Program>();
        
        // Log information about logging configuration
        logger.LogInformation("=== CLINICAL TRIAL SUMMARIZER STARTING ===");
        logger.LogInformation("Console logging: Enabled");
        logger.LogInformation("File logging: {LogPath}", Path.Combine(logsDirectory, logFileName));
        logger.LogInformation("JSON logging: {JsonLogPath}", Path.Combine(logsDirectory, jsonLogFileName));
        logger.LogInformation("Log level: {LogLevel}", verbose ? "Debug" : "Information");
        logger.LogInformation("Timestamp: {Timestamp}", DateTime.Now);
        
        try
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException($"Input file not found: {inputFile}");
            }

            var configManager = new ConfigurationManager(configFile, loggerFactory.CreateLogger<ConfigurationManager>());
            var claudeSettings = configManager.GetClaudeApiSettings();
            
            var claudeClient = new ClaudeApiClient(claudeSettings, loggerFactory.CreateLogger<ClaudeApiClient>());
            var csvProcessor = new CsvProcessor(loggerFactory.CreateLogger<CsvProcessor>());
            var progressReporter = new ProgressReporter(loggerFactory.CreateLogger<ProgressReporter>());
            
            var trialSummarizer = new TrialSummarizer(
                claudeClient,
                csvProcessor,
                configManager,
                progressReporter,
                loggerFactory.CreateLogger<TrialSummarizer>());

            await trialSummarizer.ProcessTrialsAsync(inputFile, outputFile, rows, resume);
            
            claudeClient.Dispose();
            
            logger.LogInformation("=== CLINICAL TRIAL SUMMARIZER COMPLETED SUCCESSFULLY ===");
            logger.LogInformation("Output file: {OutputFile}", outputFile);
            logger.LogInformation("Log files saved to: {LogsDirectory}", logsDirectory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application failed");
            throw;
        }
        finally
        {
            // Ensure all logs are flushed before application exits
            Log.CloseAndFlush();
        }
    }
}