using System;
using Template10.Common;
using Template10.Services.SettingsService;
using Template10.Utils;
using Windows.UI.Xaml;

namespace DevicePower.Services.SettingsServices
{
    /// <summary>
    /// Settings service class.
    /// </summary>
    public class SettingsService
    {
        #region Private fields

        ISettingsHelper _helper;

        #endregion

        #region Constructor.

        /// <summary>
        /// Constructor.
        /// </summary>
        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static SettingsService Instance { get; } = new SettingsService();

        /// <summary>
        /// Determines if shell back button will be displayed.
        /// </summary>
        public bool UseShellBackButton
        {
            get { return _helper.Read<bool>(nameof(UseShellBackButton), true); }
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.Dispatcher.Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                    BootStrapper.Current.NavigationService.Refresh();
                });
            }
        }

        /// <summary>
        /// The navigation cache timeout.
        /// </summary>
        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        /// <summary>
        /// The warning level percentage.
        /// </summary>
        public int WarningLevel
        {
            get { return _helper.Read<int>(nameof(WarningLevel), 20); }
            set { _helper.Write(nameof(WarningLevel), value); }
        }

        /// <summary>
        /// The critical level percentage.
        /// </summary>
        public int CriticalLevel
        {
            get { return _helper.Read<int>(nameof(CriticalLevel), 5); }
            set { _helper.Write(nameof(CriticalLevel), value); }
        }

        #endregion
    }
}

