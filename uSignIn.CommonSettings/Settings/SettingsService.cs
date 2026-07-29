using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace uSignIn.CommonSettings.Settings
{
    /// <summary>
    /// Strongly-typed access to the shared <c>Settings</c> configuration section, together with
    /// request-freshness validation used to bound the replay window for incoming requests.
    /// </summary>
    /// <remarks>
    /// Configuration is read once when the instance is constructed, so changes to
    /// <see cref="IConfiguration"/> after startup are not picked up until the host restarts.
    /// </remarks>
    public sealed class SettingsService
    {
        private const string RequestTimeSpanRangeKey = "RequestTimeSpanRangeInMilliseconds";

        /// <summary>
        /// Age tolerance used when <c>Settings:RequestTimeSpanRangeInMilliseconds</c> is missing or
        /// malformed: timestamps up to 50 hours old are accepted.
        /// </summary>
        private const double DefaultLowerLimitInMilliseconds = -180_000_000;

        /// <summary>
        /// Future-skew tolerance used when <c>Settings:RequestTimeSpanRangeInMilliseconds</c> is
        /// missing or malformed: timestamps up to 2 minutes ahead of server time are accepted.
        /// </summary>
        private const double DefaultUpperLimitInMilliseconds = 120_000;

        private readonly ILogger<SettingsService> _logger;
        private readonly double _lowerLimitInMilliseconds;
        private readonly double _upperLimitInMilliseconds;

        /// <summary>Root URL of the frontend application, from <c>Settings:FrontendUrl</c>.</summary>
        public Uri FrontendUri { get; }

        /// <summary>Root URL of the backend API, from <c>Settings:BaseUrl</c>.</summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// Android deep-link settings from <c>Settings:Android</c>. Never <see langword="null"/>;
        /// <see cref="PlatformSettings.Scheme"/> and <see cref="PlatformSettings.Host"/> are empty
        /// when the section is absent.
        /// </summary>
        public PlatformSettings Android { get; }

        /// <summary>
        /// iOS deep-link settings from <c>Settings:iOS</c>. Never <see langword="null"/>;
        /// <see cref="PlatformSettings.Scheme"/> and <see cref="PlatformSettings.Host"/> are empty
        /// when the section is absent.
        /// </summary>
        public PlatformSettings iOS { get; }

        /// <summary>
        /// Reads and validates the <c>Settings</c> configuration section.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Logger used to report configuration problems.</param>
        /// <exception cref="InvalidOperationException">
        /// <c>Settings:BaseUrl</c> or <c>Settings:FrontendUrl</c> is missing, blank, or not an
        /// absolute URL.
        /// </exception>
        public SettingsService(
            IConfiguration configuration,
            ILogger<SettingsService> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;
            var settingsConfig = configuration.GetSection("Settings");

            // Read everything and log every problem before throwing, so a single startup attempt
            // surfaces all misconfiguration rather than just the first fault.
            var baseUri = ReadAbsoluteUriOrLog(settingsConfig, "BaseUrl");
            var frontendUri = ReadAbsoluteUriOrLog(settingsConfig, "FrontendUrl");

            (_lowerLimitInMilliseconds, _upperLimitInMilliseconds) = ReadRequestTimeSpanRange(settingsConfig);
            _logger.LogInformation(
                "Request freshness window is {LowerLimitInMilliseconds}ms to {UpperLimitInMilliseconds}ms relative to server time.",
                _lowerLimitInMilliseconds,
                _upperLimitInMilliseconds);

            BaseUri = baseUri ?? throw MissingUrl("BaseUrl", "https://api.example.com");
            FrontendUri = frontendUri ?? throw MissingUrl("FrontendUrl", "https://app.example.com");

            Android = ReadPlatformSettings(settingsConfig, "Android");
            iOS = ReadPlatformSettings(settingsConfig, "iOS");
        }

        /// <summary>
        /// Determines whether <paramref name="date"/> is a real timestamp rather than the default
        /// <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="date">The timestamp to check.</param>
        /// <returns><see langword="true"/> when <paramref name="date"/> is not <c>default</c>.</returns>
        public bool BeAValidDateWithOffset(DateTimeOffset date) => !date.Equals(default);

        /// <summary>
        /// Determines whether <paramref name="date"/> falls inside the configured freshness window,
        /// logging at critical level when it does not.
        /// </summary>
        /// <param name="date">The request timestamp to validate.</param>
        /// <returns><see langword="true"/> when the timestamp is within the configured window.</returns>
        public bool IsFresh(DateTimeOffset date)
        {
            // Offset from server time: negative when date is in the past, positive when ahead of it.
            var offsetInMilliseconds = (date - DateTimeOffset.UtcNow).TotalMilliseconds;

            var isWithinRange = offsetInMilliseconds >= _lowerLimitInMilliseconds
                && offsetInMilliseconds <= _upperLimitInMilliseconds;

            if (!isWithinRange)
            {
                _logger.LogCritical(
                    "Request offset {OffsetInMilliseconds}ms is not within {LowerLimitInMilliseconds}ms & {UpperLimitInMilliseconds}ms",
                    offsetInMilliseconds,
                    _lowerLimitInMilliseconds,
                    _upperLimitInMilliseconds);
            }

            return isWithinRange;
        }

        private static InvalidOperationException MissingUrl(string key, string example) =>
            new($"Settings:{key} is missing or is not an absolute URL. Configure it with a value such as \"{example}\".");

        private Uri? ReadAbsoluteUriOrLog(IConfiguration settingsConfig, string key)
        {
            var value = settingsConfig[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("{Key} is not configured in Settings:{Key}. Please fix.", key, key);
                return null;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                _logger.LogCritical("Settings:{Key} is not an absolute URL. {Value}", key, value);
                return null;
            }

            return uri;
        }

        /// <summary>
        /// Parses <c>Settings:RequestTimeSpanRangeInMilliseconds</c> in the form
        /// <c>"lower:upper"</c>, falling back to the documented defaults when it is missing or
        /// malformed. Parsing is culture-invariant so the same configuration behaves identically on
        /// every host.
        /// </summary>
        private (double LowerLimitInMilliseconds, double UpperLimitInMilliseconds) ReadRequestTimeSpanRange(
            IConfiguration settingsConfig)
        {
            var value = settingsConfig[RequestTimeSpanRangeKey];

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical(
                    "{Key} is not configured in Settings:{Key}. Falling back to {LowerLimitInMilliseconds}:{UpperLimitInMilliseconds}.",
                    RequestTimeSpanRangeKey,
                    RequestTimeSpanRangeKey,
                    DefaultLowerLimitInMilliseconds,
                    DefaultUpperLimitInMilliseconds);

                return (DefaultLowerLimitInMilliseconds, DefaultUpperLimitInMilliseconds);
            }

            var parts = value.Split(':');
            if (parts.Length == 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lowerLimitInMilliseconds)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var upperLimitInMilliseconds)
                && lowerLimitInMilliseconds <= upperLimitInMilliseconds)
            {
                return (lowerLimitInMilliseconds, upperLimitInMilliseconds);
            }

            _logger.LogCritical(
                "Settings:{Key} is not configured correctly; expected \"lower:upper\" in milliseconds with lower <= upper. {Value}. Falling back to {LowerLimitInMilliseconds}:{UpperLimitInMilliseconds}.",
                RequestTimeSpanRangeKey,
                value,
                DefaultLowerLimitInMilliseconds,
                DefaultUpperLimitInMilliseconds);

            return (DefaultLowerLimitInMilliseconds, DefaultUpperLimitInMilliseconds);
        }

        private PlatformSettings ReadPlatformSettings(IConfiguration settingsConfig, string key)
        {
            var platformSettings = settingsConfig.GetSection(key).Get<PlatformSettings>();

            if (platformSettings is null)
            {
                _logger.LogWarning(
                    "Settings:{Key} is not configured. Scheme and Host will be empty.",
                    key);

                return new PlatformSettings();
            }

            return platformSettings;
        }
    }
}
