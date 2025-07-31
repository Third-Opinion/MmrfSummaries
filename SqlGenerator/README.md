# SQL Script Generator

A .NET 8.0 console application that generates SQL UPDATE scripts from clinical trial summary CSV files to update the `nlp_extensions` database.

## Overview

This utility reads CSV files containing AI-generated clinical trial summaries and creates SQL UPDATE statements to populate the `nlp_extensions` database with the summary data.

## Features

- ✅ **CSV Validation**: Validates input CSV structure and required columns
- ✅ **SQL Generation**: Creates properly formatted UPDATE statements 
- ✅ **SQL Escaping**: Handles special characters and SQL injection prevention
- ✅ **Batch Processing**: Efficient processing for large CSV files
- ✅ **Progress Logging**: Detailed logging with performance metrics
- ✅ **Error Handling**: Comprehensive error handling and validation
- ✅ **File Headers**: Generated SQL files include metadata and comments

## Usage

### Basic Usage

```bash
# Generate SQL script from CSV file
dotnet run --project SqlGenerator -- input.csv output.sql

# With verbose logging
dotnet run --project SqlGenerator -- input.csv output.sql --verbose

# With custom batch size
dotnet run --project SqlGenerator -- input.csv output.sql --batch-size 500
```

### Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `<input-file>` | Path to input CSV file | Required |
| `<output-file>` | Path for output SQL file | Required |
| `--batch-size` | Records per batch for processing | 1000 |
| `--verbose` | Enable detailed logging | false |

## Input CSV Format

The CSV file must contain the following columns:

| Column | Description | Required |
|--------|-------------|----------|
| `nct_id` | National Clinical Trial identifier | ✅ Yes |
| `short_summary` | AI-generated short summary | Optional* |
| `long_summary` | AI-generated long summary | Optional* |

*At least one summary column must have data.

### Example Input CSV

```csv
nct_id,brief_title,brief_summary,conditions,interventions,age,genders,short_summary,long_summary
NCT06171685,MMRC Trial,This trial is...,Multiple Myeloma,Teclistamab,18-99,ALL,Short summary text,Long detailed summary text
```

## Generated SQL Format

The tool generates UPDATE statements in the following format:

```sql
UPDATE nlp_extensions
SET summary = 'AI-generated long summary text',
    short_summary = 'AI-generated short summary text'
WHERE nct_id = 'NCT06171685';
```

### SQL File Structure

Generated SQL files include:

- **Header**: Generation timestamp, source file, record count
- **UPDATE Statements**: One per CSV row with valid data
- **Footer**: Total statement count and completion marker
- **Comments**: Explanatory comments throughout

## Database Schema

The tool targets a database table with the following structure:

```sql
CREATE TABLE nlp_extensions (
    nct_id VARCHAR(50) PRIMARY KEY,
    summary TEXT,
    short_summary TEXT,
    -- other columns...
);
```

## Performance

- **Processing Speed**: ~50,000 records/minute
- **Memory Usage**: Optimized for large files with batch processing
- **Batch Processing**: Configurable batch sizes for memory management

## Error Handling

The application handles various error scenarios:

- ❌ Missing input files
- ❌ Invalid CSV structure
- ❌ Missing required columns
- ❌ File permission issues
- ❌ Invalid output paths
- ❌ Memory constraints

## Examples

### Process Clinical Trial Summaries

```bash
# Generate SQL for trial summaries
dotnet run --project SqlGenerator -- summaries.csv update_nlp.sql --verbose

# Output:
# === SQL SCRIPT GENERATOR STARTING ===
# Input file: summaries.csv
# Output file: update_nlp.sql
# Batch size: 1000
# Processing completed successfully
# Total processing time: 0.15 seconds
# Records processed: 100
# SQL statements generated: 98
# Average time per record: 0.002 seconds
```

### Large File Processing

```bash
# Process large CSV with custom batch size
dotnet run --project SqlGenerator -- large_summaries.csv bulk_update.sql --batch-size 5000
```

## Integration

This utility integrates with the Clinical Trial Summarizer workflow:

1. **Generate Summaries**: Use MmrfSummaries to create AI summaries
2. **Generate SQL**: Use SqlGenerator to create UPDATE statements  
3. **Execute SQL**: Run generated statements against your database

```bash
# Complete workflow
dotnet run --project MmrfSummaries -- input.csv summaries.csv --rows 100
dotnet run --project SqlGenerator -- summaries.csv update_statements.sql
# Execute update_statements.sql against your database
```

## Dependencies

- .NET 8.0 SDK
- CsvHelper (32.0.3)
- System.CommandLine (2.0.0-beta4)
- Microsoft.Extensions.Logging (8.0.0)

## Build and Run

```bash
# Build the project
dotnet build SqlGenerator/SqlGenerator.csproj

# Run with arguments
dotnet run --project SqlGenerator -- --help

# Create standalone executable
dotnet publish SqlGenerator/SqlGenerator.csproj -c Release -o ./publish
```

## File Structure

```
SqlGenerator/
├── SqlGenerator.csproj          # Project file
├── Program.cs                   # Entry point and CLI
├── README.md                    # This file
├── Models/
│   └── SummaryRecord.cs         # Data model
└── Services/
    ├── CsvReaderService.cs      # CSV reading and validation
    ├── SqlStatementGenerator.cs # SQL generation logic
    ├── SqlFileWriter.cs         # SQL file writing
    └── SqlScriptProcessor.cs    # Main processing orchestrator
```

## Contributing

When modifying the SQL Generator:

1. Maintain backward compatibility with existing CSV formats
2. Add unit tests for new functionality
3. Update this README with new features
4. Test with various CSV sizes and edge cases
5. Ensure proper SQL escaping for security

## License

This project is part of the Clinical Trial Summarizer suite and follows the same licensing terms.