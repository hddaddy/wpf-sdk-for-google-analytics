# WPF SDK for Google Analytics™

This is ported for WPF from [Windows SDK for Google Analytics](https://github.com/dotnet/windows-sdk-for-google-analytics).  

## Target Framework
.NET Framework 4 Client Profile

## Get Started

Add references into your WPF application:
* GoogleAnalytics.Core.dll
* GoogleAnalytics.WPF.dll


Add this code below in App.xaml.cs
```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    AnalyticsManager.Current.ReportUncaughtExceptions = true;
    AnalyticsManager.Current.DispatchPeriod = TimeSpan.Zero; 

    App.Tracker = AnalyticsManager.Current.CreateTracker("<Your-GA-Id>");
}

```

Add this code below somewhere for example
```csharp
App.Tracker.Send(HitBuilder.CreateScreenView("MyWindow").Build());
```



