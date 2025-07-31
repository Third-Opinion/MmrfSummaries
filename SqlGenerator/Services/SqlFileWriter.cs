using Microsoft.Extensions.Logging;

namespace SqlGenerator.Services;

public class SqlFileWriter
{
    private readonly ILogger<SqlFileWriter> _logger;

    public SqlFileWriter(ILogger<SqlFileWriter> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(string filePath, string header, IEnumerable<string> sqlStatements)
    {
        _logger.LogInformation("Writing SQL statements to file: {FilePath}", filePath);
        
        try
        {
            // Ensure output directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created output directory: {Directory}", directory);
            }
            
            using var writer = new StreamWriter(filePath, false); // false = overwrite existing file
            
            // Write header
            await writer.WriteAsync(header);
            
            // Write SQL statements
            var statementCount = 0;
            foreach (var statement in sqlStatements)
            {
                await writer.WriteLineAsync(statement);
                statementCount++;
                
                // Add separator between statements for readability
                if (statementCount % 10 == 0)
                {
                    await writer.WriteLineAsync();
                }
            }
            
            // Write footer
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("-- ================================================================");
            await writer.WriteLineAsync($"-- End of file. Total statements: {statementCount}");
            await writer.WriteLineAsync("-- ================================================================");
            
            _logger.LogInformation("Successfully wrote {Count} SQL statements to {FilePath}", statementCount, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write SQL file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to write SQL file: {ex.Message}", ex);
        }
    }
    
    public async Task WriteBatchAsync(string filePath, string header, IAsyncEnumerable<string> sqlStatements, int batchSize = 1000)
    {
        _logger.LogInformation("Writing SQL statements to file in batches: {FilePath}", filePath);
        
        try
        {
            // Ensure output directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created output directory: {Directory}", directory);
            }
            
            using var writer = new StreamWriter(filePath, false);
            
            // Write header
            await writer.WriteAsync(header);
            
            var statementCount = 0;
            var batchCount = 0;
            
            await foreach (var statement in sqlStatements)
            {
                await writer.WriteLineAsync(statement);
                statementCount++;
                
                // Add separator and flush after each batch
                if (statementCount % batchSize == 0)
                {
                    batchCount++;
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync($"-- Batch {batchCount} completed ({statementCount} statements processed)");
                    await writer.WriteLineAsync();
                    await writer.FlushAsync();
                    
                    _logger.LogDebug("Processed batch {BatchCount} ({StatementCount} statements)", batchCount, statementCount);
                }
            }
            
            // Write footer
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("-- ================================================================");
            await writer.WriteLineAsync($"-- End of file. Total statements: {statementCount}");
            await writer.WriteLineAsync("-- ================================================================");
            
            _logger.LogInformation("Successfully wrote {Count} SQL statements to {FilePath}", statementCount, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write SQL file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to write SQL file: {ex.Message}", ex);
        }
    }
}