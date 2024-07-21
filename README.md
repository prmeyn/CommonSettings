# uSignIn.CommonSettings(https://www.nuget.org/packages/uSignIn.CommonSettings)

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
  }
  ```

After the above is done, you can just Dependency inject the `SettingsService` in your C# class.

#### For example:



```csharp
TODO

```

### GitHub Repository
Visit our GitHub repository for the latest updates, documentation, and community contributions.
https://github.com/prmeyn/CommonSettings


## License

This project is licensed under the GNU GENERAL PUBLIC LICENSE.

Happy coding! 🚀🌐📚



