# uSignIn.CommonSettings

[![NuGet](https://img.shields.io/nuget/v/uSignIn.CommonSettings.svg)](https://www.nuget.org/packages/uSignIn.CommonSettings)
[![GitHub](https://img.shields.io/badge/github-CommonSettings-blue.svg)](https://github.com/prmeyn/CommonSettings)

**uSignIn.CommonSettings** is a robust C# class library designed to centralize configuration management and provide essential validation logic for the uSignIn ecosystem. It acts as a strongly-typed wrapper around `appsettings.json`, ensuring consistency and safety across your projects.

## Features

- **Centralized Configuration**: Strongly-typed access to common settings like API URLs, frontend endpoints, and platform-specific configurations (Android/iOS).
- **Request Freshness Validation**: Built-in logic to validate request timestamps, helping to prevent replay attacks by enforcing a configurable time window.
- **History Tracking**: Generic `History<T>` DTOs and extensions to easily track and retrieve the latest values from a time-ordered sequence.
- **Platform Support**: dedicated configuration sections for Android and iOS specific settings (Scheme, Host).

## Installation

Install the [NuGet package](https://www.nuget.org/packages/uSignIn.CommonSettings) via the .NET CLI:

```bash
dotnet add package uSignIn.CommonSettings
```

Or via the NuGet Package Manager in Visual Studio.

## Configuration

Add the following `Settings` section to your `appsettings.json`. Adjust the values to match your environment.

```json
{
  "Settings": {
    "BaseUrl": "https://api.yourdomain.com",
    "FrontendUrl": "https://app.yourdomain.com",
    "RequestTimeSpanRangeInMilliseconds": "-120000:120000",
    "Android": {
      "Scheme": "usignin",
      "Host": "callback"
    },
    "iOS": {
      "Scheme": "usignin",
      "Host": "callback"
    }
  }
}
```

### Configuration Breakdown

- **`BaseUrl`**: The root URL for your backend API.
- **`FrontendUrl`**: The root URL for your frontend application.
- **`RequestTimeSpanRangeInMilliseconds`**: Defines the valid time window for requests relative to the server time.
  - Format: `"LowerLimit:UpperLimit"` (in milliseconds). Both limits are offsets of the request timestamp from server time: a **negative** limit lies in the past, a **positive** limit in the future. `LowerLimit` must be less than or equal to `UpperLimit`.
  - Example `"-120000:120000"` means a request timestamp is valid if it is between **2 minutes in the past** and **2 minutes in the future**.
  - Asymmetric windows are supported: `"-600000:1000"` tolerates **10 minutes of age** but only **1 second of future clock skew**.
  - Values are parsed with the invariant culture, so the same configuration behaves identically on every host regardless of locale. Do not use group separators — `"-60.000"` is read as -60 milliseconds, not -60000.
  - If the value is missing or malformed, a critical message is logged and the default `"-180000000:120000"` (50 hours of age, 2 minutes of future skew) is used.
- **`Android` / `iOS`**: Platform-specific settings used for deep linking or redirects. Optional — when a section is absent a warning is logged and `Scheme`/`Host` are empty strings rather than `null`.

## Usage

### 1. Registering the services

Call `AddCommonSettingsServices()` on your service collection during startup. It registers `SettingsService` as a singleton and is safe to call more than once.

```csharp
using uSignIn.CommonSettings;

builder.Services.AddCommonSettingsServices();
```

`SettingsService` reads and validates configuration when it is first constructed, throwing `InvalidOperationException` if `Settings:BaseUrl` or `Settings:FrontendUrl` is missing or is not an absolute URL.

### 2. Settings Service

Inject `SettingsService` into your classes.

```csharp
using uSignIn.CommonSettings.Settings;

public class MyService
{
    private readonly SettingsService _settings;

    public MyService(SettingsService settings)
    {
        _settings = settings;
    }

    public void DoWork()
    {
        // Access URLs
        var apiUrl = _settings.BaseUri;
        
        // Access Platform Settings
        var androidScheme = _settings.Android.Scheme;
    }

    public void ValidateRequest(DateTimeOffset requestTime)
    {
        // Check if the request is within the allowed time window
        if (!_settings.IsFresh(requestTime))
        {
            throw new InvalidOperationException("Request is expired or invalid.");
        }
    }
}
```

### 3. History Tracking

Use the `History<T>` DTO and `HistoryExtensions` to manage historical data.

```csharp
using uSignIn.CommonSettings.DTOs;

var loginHistory = new List<History<string>>
{
    new History<string> { Value = "Login_Attempt_1", TimeStamp = DateTimeOffset.UtcNow.AddMinutes(-10) },
    new History<string> { Value = "Login_Success", TimeStamp = DateTimeOffset.UtcNow }
};

// Get the value of the most recent entry
string? latestStatus = loginHistory.LatestValue(); // Returns "Login_Success"

// Get the entire latest record
History<string>? latestRecord = loginHistory.LatestRecord();
```

## Contributing

We welcome contributions! If you find a bug or have an idea for improvement, please submit an issue or a pull request on our [GitHub repository](https://github.com/prmeyn/CommonSettings).

## License

This project is licensed under the GNU GENERAL PUBLIC LICENSE.

Happy coding! 🚀🌐📚
