# Clinical Trial Summarizer

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

AI-powered clinical trial summarization tool that processes CSV files containing clinical trial data and generates intelligent summaries using Claude API.

## 🎯 Overview

The Clinical Trial Summarizer is a .NET 8.0 console application that enhances clinical trial datasets by automatically generating both concise and comprehensive summaries. It integrates with Anthropic's Claude API to create human-readable summaries that make clinical trial data more accessible to researchers, coordinators, and healthcare professionals.

### Key Features

- **🤖 AI-Powered Summaries**: Generates both short (1-2 sentences) and long (1-2 paragraphs) summaries using Claude API
- **📊 CSV Processing**: Reads clinical trial data from CSV files with ClinicalTrials.gov format support
- **⚡ Batch Processing**: Process specific number of rows or entire datasets
- **🔄 Resume Capability**: Continue processing from where you left off if interrupted
- **📝 Comprehensive Logging**: Console, file, and JSON logging with Serilog
- **🎛️ Configurable**: Customizable prompts, API settings, and processing parameters
- **🚀 Progress Tracking**: Real-time progress reporting with detailed statistics
- **🛡️ Error Handling**: Robust retry logic with exponential backoff for API calls
- **🗄️ SQL Integration**: Generate UPDATE scripts for database integration with nlp_extensions table

## 📋 Requirements

- **.NET 8.0 SDK** or later
- **Claude API Key** from Anthropic
- **Input CSV file** with clinical trial data

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/Third-Opinion/MmrfSummaries.git
cd MmrfSummaries
```

### 2. Configure API Key

Edit `MmrfSummaries/appsettings.json` and add your Claude API key:

```json
{
  "ClaudeApi": {
    "ApiKey": "your-claude-api-key-here",
    "Model": "claude-sonnet-4-20250514",
    "ApiVersion": "2023-06-01",
    "BaseUrl": "https://api.anthropic.com"
  }
}
```

### 3. Build the Application

```bash
cd MmrfSummaries
dotnet build
```

### 4. Run the Application

```bash
# Process first 10 rows
dotnet run -- sample_trials.csv output.csv --rows 10

# Process all rows with verbose logging
dotnet run -- input.csv output.csv --verbose

# Resume interrupted processing
dotnet run -- input.csv output.csv --resume
```

## 📁 Project Structure

```
MmrfSummaries/
├── MmrfSummaries/               # Main application
│   ├── Models/                  # Data models
│   │   ├── TrialRecord.cs       # Clinical trial data model
│   │   ├── ClaudeApiSettings.cs # API configuration model
│   │   ├── SummarySettings.cs   # Summary configuration model
│   │   └── PromptConfiguration.cs # Prompt settings model
│   ├── Services/                # Business logic
│   │   ├── TrialSummarizer.cs   # Main processing orchestrator
│   │   ├── ClaudeApiClient.cs   # Claude API integration
│   │   ├── CsvProcessor.cs      # CSV reading/writing
│   │   ├── ConfigurationManager.cs # Settings management
│   │   └── ProgressReporter.cs  # Progress tracking
│   ├── Program.cs               # Application entry point
│   ├── appsettings.json         # Configuration file
│   └── sample_trials.csv        # Sample data
├── ClinicalTrialsDataFetcher/   # Data fetching utility
└── README.md                    # This file
```

## 📊 Input CSV Format

The application expects CSV files with the following columns (ClinicalTrials.gov format):

| Column | Description |
|--------|-------------|
| `nct_id` | National Clinical Trial identifier |
| `brief_title` | Official trial title |
| `brief_summary` | Detailed trial description |
| `conditions` | Medical conditions being studied |
| `interventions` | Treatments or interventions |
| `age` | Age eligibility criteria |
| `genders` | Gender eligibility |

### Sample Input

```csv
nct_id,brief_title,brief_summary,conditions,interventions,age,genders
NCT06171685,MMRC Horizon One Adaptive Platform Trial,"This trial is an adaptive platform trial...",Relapse Multiple Myeloma,DRUG: Teclistamab,18 Years to 99 Years,ALL
```

## 📤 Output Format

The application generates an enhanced CSV with two additional columns:

- `short_summary`: Concise 1-2 sentence AI-generated summary
- `long_summary`: Comprehensive 1-2 paragraph AI-generated summary

## 🎛️ Command Line Options

```bash
dotnet run -- <input-file> <output-file> [options]

Arguments:
  input-file              Path to input CSV file
  output-file             Path for output CSV file

Options:
  --rows <number>         Number of rows to process (default: all)
  --config <path>         Custom config file path (default: appsettings.json)
  --verbose               Enable detailed logging
  --resume                Resume processing from last successful row
  --help                  Show help information
```

### Examples

```bash
# Process first 5 trials with verbose logging
dotnet run -- trials.csv enhanced_trials.csv --rows 5 --verbose

# Resume interrupted processing
dotnet run -- large_dataset.csv output.csv --resume

# Use custom configuration
dotnet run -- data.csv results.csv --config custom-settings.json
```

## ⚙️ Configuration

### API Settings

Configure Claude API settings in `appsettings.json`:

```json
{
  "ClaudeApi": {
    "ApiKey": "your-api-key-here",
    "Model": "claude-sonnet-4-20250514",
    "ApiVersion": "2023-06-01",
    "BaseUrl": "https://api.anthropic.com"
  }
}
```

### Summary Prompts

Customize AI prompts for different summary types:

```json
{
  "SummarySettings": {
    "ShortSummary": {
      "Prompt": "Generate a concise 1-2 sentence summary...",
      "MaxTokens": 150,
      "Temperature": 0.1
    },
    "LongSummary": {
      "Prompt": "Provide a comprehensive summary...",
      "MaxTokens": 500,
      "Temperature": 0.1
    }
  }
}
```

## 📊 Data Fetcher Utility

The project includes a separate utility for fetching sample data from ClinicalTrials.gov:

```bash
cd ClinicalTrialsDataFetcher
dotnet run -- --nct-ids NCT06171685,NCT06158841 --output sample_data.csv
```

## 🗄️ SQL Script Generator

The SQL Generator creates UPDATE statements for database integration:

```bash
# Generate SQL script from CSV summaries
dotnet run --project SqlGenerator -- summaries.csv update_statements.sql

# With verbose logging and custom batch size
dotnet run --project SqlGenerator -- summaries.csv update_statements.sql --verbose --batch-size 500
```

### Database Integration Workflow

```bash
# 1. Generate AI summaries
dotnet run --project MmrfSummaries -- input.csv summaries.csv

# 2. Generate SQL UPDATE statements
dotnet run --project SqlGenerator -- summaries.csv database_updates.sql

# 3. Execute SQL against your nlp_extensions database
# Run database_updates.sql in your SQL client
```

### Generated SQL Format

```sql
UPDATE nlp_extensions
SET summary = 'AI-generated comprehensive summary...',
    short_summary = 'AI-generated short summary...'
WHERE nct_id = 'NCT06171685';
```

For detailed SQL Generator documentation, see [SqlGenerator/README.md](SqlGenerator/README.md).

## 📝 Logging

The application provides comprehensive logging:

- **Console**: Real-time progress and status updates
- **File Logs**: Detailed execution logs in `logs/` directory
- **JSON Logs**: Structured logs for automated processing
- **Progress Reports**: Processing statistics and timing

### Log Files

```
logs/
├── clinical-trial-summarizer-20250731_120000.log    # Human-readable logs
└── clinical-trial-summarizer-20250731_120000.json   # Machine-readable logs
```

## 🔧 Development

### Prerequisites

- .NET 8.0 SDK
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Building

```bash
dotnet restore
dotnet build
```

### Testing

```bash
dotnet test
```

### Dependencies

The project uses the following key packages:

- **CsvHelper**: CSV file processing
- **Serilog**: Comprehensive logging
- **System.CommandLine**: Command-line interface
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Logging abstractions

## 🚨 Error Handling

The application includes robust error handling:

- **API Failures**: Automatic retry with exponential backoff
- **Network Issues**: Graceful degradation and recovery
- **Invalid Data**: Clear error messages with context
- **Rate Limiting**: Automatic throttling and user notification
- **Interruption Recovery**: Resume from last successful record

## 📈 Performance

- **Processing Speed**: ~100+ trials per minute (API dependent)
- **Memory Efficient**: Streaming CSV processing for large files
- **Resumable**: Continue processing interrupted jobs
- **Rate Limited**: Respects API limits automatically

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/new-feature`
5. Submit a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

If you encounter issues:

1. Check the [troubleshooting section](#troubleshooting)
2. Review log files in the `logs/` directory
3. Open an issue on GitHub with:
   - Error messages
   - Log file excerpts
   - Input data samples (without sensitive information)

## 🔍 Troubleshooting

### Common Issues

**API Key Issues**
```bash
# Error: Unauthorized
# Solution: Verify your Claude API key in appsettings.json
```

**CSV Format Issues**
```bash
# Error: Column not found
# Solution: Ensure CSV has required columns: nct_id, brief_title, brief_summary, conditions, interventions, age, genders
```

**Network Issues**
```bash
# Error: Request timeout
# Solution: Check internet connection, API will automatically retry
```

**Large File Processing**
```bash
# Use --rows option to process in smaller batches
dotnet run -- large_file.csv output.csv --rows 100
```

## 🎯 Roadmap

- [ ] Support for additional AI providers (OpenAI, Azure OpenAI)
- [ ] Parallel processing for improved performance
- [ ] Web interface for non-technical users
- [ ] Database integration for large-scale processing
- [ ] Custom output formats (JSON, XML, Excel)
- [ ] Advanced filtering and search capabilities

---

**Built with ❤️ by the Third Opinion team**

For more information, visit our [GitHub repository](https://github.com/Third-Opinion/MmrfSummaries).