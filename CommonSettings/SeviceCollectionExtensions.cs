using CommonSettings.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace CommonSettings
{
	public static class SeviceCollectionExtensions
	{
		public static void AddCommonSettingsServices(this IServiceCollection services)
		{
			services.AddSingleton<SettingsService>();
		}
	}
}
