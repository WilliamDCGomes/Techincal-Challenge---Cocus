# WeatherApp

A small weather app. You type a city, it geocodes it and shows the current
conditions plus the next few days, using the free [Open-Meteo](https://open-meteo.com) API.

This is my submission for the .NET MAUI take-home challenge.

## Requirements

- .NET 10 SDK
- MAUI workload — `dotnet workload install maui`
- **Android:** a JDK (17 or 21) and the Android SDK, plus an emulator or a device
- **iOS:** a Mac with Xcode and an iOS Simulator

## Running it

Restore once:

```bash
dotnet restore WeatherApp.sln
```

Android — start an emulator (or plug in a device), then:

```bash
dotnet build src/WeatherApp/WeatherApp.csproj -f net10.0-android -t:Run
```

iOS — boot a simulator first, then:

```bash
dotnet build src/WeatherApp/WeatherApp.csproj -f net10.0-ios -t:Run
```

Or just open `WeatherApp.sln` in Rider / Visual Studio, pick a target and hit run. Then search for
something like "Lisbon".

> On Android Debug builds, deploy with `-t:Run` (or the IDE), not `adb install`. Debug keeps the
> managed assemblies out of the APK ("fast deployment"), so a manually installed APK crashes on
> launch.

## Project layout

```
WeatherApp.sln
├── src/
│   ├── WeatherApp.Core/      # all the logic, no MAUI reference (net10.0)
│   │   ├── Models, Dtos, Enums, Mapping, Serialization
│   │   ├── Services/         # IGeocodingService, IWeatherService, cache, connectivity, ...
│   │   └── ViewModels/       # SearchViewModel, ResultsViewModel, BaseViewModel
│   └── WeatherApp/           # the MAUI app (net10.0-android / net10.0-ios)
│       ├── Views/            # SearchPage, ResultsPage
│       ├── Services/         # platform implementations (navigation, cache, theme, ...)
│       └── Controls, Converters, Handlers, Resources
└── tests/
    └── WeatherApp.Tests/     # xUnit, net10.0 — no emulator, no network
```

## Architecture

I split the solution into a plain class library (`WeatherApp.Core`) and the MAUI head. 
The Core holds everything that isn't UI — models, the Open-Meteo services, mapping, the view models, 
and has no reference to MAUI. That keeps the view models and services testable with a plain `dotnet test` 
and forces a clean boundary. Anything that genuinely needs a platform is an interface in Core with the
implementation in the head:

| Core interface        | Head implementation       |
|-----------------------|---------------------------|
| `INavigationService`  | `ShellNavigationService`  |
| `IWeatherCache`       | `PreferencesWeatherCache` |
| `IConnectivityService`| `ConnectivityService`     |
| `IThemeService`       | `ThemeService`            |

**MVVM** is CommunityToolkit.Mvvm — `[ObservableProperty]` / `[RelayCommand]` instead of hand-written
`INotifyPropertyChanged`. Views are mostly bindings, the code-behind only sets the binding context and
runs a couple of animations.

**DI** uses the `Microsoft.Extensions.DependencyInjection` container MAUI already exposes. `MauiProgram`
registers the platform services and calls `AddWeatherCore()`, which registers the view models and the
two typed `HttpClient`s. Everything is constructor-injected.

**Navigation** is Shell, between two screens (Search → Results). The selected city is passed as a typed
parameter through `IQueryAttributable` instead of a string `[QueryProperty]`, so the view model never
touches a MAUI type.

**State** is a single enum rather than a pile of booleans:

```csharp
enum ViewState { Idle, Loading, Success, Empty, Error }
```

The pages derive visibility from it through one converter, so the states stay mutually exclusive (you
can't be loading and showing an error at once). `BaseViewModel.RunGuardedAsync` wraps every async call
in the same `Loading → Success/Empty → Error` flow and clears the busy flag in a `finally`, so that
plumbing lives in one place instead of being copy-pasted into each command.

**Error handling** maps the messy parts into a typed `ErrorKind` (network / offline / invalid response /
unknown), and each kind becomes a friendly message with a Retry button. The services translate a 5xx,
an Open-Meteo `{"error":true}` body, unparseable JSON or missing fields into those kinds, so the UI
never sees a raw exception. A geocoding miss (HTTP 200 with no results) is an empty "no match" state,
not an error.

## Some specific choices

- **HTTP:** typed `HttpClient`s via `IHttpClientFactory` (no `new HttpClient()`), with
  `AddStandardResilienceHandler()` for retry / timeout / circuit-breaker in one call.
- **JSON:** System.Text.Json with source generation — mostly for trim/AOT friendliness on mobile.
- **Cache: Preferences, not SQLite.** The cached data is tiny (a small JSON blob per city plus a short
  list of recents), so a database felt like overkill. The brief says to pick the simpler option and
  justify it. Recents store the resolved coordinates, so tapping a recent reopens it without
  geocoding again — which is also what makes recents work offline.
- **Offline:** with no connection it serves the last cached forecast for that city behind a small
  banner, and refreshes automatically when the connection comes back. Failures while online are
  surfaced, not hidden behind stale cache. (Realistically the product is online-only; offline was one
  of the optional differentiators, so I added it.)
- **5-day forecast:** the forecast endpoint is queried for 7 days and the UI shows 5 — simplest way to
  satisfy both the suggested 7-day endpoint and the 5-day requirement.
- **Recents chips:** the wrapping row of recent cities uses a small custom `WrapLayout` instead of
  `FlexLayout`. FlexLayout was mis-measuring the chip widths and clipping some names ("Berlin" →
  "Berli"), so I wrote a minimal flow layout that measures each chip at its content width.
- **Orientation** is locked to portrait — it's a phone form factor and there's no landscape layout.
- **Look & feel:** weather icons are emoji and the type uses the system / OpenSans fonts. The brief
  says polish isn't expected, so I didn't bring in an icon font or custom artwork.

## Libraries

- **CommunityToolkit.Mvvm** — the MVVM source generators.
- **Microsoft.Extensions.Http.Resilience** — typed `HttpClient` + the standard resilience handler.
- **Microsoft.Extensions.Logging.Abstractions** — `ILogger<T>` in the services without coupling Core
  to a logging provider.
- **System.Text.Json** (in-box) — serialization.
- **xUnit** + **NSubstitute** — tests and mocking.

## Tests

```bash
dotnet test tests/WeatherApp.Tests/WeatherApp.Tests.csproj
```

The test project is plain `net10.0` — no emulator, no real network, all green. It covers:

- **View models** with the services mocked via NSubstitute: search valid / empty / network / invalid
  flows, the `Loading → Success` and `Loading → Error` transitions, the busy guard, retry, recents
  reorder, and the offline / reconnect paths.
- **The service layer** end to end against a fake `HttpMessageHandler` — so the real services and the
  real JSON run without hitting the network (URL/query building, valid responses, 200-no-results,
  `error:true`, malformed JSON, 5xx, cancellation).
- Plus the WMO code → description map, the DTO→model mapping, and the cache serializer.

## Known limitations / what I'd do next

- Offline serves whatever is cached regardless of age. Fine for availability, but I'd add a
  "last updated" / staleness indicator.
- No city disambiguation: if several cities share a name it takes the top geocoding match. A
  "did you mean…" list would be the obvious next step.
- All UI text is English and there's no localization layer (resx); a real app would localize.
