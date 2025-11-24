# uSignIn.CommonSettings

**uSignIn.CommonSettings** is a robust C# class library designed to centralize configuration management and provide essential validation logic for the uSignIn ecosystem. It acts as a strongly-typed wrapper around `appsettings.json`, ensuring consistency and safety across your projects.

## Features

- **Centralized Configuration**: Strongly-typed access to common settings like API URLs, frontend endpoints, and platform-specific configurations (Android/iOS).
- **Request Freshness Validation**: Built-in logic to validate request timestamps, helping to prevent replay attacks by enforcing a configurable time window.
- **History Tracking**: Generic `History<T>` DTOs and extensions to easily track and retrieve the latest values from a time-ordered sequence.
- **Platform Support**: dedicated configuration sections for Android and iOS specific settings (Scheme, Host).

## Installation

Install the NuGet package via the .NET CLI:

```bash
dotnet add package uSignIn.CommonSettings
```

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
  - Format: `"LowerLimit:UpperLimit"` (in milliseconds).
  - Example `"-120000:120000"` means a request timestamp is valid if it is between **2 minutes in the past** and **2 minutes in the future**.
- **`Android` / `iOS`**: Platform-specific settings used for deep linking or redirects.

## Usage

### 1. Settings Service

Register the service in your Dependency Injection container (if not already handled by a startup extension) and inject `SettingsService` into your classes.

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
            throw new Exception("Request is expired or invalid.");
        }
    }
}
```

### 2. History Tracking

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

We welcome contributions! If you find a bug or have an idea for improvement, please submit an issue or a pull request on GitHub.

## License

This project is licensed under the GNU GENERAL PUBLIC LICENSE.

Happy coding! 🚀🌐📚
