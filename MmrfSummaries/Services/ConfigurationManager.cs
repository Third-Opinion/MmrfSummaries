using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MmrfSummaries.Models;

namespace MmrfSummaries.Services;

public class ConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationManager> _logger;

    public ConfigurationManager(string? configFilePath = null, ILogger<ConfigurationManager>? logger = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var configFile = configFilePath ?? "appsettings.json";
        
        if (!File.Exists(configFile))
        {
            _logger.LogError("Configuration file not found: {ConfigFile}", configFile);
            throw new FileNotFoundException($"Configuration file not found: {configFile}");
        }

        var builder = new ConfigurationBuilder()
            .AddJsonFile(configFile, optional: false, reloadOnChange: false);

        _configuration = builder.Build();
        _logger.LogInformation("Configuration loaded from: {ConfigFile}", configFile);
    }

    public ClaudeApiSettings GetClaudeApiSettings()
    {
        var settings = new ClaudeApiSettings();
        _configuration.GetSection("ClaudeApi").Bind(settings);
        
        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            _logger.LogError("Claude API key is not configured");
            throw new InvalidOperationException("Claude API key is required in configuration");
        }
        
        _logger.LogInformation("Claude API settings loaded successfully");
        return settings;
    }

    public SummarySettings GetSummarySettings()
    {
        var settings = new SummarySettings();
        _configuration.GetSection("SummarySettings").Bind(settings);
        
        if (string.IsNullOrEmpty(settings.ShortSummary.Prompt) || 
            string.IsNullOrEmpty(settings.LongSummary.Prompt))
        {
            _logger.LogError("Summary prompts are not properly configured");
            throw new InvalidOperationException("Summary prompts are required in configuration");
        }
        
        _logger.LogInformation("Summary settings loaded successfully");
        return settings;
    }

    public string ProcessPromptTemplate(string template, TrialRecord trial)
    {
        return template
            .Replace("{nct_id}", trial.NctId)
            .Replace("{brief_title}", trial.BriefTitle)
            .Replace("{brief_summary}", trial.BriefSummary)
            .Replace("{conditions}", trial.Conditions)
            .Replace("{interventions}", trial.Interventions)
            .Replace("{age}", trial.Age)
            .Replace("{genders}", trial.Genders);
    }
}