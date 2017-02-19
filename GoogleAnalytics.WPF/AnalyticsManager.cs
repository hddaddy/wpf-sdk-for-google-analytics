using System;
using System.Windows;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;

namespace GoogleAnalytics
{
    /// <summary>
    /// Provides shared/platform specific infrastrcuture for GoogleAnalytics.Core Tracker
    /// </summary>
    public sealed class AnalyticsManager : TrackerManager
    {
        const string Key_AppOptOut = "GoogleAnaltyics.AppOptOut";

        static AnalyticsManager current;

        bool isAppOptOutSet;
        Application application;
        bool reportUncaughtExceptions;
        bool autoTrackNetworkConnectivity;

        /// <summary>
        /// Instantiates a new instance of <see cref="AnalyticsManager"/> 
        /// </summary>
        /// <param name="platformInfoProvider"> The platform info provider to be used by this Analytics Manager. Can not be null.</param>
        public AnalyticsManager(IPlatformInfoProvider platformInfoProvider) : base(platformInfoProvider)
        {
            this.application = Application.Current;
        }

        AnalyticsManager(Application application) : base(new PlatformInfoProvider())
        {
            this.application = application;
        }

        /// <summary>
        /// Shared, singleton instance of AnalyticsManager 
        /// </summary>
        public static AnalyticsManager Current
        {
            get
            {
                if (current == null)
                {
                    current = new AnalyticsManager(Application.Current);
                }
                return current;
            }
        }

        /// <summary>
        /// True when the user has opted out of analytics, this disables all tracking activities.
        /// </summary>
        /// <remarks>See Google Analytics usage guidelines for more information.</remarks>
        public override bool AppOptOut
        {
            get
            {
                if (!isAppOptOutSet) LoadAppOptOut();
                return base.AppOptOut;
            }
            set
            {
                base.AppOptOut = value;
                isAppOptOutSet = true;

                ConfigurationManager.AppSettings[Key_AppOptOut] = isAppOptOutSet.ToString();
                if (value) Clear();
            }
        }

        /// <summary>
        /// Enables (when set to true) automatic catching and tracking of Unhandled Exceptions.
        /// </summary>
        public bool ReportUncaughtExceptions
        {
            get
            {
                return reportUncaughtExceptions;
            }
            set
            {
                if (reportUncaughtExceptions != value)
                {
                    reportUncaughtExceptions = value;
                    if (reportUncaughtExceptions)
                    {
                        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                        Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                    }
                    else
                    {
                        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
                        Application.Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
                    }
                }
            }
        }


        /// <summary>
        /// Enables (when set to true) listening to network connectivity events to have trackers behave accordingly to their connectivity status.
        /// </summary>
        public bool AutoTrackNetworkConnectivity
        {
            get
            {
                return autoTrackNetworkConnectivity;
            }
            set
            {
                if (autoTrackNetworkConnectivity != value)
                {
                    autoTrackNetworkConnectivity = value;
                    if (autoTrackNetworkConnectivity)
                    {
                        UpdateConnectionStatus();
                        NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                    }
                    else
                    {
                        NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
                        base.IsEnabled = true;
                    }
                }
            }
        }

        private void UpdateConnectionStatus()
        {
            // TODO:
            IsEnabled = true;
        }

        /// <summary>
        /// Creates a new Tracker using a given property ID. 
        /// </summary>
        /// <param name="propertyId">The property ID that the <see cref="Tracker"/> should log to.</param>
        /// <returns>The new or existing instance keyed on the property ID.</returns>
        public override Tracker CreateTracker(string propertyId)
        {
            var tracker = base.CreateTracker(propertyId);
            return tracker;
        }

        /// <summary>
        /// Creates a new Tracker using a given property ID. 
        /// </summary>
        /// <param name="propertyId">The property ID that the <see cref="Tracker"/> should log to.</param>
        /// <param name="appName"></param>
        /// <param name="appVersion">ex. 1.4.5.123</param>
        /// <returns>The new or existing instance keyed on the property ID.</returns>
        public Tracker CreateTracker(string propertyId, string appName, string appVersion)
        {
            var tracker = this.CreateTracker(propertyId);
            tracker.AppName = appName;
            tracker.AppVersion = appVersion;
            return tracker;
        }

        void LoadAppOptOut()
        {
            if (ConfigurationManager.AppSettings.AllKeys.FirstOrDefault(c => c == Key_AppOptOut) != null)
            {
                base.AppOptOut = bool.Parse(ConfigurationManager.AppSettings[Key_AppOptOut]);
            }
            else
            {
                base.AppOptOut = false;
            }
            isAppOptOutSet = true;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            IsEnabled = e.IsAvailable;
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception.InnerException ?? e.Exception; // inner exception contains better info for unobserved tasks
            foreach (var tracker in Trackers)
            {
                tracker.Send(HitBuilder.CreateException(ex.ToString(), false).Build());
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            foreach (var tracker in Trackers)
            {
                tracker.Send(HitBuilder.CreateException(ex.Message, true).Build());
            }
            var t = DispatchAsync();
        }
    }
}
