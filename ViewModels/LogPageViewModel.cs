/*
 *  Copyright © 2016, Russell Libby
 */

using DevicePower.Commands;
using DevicePowerCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;

namespace DevicePower.ViewModels
{
    /// <summary>
    /// Log page view model base.
    /// </summary>
    public class LogPageViewModel : ViewModelBase
    {
        #region Private fields

        private readonly ObservableCollection<string> _log = new ObservableCollection<string>();
        private RelayCommand _clearCommand;
        private RelayCommand _copyCommand;

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if the log can be cleared.
        /// </summary>
        /// <returns>Always true.</returns>
        private bool CanClear()
        {
            return true;
        }

        /// <summary>
        /// Determines if the log can be cleared.
        /// </summary>
        /// <returns>True if data in the log.</returns>
        private bool CanCopy()
        {
            return (_log.Count > 0);
        }

        /// <summary>
        /// Load the log file into our observable collection.
        /// </summary>
        private void LoadLogFile()
        {
            var log = Logging.Log();

            if (log.Count == 0) log.Add(string.Format("{0} - {1}", DateTime.Now.ToString("MM/dd/yy HH:mm:ss"), "No data to view."));

            foreach (var line in log) _log.Add(line);
        }

        /// <summary>
        /// Command action to copy the application log.
        /// </summary>
        private void Copy()
        {
            var package = new DataPackage();
            var temp = new StringBuilder();

            foreach (var line in _log) temp.AppendLine(line);

            package.RequestedOperation = DataPackageOperation.Copy;
            package.SetText(temp.ToString());

            Clipboard.SetContent(package);
        }

        /// <summary>
        /// Command action to clear the application log.
        /// </summary>
        private void Clear()
        {
            Logging.Clear();

            try
            {
                _log.Clear();
                Logging.Append("Log was cleared.");
            }
            finally
            {
                LoadLogFile();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) { }

            _copyCommand = new RelayCommand(new Action(Copy), CanCopy);
            _clearCommand = new RelayCommand(new Action(Clear), CanClear);

            LoadLogFile();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Event that is triggered when this view model is navigated to.
        /// </summary>
        /// <param name="parameter">The sender of the event.</param>
        /// <param name="mode">The navigation mode.</param>
        /// <param name="suspensionState">The dictionary of application state.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Event that is triggered when this view model is navigated from.
        /// </summary>
        /// <param name="suspensionState">The dictionary of application state.</param>
        /// <param name="suspending">True if application is suspending.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Event that is triggered when this view model is about to be navigated from.
        /// </summary>
        /// <param name="args">The navigating event arguments.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            try
            {
                args.Cancel = false;
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// True if not running on a mobile device.
        /// </summary>
        public bool IsNotMobile
        {
            get { return !IsMobile; }
        }

        /// <summary>
        /// True if running on a mobile device.
        /// </summary>
        public bool IsMobile
        {
            get { return Common.DeviceFamily.Equals("Mobile", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Relay command for clearing the log.
        /// </summary>
        public RelayCommand ClearCommand
        {
            get { return _clearCommand; }
        }

        /// <summary>
        /// Relay command for copying the log.
        /// </summary>
        public RelayCommand CopyCommand
        {
            get { return _copyCommand; }
        }

        /// <summary>
        /// The collection of logged data.
        /// </summary>
        public ObservableCollection<string> Log
        {
            get { return _log; }
        }

        #endregion
    }
}

