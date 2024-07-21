using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace uSignIn.CommonSettings.Settings
{
    public sealed class SettingsService
    {

        public Uri BaseUri { get; set; }
        public PlatformSettings Android { get; init; }
        public PlatformSettings iOS { get; init; }

        public SettingsService(
            IConfiguration configuration,
            ILogger<SettingsService> logger)
        {
            var settingsConfig = configuration.GetSection("Settings");
            string? baseUrl = settingsConfig["BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogCritical("BaseUrl is not configured in Settings:BaseUrl. Please fix.");
            }

			BaseUri = new Uri(baseUrl);

            Android = settingsConfig.GetSection("Android").Get<PlatformSettings>();
            iOS = settingsConfig.GetSection("iOS").Get<PlatformSettings>();
        }
    }
}
