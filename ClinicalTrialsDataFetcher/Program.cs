using System.CommandLine;
using Microsoft.Extensions.Logging;
using ClinicalTrialsDataFetcher.Services;

namespace ClinicalTrialsDataFetcher;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var outputFileArgument = new Argument<string>("output-file", "Path for output CSV file");
        var nctIdsOption = new Option<string[]>("--nct-ids", "NCT IDs to fetch (can specify multiple)")
        {
            AllowMultipleArgumentsPerToken = true
        };
        var verboseOption = new Option<bool>("--verbose", "Enable detailed logging");

        var rootCommand = new RootCommand("ClinicalTrials.gov Data Fetcher - Generate sample CSV data for clinical trials")
        {
            outputFileArgument,
            nctIdsOption,
            verboseOption
        };

        rootCommand.SetHandler(async (outputFile, nctIds, verbose) =>
        {
            try
            {
                await ExecuteAsync(outputFile, nctIds, verbose);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
        }, outputFileArgument, nctIdsOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ExecuteAsync(string outputFile, string[] nctIds, bool verbose)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            if (verbose)
            {
                builder.SetMinimumLevel(LogLevel.Debug);
            }
            else
            {
                builder.SetMinimumLevel(LogLevel.Information);
            }
        });

        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            // Default NCT IDs if none provided
            if (nctIds == null || nctIds.Length == 0)
            {
                nctIds = new[]
                {
                    "NCT06171685", // Horizon trial
                    "NCT06158841"  // Cervino trial
                };
                logger.LogInformation("Using default NCT IDs: {NctIds}", string.Join(", ", nctIds));
            }

            var apiClient = new ClinicalTrialsApiClient(loggerFactory.CreateLogger<ClinicalTrialsApiClient>());
            var csvExporter = new CsvExportService(loggerFactory.CreateLogger<CsvExportService>());

            Console.WriteLine("ClinicalTrials.gov Data Fetcher");
            Console.WriteLine("===============================");
            Console.WriteLine($"Fetching data for {nctIds.Length} trials...");
            Console.WriteLine();

            var trials = await apiClient.GetTrialsAsync(nctIds.ToList());

            if (trials.Count == 0)
            {
                logger.LogWarning("No trial data was successfully fetched.");
                Console.WriteLine("No trial data was successfully fetched.");
                return;
            }

            Console.WriteLine($"Successfully fetched {trials.Count} trials:");
            foreach (var trial in trials)
            {
                Console.WriteLine($"  - {trial.NctId}: {trial.BriefTitle}");
            }
            Console.WriteLine();

            await csvExporter.ExportToCsvAsync(trials, outputFile);

            Console.WriteLine($"Sample CSV file created: {outputFile}");
            Console.WriteLine("Ready to use with the Clinical Trial Summarizer!");

            apiClient.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application failed");
            throw;
        }
    }
}