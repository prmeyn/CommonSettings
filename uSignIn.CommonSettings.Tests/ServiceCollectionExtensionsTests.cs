using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using uSignIn.CommonSettings.Settings;

namespace uSignIn.CommonSettings.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private static IServiceCollection BuildServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Settings:BaseUrl", "https://api.example.com" },
                    { "Settings:FrontendUrl", "https://app.example.com" },
                    { "Settings:RequestTimeSpanRangeInMilliseconds", "-60000:60000" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILogger<SettingsService>>(NullLogger<SettingsService>.Instance);
            return services;
        }

        [Fact]
        public void AddCommonSettingsServices_ShouldRegisterResolvableSettingsService()
        {
            // Arrange
            var services = BuildServices();

            // Act
            services.AddCommonSettingsServices();
            using var provider = services.BuildServiceProvider();
            var settings = provider.GetRequiredService<SettingsService>();

            // Assert
            Assert.Equal(new Uri("https://api.example.com"), settings.BaseUri);
        }

        [Fact]
        public void AddCommonSettingsServices_ShouldRegisterAsSingleton()
        {
            // Arrange
            var services = BuildServices();
            services.AddCommonSettingsServices();
            using var provider = services.BuildServiceProvider();

            // Act
            var first = provider.GetRequiredService<SettingsService>();
            var second = provider.GetRequiredService<SettingsService>();

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void AddCommonSettingsServices_ShouldReturnSameCollection_ForChaining()
        {
            // Arrange
            var services = BuildServices();

            // Act & Assert
            Assert.Same(services, services.AddCommonSettingsServices());
        }

        [Fact]
        public void AddCommonSettingsServices_ShouldRegisterOnce_WhenCalledTwice()
        {
            // Arrange
            var services = BuildServices();

            // Act
            services.AddCommonSettingsServices();
            services.AddCommonSettingsServices();

            // Assert
            Assert.Equal(1, services.Count(descriptor => descriptor.ServiceType == typeof(SettingsService)));
        }
    }
}
