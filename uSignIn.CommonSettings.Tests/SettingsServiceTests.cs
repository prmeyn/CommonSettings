using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using uSignIn.CommonSettings.Settings;

namespace uSignIn.CommonSettings.Tests
{
    public class SettingsServiceTests
    {
        private readonly Mock<ILogger<SettingsService>> _loggerMock;

        public SettingsServiceTests()
        {
            _loggerMock = new Mock<ILogger<SettingsService>>();
        }

        private IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        private static Dictionary<string, string?> ValidSettings(string? range = "-60000:60000") => new()
        {
            { "Settings:BaseUrl", "https://api.example.com" },
            { "Settings:FrontendUrl", "https://app.example.com" },
            { "Settings:RequestTimeSpanRangeInMilliseconds", range }
        };

        private SettingsService BuildService(Dictionary<string, string?> settings) =>
            new(BuildConfiguration(settings), _loggerMock.Object);

        private void VerifyLogged(LogLevel level, string expectedFragment, Times times)
        {
            _loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedFragment)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        [Fact]
        public void Constructor_ShouldLoadSettingsCorrectly()
        {
            // Arrange
            var settings = ValidSettings();
            settings["Settings:Android:Scheme"] = "android-scheme";
            settings["Settings:Android:Host"] = "android-host";
            settings["Settings:iOS:Scheme"] = "ios-scheme";
            settings["Settings:iOS:Host"] = "ios-host";

            // Act
            var service = BuildService(settings);

            // Assert
            Assert.Equal(new Uri("https://api.example.com"), service.BaseUri);
            Assert.Equal(new Uri("https://app.example.com"), service.FrontendUri);
            Assert.Equal("android-scheme", service.Android.Scheme);
            Assert.Equal("android-host", service.Android.Host);
            Assert.Equal("ios-scheme", service.iOS.Scheme);
            Assert.Equal("ios-host", service.iOS.Host);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenConfigurationIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SettingsService(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SettingsService(BuildConfiguration(ValidSettings()), null!));
        }

        [Fact]
        public void Constructor_ShouldLogCriticalAndThrow_WhenBaseUrlIsMissing()
        {
            // Arrange
            var settings = ValidSettings();
            settings.Remove("Settings:BaseUrl");

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => BuildService(settings));

            // Assert
            Assert.Contains("Settings:BaseUrl", exception.Message);
            VerifyLogged(LogLevel.Critical, "BaseUrl is not configured", Times.Once());
        }

        [Fact]
        public void Constructor_ShouldLogCriticalAndThrow_WhenFrontendUrlIsMissing()
        {
            // Arrange
            var settings = ValidSettings();
            settings.Remove("Settings:FrontendUrl");

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => BuildService(settings));

            // Assert
            Assert.Contains("Settings:FrontendUrl", exception.Message);
            VerifyLogged(LogLevel.Critical, "FrontendUrl is not configured", Times.Once());
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenBaseUrlIsNotAbsolute()
        {
            // Arrange
            var settings = ValidSettings();
            settings["Settings:BaseUrl"] = "api/relative-path";

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => BuildService(settings));

            // Assert
            Assert.Contains("Settings:BaseUrl", exception.Message);
            VerifyLogged(LogLevel.Critical, "is not an absolute URL", Times.Once());
        }

        [Fact]
        public void Constructor_ShouldDefaultPlatformSettings_WhenSectionsAreAbsent()
        {
            // Act
            var service = BuildService(ValidSettings());

            // Assert
            Assert.NotNull(service.Android);
            Assert.NotNull(service.iOS);
            Assert.Equal(string.Empty, service.Android.Scheme);
            Assert.Equal(string.Empty, service.Android.Host);
            Assert.Equal(string.Empty, service.iOS.Scheme);
            Assert.Equal(string.Empty, service.iOS.Host);
            VerifyLogged(LogLevel.Warning, "Settings:Android is not configured", Times.Once());
            VerifyLogged(LogLevel.Warning, "Settings:iOS is not configured", Times.Once());
        }

        [Fact]
        public void IsFresh_ShouldReturnTrue_WhenDateIsWithinRange()
        {
            var service = BuildService(ValidSettings()); // +/- 1 minute

            Assert.True(service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(-30)));
        }

        [Fact]
        public void IsFresh_ShouldReturnFalse_WhenDateIsTooOld()
        {
            var service = BuildService(ValidSettings()); // +/- 1 minute

            Assert.False(service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(-61)));
        }

        [Fact]
        public void IsFresh_ShouldReturnFalse_WhenDateIsTooFarInFuture()
        {
            var service = BuildService(ValidSettings()); // +/- 1 minute

            Assert.False(service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(61)));
        }

        [Fact]
        public void IsFresh_ShouldTreatNegativeBoundAsPastTolerance_WhenRangeIsAsymmetric()
        {
            // Arrange: tolerate 10 minutes of age, but only 1 second of future clock skew.
            // A symmetric range cannot detect a sign error, so this asymmetric case is the guard.
            var service = BuildService(ValidSettings("-600000:1000"));
            var now = DateTimeOffset.UtcNow;

            // Act & Assert
            Assert.True(
                service.IsFresh(now.AddMinutes(-5)),
                "a 5-minute-old timestamp must be fresh when 10 minutes of age is tolerated");
            Assert.False(
                service.IsFresh(now.AddMinutes(5)),
                "a timestamp 5 minutes in the future must be rejected when only 1s of skew is tolerated");
        }

        [Fact]
        public void IsFresh_ShouldAcceptZeroLowerBound_WithoutDegradingToRejectAll()
        {
            // Arrange: no age tolerance at all, 2 minutes of future skew. The previous
            // implementation required lower < 0 and silently rejected every request here.
            var service = BuildService(ValidSettings("0:120000"));
            var now = DateTimeOffset.UtcNow;

            // Act & Assert
            Assert.True(service.IsFresh(now.AddSeconds(30)));
            Assert.False(service.IsFresh(now.AddSeconds(-30)));
        }

        [Theory]
        [InlineData("garbage")]
        [InlineData("1:2:3")]
        [InlineData("abc:def")]
        [InlineData("5:-5")] // lower > upper
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_ShouldFallBackToDefaultRange_WhenRangeIsMissingOrMalformed(string? range)
        {
            // Act
            var service = BuildService(ValidSettings(range));
            var now = DateTimeOffset.UtcNow;

            // Assert: the documented default tolerates 50h of age and 2 minutes of future skew.
            // A malformed value must not leave the window at 0:0, which would reject everything.
            Assert.True(
                service.IsFresh(now.AddMinutes(-5)),
                "malformed configuration must fall back to the documented default, not reject every request");
            Assert.False(service.IsFresh(now.AddMinutes(-5000)));
            Assert.False(service.IsFresh(now.AddMinutes(5)));
            VerifyLogged(LogLevel.Critical, "RequestTimeSpanRangeInMilliseconds", Times.Once());
        }

        [Fact]
        public void Constructor_ShouldParseRangeInvariantly_WhenCurrentCultureUsesCommaDecimalSeparator()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("da-DK");

                // Guard the premise: if the host lacks full ICU data this test would silently
                // stop exercising culture-sensitive parsing.
                Assert.Equal(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                var service = BuildService(ValidSettings("-60000:60000"));

                Assert.True(service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(-30)));
                Assert.False(service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(-61)));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void Constructor_ShouldNotTreatDotAsGroupSeparator_WhenCurrentCultureIsDanish()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("da-DK");
                Assert.Equal(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                // Parsed invariantly, "-60.000:60.000" is a +/- 60 MILLISECOND window.
                // Parsed with Danish conventions and AllowThousands it would be +/- 60 SECONDS.
                var service = BuildService(ValidSettings("-60.000:60.000"));

                Assert.False(
                    service.IsFresh(DateTimeOffset.UtcNow.AddSeconds(-1)),
                    "a 1s-old timestamp must fall outside a +/- 60ms window; if it is fresh, the value was parsed with a culture-specific group separator");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void BeAValidDateWithOffset_ShouldReturnFalse_ForDefaultValue()
        {
            var service = BuildService(ValidSettings());

            Assert.False(service.BeAValidDateWithOffset(default));
        }

        [Fact]
        public void BeAValidDateWithOffset_ShouldReturnTrue_ForRealTimestamp()
        {
            var service = BuildService(ValidSettings());

            Assert.True(service.BeAValidDateWithOffset(DateTimeOffset.UtcNow));
        }
    }
}
