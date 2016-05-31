/*
 *  Copyright © 2015 Russell Libby
 */

using DevicePowerCommon;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevicePower.Pages
{
    /// <summary>
    /// Page used to display the logging information.
    /// </summary>
    public sealed partial class LoggingPage : Page
    {
        #region Protected methods

        /// <summary>
        /// Invoked when this page is about to be navigated away from.
        /// </summary>
        /// <param name="e">Event data that describes how this page was navigated from.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.BackRequested -= BackNavigation;
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LogList.ItemsSource = Logging.Log();

            var currentView = SystemNavigationManager.GetForCurrentView();

            if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                currentView.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }

            currentView.BackRequested += BackNavigation;
        }

        private void BackNavigation(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }

        #endregion

        #region Constructor

        public LoggingPage()
        {
            InitializeComponent();
        }

        #endregion
    }
}
