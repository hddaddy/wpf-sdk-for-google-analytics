using System;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace GoogleAnalytics
{
    /// <summary>
    /// Windows 10, Universal Platform implementation of GoogleAnalytics.Core.IPlatformInfoProvider.
    /// </summary>
    public sealed class PlatformInfoProvider : IPlatformInfoProvider
    {
        const string Key_AnonymousClientId = "GoogleAnaltyics.AnonymousClientId";

        static string userAgent;

        bool windowInitialized = false;
        string anonymousClientId;
        Dimensions? viewPortResolution;
        Dimensions? screenResolution;
         
        /// <inheritdoc /> 
        public event EventHandler ViewPortResolutionChanged;
        /// <inheritdoc /> 
        public event EventHandler ScreenResolutionChanged;

        public PlatformInfoProvider()
        {
            InitializeWindow();
        }

        /// <inheritdoc /> 
        public void OnTracking()
        {
            if (!windowInitialized)
            {
                InitializeWindow();
            }
        }

        /// <inheritdoc /> 
        public string AnonymousClientId
        {
            get
            {
                if (anonymousClientId == null)
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    if (appSettings.AllKeys.FirstOrDefault(c => c == Key_AnonymousClientId) == null)
                    {
                        anonymousClientId = Guid.NewGuid().ToString();
                        appSettings[Key_AnonymousClientId] = anonymousClientId;
                    }
                    else
                    {
                        anonymousClientId = (string)appSettings[Key_AnonymousClientId];
                    }
                }
                return anonymousClientId;
            }
            set { anonymousClientId = value; }
        }

        /// <inheritdoc /> 
        public Dimensions? ViewPortResolution
        {
            get { return viewPortResolution; }
            private set
            {
                viewPortResolution = value;
                ViewPortResolutionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc /> 
        public Dimensions? ScreenResolution
        {
            get { return screenResolution; }
            private set
            {
                screenResolution = value;
                ScreenResolutionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc /> 
        public string UserLanguage
        {
            get { return System.Globalization.CultureInfo.CurrentUICulture.Name; }
        }

        /// <inheritdoc /> 
        /// <remarks>This feature not implemented on this UWP implementation </remarks>
        public int? ScreenColors
        {
            get { return null; }
        }

        /// <inheritdoc /> 
        public string UserAgent
        {
            get
            {
                if (userAgent == null)
                {
                    userAgent = GetUserAgent();
                }
                return userAgent;
            }
        }

        void InitializeWindow()
        {
            try
            {
                var window = Application.Current.MainWindow;

                if (window != null && window.Content != null)
                {
                    var bounds = new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight);
                    double w, h;
                    w = bounds.Width;
                    h = bounds.Height;

                    // DPI 배율인수 고려
                    //var scale = (double)(int)displayInfo.ResolutionScale / 100d;
                    //w = Math.Round(w * scale);
                    //h = Math.Round(h * scale);

                    ScreenResolution = new Dimensions((int)w, (int)h);
                    ViewPortResolution = new Dimensions((int)bounds.Width, (int)bounds.Height); // leave viewport at the scale unadjusted size
                    window.SizeChanged += Window_SizeChanged;
                    windowInitialized = true;
                }
            }
            catch { /* ignore, Bounds may not be ready yet */ }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewPortResolution = new Dimensions((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        static string GetUserAgent()
        {
            string systemVersion = Environment.OSVersion.VersionString;

            String uaArchitecture;
            if (Environment.Is64BitOperatingSystem == true)
            {
                uaArchitecture = "Win64; X64";
            }
            else
            {
                uaArchitecture = "Win32; X86";
            }

            return $"Mozilla/5.0 (Windows NT {systemVersion}; {uaArchitecture})";
        }
    }
}
