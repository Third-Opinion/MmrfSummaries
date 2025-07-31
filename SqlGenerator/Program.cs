using System.CommandLine;
using Microsoft.Extensions.Logging;
using SqlGenerator.Services;

namespace SqlGenerator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var inputFileArgument = new Argument<string>("input-file", "Path to input CSV file containing summary data");
        var outputFileArgument = new Argument<string>("output-file", "Path for output SQL file");
        
        var batchSizeOption = new Option<int>("--batch-size", () => 1000, "Number of rows to process in each batch");
        var verboseOption = new Option<bool>("--verbose", "Enable detailed logging");
        
        var rootCommand = new RootCommand("SQL Script Generator - Generate UPDATE statements for nlp_extensions database")
        {
            inputFileArgument,
            outputFileArgument,
            batchSizeOption,
            verboseOption
        };

        rootCommand.SetHandler(async (inputFile, outputFile, batchSize, verbose) =>
        {
            try
            {
                await ExecuteAsync(inputFile, outputFile, batchSize, verbose);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
        }, inputFileArgument, outputFileArgument, batchSizeOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ExecuteAsync(string inputFile, string outputFile, int batchSize, bool verbose)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            if (verbose)
                builder.SetMinimumLevel(LogLevel.Debug);
            else
                builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();
        
        logger.LogInformation("=== SQL SCRIPT GENERATOR STARTING ===");
        logger.LogInformation("Input file: {InputFile}", inputFile);
        logger.LogInformation("Output file: {OutputFile}", outputFile);
        logger.LogInformation("Batch size: {BatchSize}", batchSize);
        
        try
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException($"Input file not found: {inputFile}");
            }

            var csvReader = new CsvReaderService(loggerFactory.CreateLogger<CsvReaderService>());
            var sqlGenerator = new SqlStatementGenerator(loggerFactory.CreateLogger<SqlStatementGenerator>());
            var sqlWriter = new SqlFileWriter(loggerFactory.CreateLogger<SqlFileWriter>());
            
            var processor = new SqlScriptProcessor(
                csvReader,
                sqlGenerator, 
                sqlWriter,
                loggerFactory.CreateLogger<SqlScriptProcessor>());

            await processor.ProcessAsync(inputFile, outputFile, batchSize);
            
            logger.LogInformation("=== SQL SCRIPT GENERATOR COMPLETED SUCCESSFULLY ===");
            logger.LogInformation("SQL file generated: {OutputFile}", outputFile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application failed");
            throw;
        }
    }
}