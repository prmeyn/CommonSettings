# uSignIn.CommonSettings

**uSignIn.CommonSettings** is an open-source C# class library that provides a wrapper around Common settings in the `appsettings.json`

## Features

- All common settings that need to used by your project or packages can be accessed via this service


## Contributing

We welcome contributions! If you find a bug, have an idea for improvement, please submit an issue or a pull request on GitHub.

## Getting Started

### [NuGet Package](https://www.nuget.org/packages/uSignIn.CommonSettings)

To include **uSignIn.CommonSettings** in your project, [install the NuGet package](https://www.nuget.org/packages/uSignIn.CommonSettings):

```bash
dotnet add package uSignIn.CommonSettings
```
Then in your `appsettings.json` add the following sample configuration and change the values to match the details of your credentials to the various services.
```json
  "Settings": {
    "BaseUrl": "https://[YOUR-HOSTNAME]",
    "RequestTimeSpanRangeInMilliseconds": "-120000:120000"
  }
  ```
The `RequestTimeSpanRangeInMilliseconds` example above means that all communication to the server cannot be replayed, as the server timestamp needs to match the client timestamp allowed within a range of -120 seconds to 120 seconds.

After the above is done, you can just Dependency inject the `SettingsService` in your C# class.

#### For example:

Here is a Sample Service `SampleService` C# class that injects the `SettingsService` and then uses the `_settingsService.BaseUri`

```csharp
using uSignIn.CommonSettings.Settings;

namespace Test.Stuff
{
	public sealed class SampleService
	{
		private readonly SettingsService _settingsService;

		public SampleService(
			SettingsService settingsService
		{
			_settingsService = settingsService;
		}
		internal string NotificationUrl => new Uri(_settingsService.BaseUri, "/helloWorld").ToString();
	}
}
```

### GitHub Repository
Visit our GitHub repository for the latest updates, documentation, and community contributions.
https://github.com/prmeyn/CommonSettings


## License

This project is licensed under the GNU GENERAL PUBLIC LICENSE.

Happy coding! 🚀🌐📚



