# Clinical Trial Summarizer Suite

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

A comprehensive .NET 8.0 solution for AI-powered clinical trial data processing, summarization, and database integration using Anthropic's Claude API.

## 🎯 Overview

The Clinical Trial Summarizer Suite is a multi-project .NET solution that provides end-to-end processing for clinical trial data. From fetching raw data to generating AI summaries and creating database UPDATE scripts, this suite streamlines the entire workflow for researchers, coordinators, and healthcare professionals.

### Solution Components

| Project | Purpose | Key Features |
|---------|---------|--------------|
| **MmrfSummaries** | Main AI summarization engine | Claude API integration, batch processing, resume capability |
| **SqlGenerator** | Database integration utility | SQL script generation, batch processing, SQL injection prevention |
| **ClinicalTrialsDataFetcher** | Data acquisition tool | ClinicalTrials.gov API integration, CSV export |

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0 SDK** or later
- **Claude API Key** from Anthropic
- **Git** for version control

### 1. Clone and Setup

```bash
git clone https://github.com/Third-Opinion/MmrfSummaries.git
cd MmrfSummaries
dotnet restore
```

### 2. Configure API Key

Create or update `MmrfSummaries/appsettings.json`:

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

### 3. Build Solution

```bash
dotnet build
```

### 4. Complete Workflow Example

```bash
# 1. Fetch clinical trial data (optional)
dotnet run --project ClinicalTrialsDataFetcher -- --nct-ids NCT06171685,NCT06158841 --output trials.csv

# 2. Generate AI summaries
dotnet run --project MmrfSummaries -- trials.csv summaries.csv --rows 10 --verbose

# 3. Generate SQL UPDATE statements
dotnet run --project SqlGenerator -- summaries.csv database_updates.sql --verbose

# 4. Execute the SQL against your database
# Run database_updates.sql in your SQL client
```

## 📁 Solution Structure

```
MmrfSummaries/                          # Solution root
├── MmrfSummaries.sln                   # Visual Studio solution file
├── global.json                         # .NET SDK version configuration
├── README.md                           # This file
├── .gitignore                          # Git ignore patterns
├── .env.example                        # Environment variables template
│
├── MmrfSummaries/                      # 🤖 Main AI Summarizer
│   ├── Models/                         # Data models and configurations
│   │   ├── TrialRecord.cs              # Clinical trial data structure
│   │   ├── ClaudeApiSettings.cs        # API configuration
│   │   ├── SummarySettings.cs          # Summary generation settings
│   │   └── PromptConfiguration.cs      # AI prompt configurations
│   ├── Services/                       # Core business logic
│   │   ├── TrialSummarizer.cs          # Main processing orchestrator
│   │   ├── ClaudeApiClient.cs          # Claude API integration
│   │   ├── CsvProcessor.cs             # CSV file operations
│   │   ├── ConfigurationManager.cs     # Settings management
│   │   └── ProgressReporter.cs         # Progress tracking and reporting
│   ├── Program.cs                      # Application entry point
│   ├── appsettings.json                # Configuration file
│   ├── sample_trials.csv               # Sample input data
│   └── logs/                           # Application logs directory
│
├── SqlGenerator/                       # 🗄️ Database Integration Tool
│   ├── Models/
│   │   └── SummaryRecord.cs            # CSV data model for SQL generation
│   ├── Services/
│   │   ├── CsvReaderService.cs         # CSV reading and validation
│   │   ├── SqlStatementGenerator.cs    # SQL generation with escaping
│   │   ├── SqlFileWriter.cs            # SQL file writing with headers
│   │   └── SqlScriptProcessor.cs       # Main processing orchestrator
│   ├── Program.cs                      # CLI entry point
│   ├── SqlGenerator.csproj             # Project configuration
│   └── README.md                       # Detailed SQL Generator docs
│
├── ClinicalTrialsDataFetcher/          # 📊 Data Acquisition Utility
│   ├── Models/
│   │   └── ClinicalTrialData.cs        # API response data models
│   ├── Services/
│   │   ├── ClinicalTrialsApiClient.cs  # ClinicalTrials.gov API client
│   │   └── CsvExportService.cs         # CSV export functionality
│   ├── Program.cs                      # CLI entry point
│   ├── ClinicalTrialsDataFetcher.csproj # Project configuration
│   └── sample_trials.csv               # Generated sample data
│
└── .taskmaster/                        # 📋 Project Management
    ├── tasks/                          # Task definitions and status
    ├── templates/                      # PRD and task templates
    └── config.json                     # Task Master configuration
```

## 🔧 Project Details

### 🤖 MmrfSummaries - AI Summarization Engine

The core application that processes clinical trial CSV files and generates AI-powered summaries.

**Key Features:**
- ✅ Claude API integration with retry logic
- ✅ Batch processing with configurable row limits
- ✅ Resume capability for interrupted processing
- ✅ Comprehensive logging (console, file, JSON)
- ✅ Progress tracking with statistics
- ✅ Configurable AI prompts and parameters

**Usage:**
```bash
# Basic usage
dotnet run --project MmrfSummaries -- input.csv output.csv

# Process first 50 rows with verbose logging
dotnet run --project MmrfSummaries -- input.csv output.csv --rows 50 --verbose

# Resume interrupted processing
dotnet run --project MmrfSummaries -- input.csv output.csv --resume
```

### 🗄️ SqlGenerator - Database Integration Tool

Converts AI-generated summaries into SQL UPDATE statements for database integration.

**Key Features:**
- ✅ SQL injection prevention with proper escaping
- ✅ Batch processing for large datasets
- ✅ Comprehensive validation and error handling
- ✅ Generated SQL file includes metadata and comments
- ✅ Support for nlp_extensions database schema

**Usage:**
```bash
# Generate SQL UPDATE statements
dotnet run --project SqlGenerator -- summaries.csv updates.sql

# With custom batch size and verbose logging
dotnet run --project SqlGenerator -- summaries.csv updates.sql --batch-size 500 --verbose
```

**Generated SQL Format:**
```sql
UPDATE nlp_extensions
SET summary = 'AI-generated comprehensive summary...',
    short_summary = 'AI-generated short summary...'
WHERE nct_id = 'NCT06171685';
```

### 📊 ClinicalTrialsDataFetcher - Data Acquisition Tool

Fetches clinical trial data from ClinicalTrials.gov API and exports to CSV format.

**Key Features:**
- ✅ Direct integration with ClinicalTrials.gov API
- ✅ Flexible NCT ID input (comma-separated or file)
- ✅ CSV export with standardized column format
- ✅ Error handling for API failures
- ✅ Progress reporting for large datasets

**Usage:**
```bash
# Fetch specific trials
dotnet run --project ClinicalTrialsDataFetcher -- --nct-ids NCT06171685,NCT06158841 --output data.csv

# Fetch from file list
dotnet run --project ClinicalTrialsDataFetcher -- --nct-file trial_ids.txt --output data.csv
```

## 📊 Data Flow

```mermaid
graph LR
A[ClinicalTrials.gov] --> B[ClinicalTrialsDataFetcher]
B --> C[Raw CSV Data]
C --> D[MmrfSummaries]
D --> E[AI-Enhanced CSV]
E --> F[SqlGenerator]
F --> G[SQL UPDATE Scripts]
G --> H[Database Integration]
```

## 🎛️ Configuration

### Environment Variables

Create a `.env` file based on `.env.example`:

```bash
# Claude API Configuration
CLAUDE_API_KEY=your-api-key-here
CLAUDE_MODEL=claude-sonnet-4-20250514

# Application Settings
LOG_LEVEL=Information
BATCH_SIZE=1000
```

### API Settings (appsettings.json)

```json
{
  "ClaudeApi": {
    "ApiKey": "your-api-key-here",
    "Model": "claude-sonnet-4-20250514",
    "ApiVersion": "2023-06-01",
    "BaseUrl": "https://api.anthropic.com",
    "MaxRetries": 3,
    "TimeoutSeconds": 30
  },
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

## 📋 Input/Output Formats

### Expected CSV Input Format

| Column | Description | Required |
|--------|-------------|----------|
| `nct_id` | National Clinical Trial identifier | ✅ Yes |
| `brief_title` | Official trial title | ✅ Yes |
| `brief_summary` | Detailed trial description | ✅ Yes |
| `conditions` | Medical conditions being studied | ✅ Yes |
| `interventions` | Treatments or interventions | ✅ Yes |
| `age` | Age eligibility criteria | ✅ Yes |
| `genders` | Gender eligibility | ✅ Yes |

### Enhanced CSV Output Format

The AI summarizer adds these columns:
- `short_summary`: Concise 1-2 sentence AI-generated summary
- `long_summary`: Comprehensive 1-2 paragraph AI-generated summary

### Database Schema

The SQL Generator targets this table structure:

```sql
CREATE TABLE nlp_extensions (
    nct_id VARCHAR(50) PRIMARY KEY,
    summary TEXT,                    -- Maps to long_summary
    short_summary TEXT,              -- Maps to short_summary
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

## 🚨 Error Handling & Logging

### Comprehensive Error Handling
- **API Failures**: Automatic retry with exponential backoff
- **Network Issues**: Graceful degradation and recovery
- **File Operations**: Detailed error messages with context
- **Data Validation**: Clear validation error reporting
- **Memory Management**: Optimized for large file processing

### Logging Levels
- **Console**: Real-time progress and status updates
- **File Logs**: Detailed execution logs with timestamps
- **JSON Logs**: Structured logs for automated processing
- **Performance Metrics**: Processing statistics and timing

### Log File Locations
```
logs/
├── clinical-trial-summarizer-YYYYMMDD_HHMMSS.log     # Human-readable
├── clinical-trial-summarizer-YYYYMMDD_HHMMSS.json    # Machine-readable
└── sql-generator-YYYYMMDD_HHMMSS.log                 # SQL Generator logs
```

## 📈 Performance & Scalability

### Performance Metrics
- **AI Summarization**: ~100+ trials per minute (API dependent)
- **SQL Generation**: ~50,000 records per minute
- **Data Fetching**: ~200 trials per minute (API dependent)

### Memory Optimization
- **Streaming Processing**: Handles large CSV files efficiently
- **Batch Processing**: Configurable batch sizes for memory management
- **Resource Management**: Proper disposal and garbage collection

### Scalability Features
- **Resume Capability**: Continue interrupted long-running processes
- **Configurable Batch Sizes**: Adjust for available system resources
- **Rate Limiting**: Automatic throttling for API compliance
- **Progress Reporting**: Real-time processing status

## 🔧 Development

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build MmrfSummaries/MmrfSummaries.csproj

# Run tests (when available)
dotnet test

# Clean build artifacts
dotnet clean
```

### Project Dependencies

**Core Dependencies:**
- **CsvHelper**: High-performance CSV processing
- **Serilog**: Structured logging framework
- **System.CommandLine**: Modern CLI framework
- **Microsoft.Extensions.***: Configuration and dependency injection

**API Integration:**
- **System.Net.Http**: HTTP client for API calls
- **System.Text.Json**: JSON serialization/deserialization

## 🧪 Testing Strategy

### Unit Testing
- Model validation and data transformation
- CSV processing with various edge cases
- SQL generation and escaping
- API client error handling

### Integration Testing
- End-to-end workflow testing
- Database integration verification
- API rate limiting and retry logic
- Large file processing validation

### Performance Testing
- Memory usage monitoring
- Processing speed benchmarks
- Concurrent processing validation
- API throttling compliance

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **Make** your changes following the coding standards
4. **Test** your changes thoroughly
5. **Commit** your changes: `git commit -m 'Add amazing feature'`
6. **Push** to the branch: `git push origin feature/amazing-feature`
7. **Open** a Pull Request

### Coding Standards
- Follow C# naming conventions
- Add XML documentation for public APIs
- Include unit tests for new functionality
- Update README.md for new features
- Ensure proper error handling

## 🛠️ Troubleshooting

### Common Issues

**Authentication Errors**
```bash
# Error: 401 Unauthorized
# Solution: Verify Claude API key in appsettings.json or environment variables
```

**CSV Format Issues**
```bash
# Error: Required column not found
# Solution: Ensure CSV has all required columns with correct names
```

**Memory Issues with Large Files**
```bash
# Error: OutOfMemoryException
# Solution: Reduce batch size or process in smaller chunks
dotnet run --project MmrfSummaries -- large_file.csv output.csv --rows 100
```

**Network Connectivity**
```bash
# Error: Request timeout or connection failed
# Solution: Check internet connection; API will automatically retry
```

### Debug Mode

Enable detailed debugging:

```bash
# Set environment variable for detailed logging
export ASPNETCORE_ENVIRONMENT=Development

# Run with verbose logging
dotnet run --project MmrfSummaries -- input.csv output.csv --verbose
```

## 🚀 Roadmap

### Planned Features
- [ ] **Multi-Provider AI Support**: OpenAI, Azure OpenAI integration
- [ ] **Parallel Processing**: Multi-threaded summarization
- [ ] **Web Interface**: Browser-based UI for non-technical users
- [ ] **Advanced Database Integration**: Direct database connectivity
- [ ] **Custom Output Formats**: JSON, XML, Excel export options
- [ ] **Advanced Analytics**: Processing statistics and insights
- [ ] **Docker Support**: Containerized deployment
- [ ] **Cloud Integration**: Azure/AWS deployment templates

### Performance Improvements
- [ ] **Caching Layer**: Reduce redundant API calls
- [ ] **Queue Processing**: Background job processing
- [ ] **Horizontal Scaling**: Multi-instance support
- [ ] **Advanced Monitoring**: Health checks and metrics

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support & Contact

### Getting Help
1. **Check Documentation**: Review project READMEs and inline documentation
2. **Search Issues**: Look through existing GitHub issues
3. **Create New Issue**: Provide detailed information including:
   - Error messages and stack traces
   - Input data samples (anonymized)
   - System configuration details
   - Steps to reproduce the issue

### Community Resources
- **GitHub Issues**: Bug reports and feature requests
- **Discussions**: General questions and community support
- **Wiki**: Extended documentation and tutorials

---

**Built with ❤️ by the Third Opinion team**

For more information and updates, visit our [GitHub repository](https://github.com/Third-Opinion/MmrfSummaries).