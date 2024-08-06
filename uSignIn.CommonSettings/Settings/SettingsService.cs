using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace uSignIn.CommonSettings.Settings
{
    public sealed class SettingsService
    {
		private readonly ILogger<SettingsService> _logger;

		public Uri BaseUri { get; set; }
        public PlatformSettings Android { get; init; }
        public PlatformSettings iOS { get; init; }
		private double LowerLimitInMilliseconds { get; set; }
		private double UpperLimitInMilliseconds { get; set; }

		public SettingsService(
            IConfiguration configuration,
            ILogger<SettingsService> logger)
        {
			_logger = logger;
			var settingsConfig = configuration.GetSection("Settings");
            string? baseUrl = settingsConfig["BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogCritical("BaseUrl is not configured in Settings:BaseUrl. Please fix.");
            }

			string requestTimeSpanRangeInMilliseconds = settingsConfig["RequestTimeSpanRangeInMilliseconds"];
			if (string.IsNullOrWhiteSpace(requestTimeSpanRangeInMilliseconds))
			{
				logger.LogCritical("RequestTimeSpanRangeInMilliseconds is not configured in Settings:RequestTimeSpanRangeInMilliseconds. Please fix.");
				requestTimeSpanRangeInMilliseconds = "-180000000:120000";
			}
			var requestTimeSpanRangeInMillisecondsArray = requestTimeSpanRangeInMilliseconds.Split(':');
			if (requestTimeSpanRangeInMillisecondsArray.Length == 2
				&& double.TryParse(requestTimeSpanRangeInMillisecondsArray[0], out double lowerLimitInMilliseconds)
				&& lowerLimitInMilliseconds < 0
				&& double.TryParse(requestTimeSpanRangeInMillisecondsArray[1], out double upperLimitInMilliseconds)
				&& upperLimitInMilliseconds > 0)
			{
				LowerLimitInMilliseconds = lowerLimitInMilliseconds;
				UpperLimitInMilliseconds = upperLimitInMilliseconds;
			}
			else
			{
				logger.LogCritical("RequestTimeSpanRangeInMilliseconds is not configured correctly  Settings:RequestTimeSpanRangeInMilliseconds. {RequestTimeSpanRangeInMilliseconds}", requestTimeSpanRangeInMilliseconds);
			}
			logger.LogInformation("LowerLimitInMilliseconds is {LowerLimitInMilliseconds} & UpperLimitInMilliseconds is {UpperLimitInMilliseconds}", LowerLimitInMilliseconds, UpperLimitInMilliseconds);

			BaseUri = new Uri(baseUrl);

            Android = settingsConfig.GetSection("Android").Get<PlatformSettings>();
            iOS = settingsConfig.GetSection("iOS").Get<PlatformSettings>();
        }

		public bool BeAValidDateWithOffset(DateTimeOffset date) => !date.Equals(default);

		public bool IsFresh(DateTimeOffset date) => IsWithinRange(date.UtcDateTime);

		private bool IsWithinRange(DateTimeOffset date)
		{
			// Calculate the difference in milliseconds
			var difference = (DateTime.UtcNow - date).TotalMilliseconds;

			bool isWithinRange = difference >= LowerLimitInMilliseconds && difference <= UpperLimitInMilliseconds;
			// Check if the difference is outside the range

			if (!isWithinRange)
			{
				_logger.LogCritical("Request {Difference} TotalMilliseconds is not within {LowerLimitInMilliseconds} & {UpperLimitInMilliseconds}", difference, LowerLimitInMilliseconds, UpperLimitInMilliseconds);
			}

			return isWithinRange;
		}
	}
}
