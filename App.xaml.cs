/*
 *  Copyright © 2016, Russell Libby
 */

using DevicePower.Pages;
using DevicePower.Theme;
using DevicePowerCommon;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DevicePower
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : INotifyPropertyChanged
    {
        #region Private fields

        private TransitionCollection _transitions;
        private DisplayRequest _displayRequest;
        private bool _syncing = true;
        private bool _paired;
        private bool _tileAdded;

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if we can sync to and/or remove the band tile.
        /// </summary>
        /// <returns>True if connected to a band tile and not currently syncing.</returns>
        private bool CanSyncOrRemove()
        {
            return (_paired && _tileAdded && !_syncing);
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;

            if (rootFrame == null) throw new ArgumentException(@"RootFrame is null.");

            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

#if DEBUG
            if (_displayRequest != null)
            {
                _displayRequest.RequestRelease();
                _displayRequest = null;
            }
#endif
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when application execution is being resumed. 
        /// </summary>
        /// <param name="sender">The source of the resume request.</param>
        /// <param name="e">The object associated with the event.</param>
        private void OnResuming(object sender, object e)
        {
#if DEBUG
            if (_displayRequest == null)
            {
                _displayRequest = new DisplayRequest();
                _displayRequest.RequestActive();
            }
#endif                
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Handle window creation by setting our own custom theme.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            ThemeManager.SetThemeColor((Color)Resources["ThemeColor"]);

            base.OnWindowCreated(args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) DebugSettings.EnableFrameRateCounter = true;

            _displayRequest = new DisplayRequest();
            _displayRequest.RequestActive(); 
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();

                if (statusBar != null)
                {
                    statusBar.ForegroundColor = Colors.White;
                }
            }

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                   /* No state to load */
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    if (rootFrame.Content == null)
                    {
                        if (rootFrame.ContentTransitions != null)
                        {
                            _transitions = new TransitionCollection();

                            foreach (var c in rootFrame.ContentTransitions)
                            {
                                _transitions.Add(c);
                            }
                        }

                        rootFrame.ContentTransitions = null;
                        rootFrame.Navigated += RootFrame_FirstNavigated;

                        if (!rootFrame.Navigate(typeof(MainPage), e.Arguments)) throw new Exception("Failed to create initial page");
                    }
                }

                Window.Current.Activate();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// True if a tile can be added, otherwise false.
        /// </summary>
        public bool CanAddTile
        {
            get { return (_paired && !_tileAdded && !_syncing); }
        }

        /// <summary>
        /// True if a tile can be removed, otherwise false.
        /// </summary>
        public bool CanRemoveTile
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// True if we can sync items with the band, otherwise false.
        /// </summary>
        public bool CanSync
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsPaired
        {
            get { return _paired; }
            set
            {
                _paired = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsPaired"));
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsTileAdded
        {
            get { return _tileAdded; }
            set
            {
                _tileAdded = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsTileAdded"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// Returns the visibility state for the progress bar when syncing.
        /// </summary>
        public Visibility SyncVisibility
        {
            get { return (_syncing ? Visibility.Visible : Visibility.Collapsed); }
        }

        /// <summary>
        /// Returns true if we are already syncing with the band.
        /// </summary>
        public bool IsSyncing
        {
            get { return _syncing; }
            set
            {
                _syncing = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsSyncing"));
                PropertyChanged(this, new PropertyChangedEventArgs("SyncVisibility"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanSync"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// Returns the version.
        /// </summary>
        public string Version
        {
            get { return Common.Version; }
        }

        /// <summary>
        /// Returns the email address.
        /// </summary>
        public string Email
        {
            get { return Common.Email; }
        }

        /// <summary>
        /// The event that is fired when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the current instance of the application.
        /// </summary>
        public static new App Current
        {
            get { return (App)Application.Current; }
        }

        #endregion 
    }
}