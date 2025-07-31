# Clinical Trial Summarizer - Product Requirements Document

## 1. Product Overview

### 1.1 Purpose
The Clinical Trial Summarizer is a command-line C# application that processes CSV files containing clinical trial data and generates AI-powered summaries using the Claude API. The application reads trial information and creates both short and long descriptions for each trial, enhancing the original dataset with human-readable summaries.

### 1.2 Target Users
- Clinical research coordinators
- Data analysts working with clinical trial databases
- Healthcare professionals needing digestible trial summaries
- Research organizations processing NCDID trial data

## 2. Functional Requirements

### 2.1 Core Functionality

#### 2.1.1 CSV Processing
- **Input**: Read CSV files containing clinical trial data with columns:
  - `NCDID` (National Clinical Trial Database ID)
  - `trial-title` (Official trial title)
  - `trial-description` (Detailed trial description)
- **Output**: Generate enhanced CSV with additional columns:
  - `short-summary` (AI-generated brief summary)
  - `long-summary` (AI-generated detailed summary)
- **Preservation**: Maintain all original data and column order

#### 2.1.2 AI Summary Generation
- Generate two types of summaries for each trial:
  - **Short Summary**: Concise overview (typically 1-2 sentences)
  - **Long Summary**: Comprehensive description (typically 1-2 paragraphs)
- Use Claude API for natural language generation
- Apply configurable prompts for consistent output quality

#### 2.1.3 Batch Processing Control
- Accept command-line parameter to specify number of rows to process
- Support partial processing of large datasets
- Process rows sequentially starting from the first data row

### 2.2 Configuration Management

#### 2.2.1 Settings File Structure
The application shall use a JSON configuration file (`appsettings.json`) containing:

```json
{
  "ClaudeApi": {
    "ApiKey": "your-api-key-here",
    "Model": "claude-sonnet-4-20250514",
    "ApiVersion": "2023-06-01",
    "BaseUrl": "https://api.anthropic.com"
  },
  "SummarySettings": {
    "ShortSummary": {
      "Prompt": "Generate a concise 1-2 sentence summary of this clinical trial: {trial-title} - {trial-description}",
      "MaxTokens": 150,
      "Temperature": 0.3
    },
    "LongSummary": {
      "Prompt": "Provide a comprehensive 1-2 paragraph summary of this clinical trial, including purpose, methodology, and target population: {trial-title} - {trial-description}",
      "MaxTokens": 500,
      "Temperature": 0.3
    }
  }
}
```

#### 2.2.2 Prompt Customization
- Support template variables: `{trial-title}`, `{trial-description}`, `{NCDID}`
- Allow users to modify prompts without code changes
- Enable different model parameters for short vs. long summaries

### 2.3 Command Line Interface

#### 2.3.1 Usage Pattern
```bash
TrialSummarizer.exe <input-file> <output-file> --rows <number> [options]
```

#### 2.3.2 Parameters
- **Required**:
  - `<input-file>`: Path to input CSV file
  - `<output-file>`: Path for output CSV file
- **Optional**:
  - `--rows <number>`: Number of rows to process (default: all)
  - `--config <path>`: Custom config file path (default: appsettings.json)
  - `--verbose`: Enable detailed logging
  - `--help`: Display usage information

#### 2.3.3 Example Usage
```bash
# Process first 10 rows
TrialSummarizer.exe trials.csv enhanced_trials.csv --rows 10

# Process all rows with custom config
TrialSummarizer.exe data.csv output.csv --config custom-settings.json --verbose
```

## 3. Technical Requirements

### 3.1 Technology Stack
- **Framework**: .NET 8.0 or later
- **Language**: C# 12
- **Dependencies**:
  - System.Text.Json (JSON handling)
  - CsvHelper (CSV processing)
  - Microsoft.Extensions.Configuration (settings management)
  - Microsoft.Extensions.Logging (logging)

### 3.2 Architecture Components

#### 3.2.1 Core Classes
- `Program`: Entry point and command-line argument handling
- `TrialSummarizer`: Main business logic coordinator
- `ClaudeApiClient`: Claude API integration
- `CsvProcessor`: CSV reading/writing operations
- `ConfigurationManager`: Settings file management
- `ProgressReporter`: Status and progress tracking

#### 3.2.2 Data Models
```csharp
public class TrialRecord
{
    public string NCDID { get; set; }
    public string TrialTitle { get; set; }
    public string TrialDescription { get; set; }
    public string ShortSummary { get; set; }
    public string LongSummary { get; set; }
}

public class SummarySettings
{
    public PromptConfiguration ShortSummary { get; set; }
    public PromptConfiguration LongSummary { get; set; }
}

public class PromptConfiguration
{
    public string Prompt { get; set; }
    public int MaxTokens { get; set; }
    public decimal Temperature { get; set; }
}
```

### 3.3 Performance Requirements
- Process at least 100 trials per minute (dependent on Claude API response times)
- Memory usage should remain reasonable for files with 10,000+ rows
- Implement rate limiting to respect Claude API limits
- Support resume capability for interrupted processing

## 4. User Experience Requirements

### 4.1 Status Reporting
The application shall provide real-time progress feedback:

```
Clinical Trial Summarizer v1.0
================================
Input File: trials.csv (1,250 rows)
Output File: enhanced_trials.csv
Processing: 10 rows

Progress: [████████████████████████████████████████] 100%
┌─────────────────────────────────────────────────────────────┐
│ Row 8/10: Processing Trial NCDID-2024-001...               │
│ ✓ Short summary generated (0.8s)                           │
│ ✓ Long summary generated (1.2s)                            │
│ Status: SUCCESS                                             │
└─────────────────────────────────────────────────────────────┘

Summary:
- Total Processed: 10/10 rows
- Successful: 10
- Failed: 0
- Total Time: 45.2 seconds
- Average Time per Row: 4.5 seconds
```

### 4.2 Error Handling
- **API Failures**: Retry logic with exponential backoff
- **Invalid CSV**: Clear error messages with row/column information
- **Network Issues**: Graceful degradation with retry options
- **Rate Limiting**: Automatic throttling with user notification

### 4.3 Logging
- **Console Output**: Progress and status information
- **File Logging**: Detailed execution logs for troubleshooting
- **Error Tracking**: Failed row details for manual review

## 5. Quality Requirements

### 5.1 Reliability
- Handle malformed CSV data gracefully
- Validate API responses before processing
- Maintain data integrity throughout processing
- Support resume from last successful row

### 5.2 Maintainability
- Clear separation of concerns between components
- Comprehensive error messages and logging
- Configurable prompts and settings
- Unit testable architecture

### 5.3 Security
- Secure API key storage and handling
- No API keys in logs or console output
- Input validation for all user-provided data
- Safe file handling practices

## 6. Implementation Timeline

### Phase 1: Core Infrastructure (Week 1)
- Basic CLI argument parsing
- Configuration file loading
- CSV reading/writing foundation
- Claude API client implementation

### Phase 2: Processing Engine (Week 2)
- Trial record processing logic
- Progress reporting system
- Error handling and retry mechanisms
- Logging implementation

### Phase 3: Enhancement & Testing (Week 3)
- Performance optimization
- Comprehensive error scenarios
- User experience refinements
- Documentation and examples

## 7. Success Criteria

### 7.1 Functional Success
- Successfully process CSV files with clinical trial data
- Generate meaningful short and long summaries
- Maintain data integrity throughout processing
- Handle common error scenarios gracefully

### 7.2 Performance Success
- Process 100+ trials per minute under normal conditions
- Memory usage remains stable for large datasets
- Resume capability works reliably
- API rate limiting prevents service disruption

### 7.3 User Success
- Clear, actionable error messages
- Intuitive command-line interface
- Reliable progress reporting
- Comprehensive documentation

## 8. Future Considerations

### 8.1 Potential Enhancements
- Support for additional AI providers (OpenAI, Anthropic alternatives)
- Batch processing with parallel API calls
- Web interface for non-technical users
- Integration with clinical trial databases
- Custom output formats (JSON, XML, Excel)

### 8.2 Scalability Considerations
- Database storage for large-scale processing
- Cloud deployment options
- API key management for team environments
- Audit trails for regulatory compliance