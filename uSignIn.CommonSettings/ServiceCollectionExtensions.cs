using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using uSignIn.CommonSettings.Settings;

namespace uSignIn.CommonSettings
{
	/// <summary>
	/// Registration helpers for the uSignIn common settings services.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers <see cref="SettingsService"/> as a singleton. Calling this more than once is
		/// safe: the registration is only added when it is not already present.
		/// </summary>
		/// <param name="services">The service collection to add the registration to.</param>
		/// <returns>The same <paramref name="services"/> instance, to allow chaining.</returns>
		public static IServiceCollection AddCommonSettingsServices(this IServiceCollection services)
		{
			services.TryAddSingleton<SettingsService>();
			return services;
		}
	}
}
