using Microsoft.Extensions.DependencyInjection;
using uSignIn.CommonSettings.Settings;

namespace uSignIn.CommonSettings
{
	public static class SeviceCollectionExtensions
	{
		public static void AddCommonSettingsServices(this IServiceCollection services)
		{
			services.AddSingleton<SettingsService>();
		}
	}
}
