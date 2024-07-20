using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommonSettings.Settings
{
    public sealed class SettingsService
    {

        public Uri BaseUri { get; init; }
        public PlatformSettings Android { get; init; }
        public PlatformSettings iOS { get; init; }

        public SettingsService(
            IConfiguration configuration,
            ILogger<SettingsService> logger)
        {
            var settingsConfig = configuration.GetSection("Settings");
            BaseUri = new Uri(settingsConfig["BaseUrl"]);

            Android = settingsConfig.GetSection("Android").Get<PlatformSettings>();
            iOS = settingsConfig.GetSection("iOS").Get<PlatformSettings>();
        }

        
    }
}
