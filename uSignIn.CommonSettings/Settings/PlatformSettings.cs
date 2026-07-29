namespace uSignIn.CommonSettings.Settings
{
    /// <summary>
    /// Deep-link settings for a single mobile platform, bound from <c>Settings:Android</c> or
    /// <c>Settings:iOS</c>.
    /// </summary>
    public sealed class PlatformSettings
    {
        /// <summary>
        /// Custom URL scheme used for deep links. Empty when the platform section is not configured.
        /// </summary>
        public string Scheme { get; set; } = string.Empty;

        /// <summary>
        /// Deep-link host. Empty when the platform section is not configured.
        /// </summary>
        public string Host { get; set; } = string.Empty;
    }
}
