/*
 *  Copyright © 2016, Russell Libby
 */

using DevicePower.Services.SettingsServices;
using DevicePowerCommon;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Animation;

namespace DevicePower
{
    /// <summary>
    /// The application.
    /// </summary>
    [Bindable]
    sealed public partial class App : BootStrapper
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public App()
        {
            InitializeComponent();

            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;

            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion

#if DEBUG
            Logging.Append("Application started.");
#endif
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="args">The activation event arguments.</param>
        /// <returns>The async task.</returns>
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // Create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.Busy(),
                };
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Code that runs when application is resuming.
        /// </summary>
        /// <param name="s">The sender of the event.</param>
        /// <param name="e">Extra argument.</param>
        /// <param name="previousExecutionState">The previous execution state.</param>
        public override void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
        }

        /// <summary>
        /// Code that is executed when the application is suspending.
        /// </summary>
        /// <param name="s">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <param name="prelaunchActivated">True if pre-launch activated.</param>
        /// <returns>The task.</returns>
        public override async Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Startup routine.
        /// </summary>
        /// <param name="startKind">The startup kind.</param>
        /// <param name="args">The activation event arguments.</param>
        /// <returns>The async task.</returns>
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            try
            {
                await Task.Delay(1000);

                NavigationService.Navigate(typeof(Views.MainPage), null, new SuppressNavigationTransitionInfo());
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        #endregion
    }
}

