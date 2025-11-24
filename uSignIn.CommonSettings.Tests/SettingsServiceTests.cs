using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using uSignIn.CommonSettings.Settings;
using Xunit;

namespace uSignIn.CommonSettings.Tests
{
    public class SettingsServiceTests
    {
        private readonly Mock<ILogger<SettingsService>> _loggerMock;

        public SettingsServiceTests()
        {
            _loggerMock = new Mock<ILogger<SettingsService>>();
        }

        private IConfiguration BuildConfiguration(Dictionary<string, string> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void Constructor_ShouldLoadSettingsCorrectly()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "Settings:BaseUrl", "https://api.example.com" },
                { "Settings:FrontendUrl", "https://app.example.com" },
                { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" },
                { "Settings:Android:Scheme", "android-scheme" },
                { "Settings:Android:Host", "android-host" },
                { "Settings:iOS:Scheme", "ios-scheme" },
                { "Settings:iOS:Host", "ios-host" }
            };
            var configuration = BuildConfiguration(settings);

            // Act
            var service = new SettingsService(configuration, _loggerMock.Object);

            // Assert
            Assert.Equal(new Uri("https://api.example.com"), service.BaseUri);
            Assert.Equal(new Uri("https://app.example.com"), service.FrontendUri);
            Assert.Equal("android-scheme", service.Android.Scheme);
            Assert.Equal("android-host", service.Android.Host);
            Assert.Equal("ios-scheme", service.iOS.Scheme);
            Assert.Equal("ios-host", service.iOS.Host);
        }

        [Fact]
        public void Constructor_ShouldLogCritical_WhenBaseUrlIsMissing()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "Settings:FrontendUrl", "https://app.example.com" },
                { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" }
            };
            var configuration = BuildConfiguration(settings);

            // Act
            try
            {
                var service = new SettingsService(configuration, _loggerMock.Object);
            }
            catch (Exception ex) when (ex is UriFormatException || ex is ArgumentNullException)
            {
                // Expected because BaseUri will try to parse null or invalid string
            }

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("BaseUrl is not configured")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsFresh_ShouldReturnTrue_WhenDateIsWithinRange()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "Settings:BaseUrl", "https://api.example.com" },
                { "Settings:FrontendUrl", "https://app.example.com" },
                { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" } // +/- 1 minute
            };
            var configuration = BuildConfiguration(settings);
            var service = new SettingsService(configuration, _loggerMock.Object);
            var now = DateTimeOffset.UtcNow;

            // Act
            var result = service.IsFresh(now.AddSeconds(-30)); // 30 seconds ago

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsFresh_ShouldReturnFalse_WhenDateIsTooOld()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "Settings:BaseUrl", "https://api.example.com" },
                { "Settings:FrontendUrl", "https://app.example.com" },
                { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" } // +/- 1 minute
            };
            var configuration = BuildConfiguration(settings);
            var service = new SettingsService(configuration, _loggerMock.Object);
            var now = DateTimeOffset.UtcNow;

            // Act
            var result = service.IsFresh(now.AddSeconds(-61)); // 61 seconds ago

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsFresh_ShouldReturnFalse_WhenDateIsTooFarInFuture()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "Settings:BaseUrl", "https://api.example.com" },
                { "Settings:FrontendUrl", "https://app.example.com" },
                { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" } // +/- 1 minute
            };
            var configuration = BuildConfiguration(settings);
            var service = new SettingsService(configuration, _loggerMock.Object);
            var now = DateTimeOffset.UtcNow;

            // Act
            var result = service.IsFresh(now.AddSeconds(61)); // 61 seconds in future

            // Assert
            Assert.False(result);
        }
    }
}
