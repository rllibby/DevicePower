/*
 *  Copyright © 2016 Russell Libby
 */

using DevicePowerCommon;
using System;
using Template10.Mvvm;
using Windows.ApplicationModel.Email;

namespace DevicePower.ViewModels
{
    /// <summary>
    /// View model for the entire settings page.
    /// </summary>
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Public properties

        /// <summary>
        /// The settings view model.
        /// </summary>
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();

        /// <summary>
        /// The about view model.
        /// </summary>
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();

        #endregion
    }

    /// <summary>
    /// View model for settings.
    /// </summary>
    public class SettingsPartViewModel : ViewModelBase
    {
        #region Private fields

        private Services.SettingsServices.SettingsService _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingsPartViewModel()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Use shell drawn back button.
        /// </summary>
        public bool UseShellBackButton
        {
            get { return _settings.UseShellBackButton; }
            set
            {
                _settings.UseShellBackButton = value;

                base.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The warning level percentage.
        /// </summary>
        public int WarningLevel
        {
            get { return _settings.WarningLevel; }
            set
            {
                _settings.WarningLevel = value;

                if (_settings.CriticalLevel >= value)
                {
                    _settings.CriticalLevel = value - 1;
                    RaisePropertyChanged("CriticalLevel");
                }

                base.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The critical level percentage.
        /// </summary>
        public int CriticalLevel
        {
            get { return _settings.CriticalLevel; }
            set
            {
                _settings.CriticalLevel = value;

                if (_settings.WarningLevel <= value)
                {
                    _settings.WarningLevel = value + 1;
                    RaisePropertyChanged("WarningLevel");
                }

                base.RaisePropertyChanged();
            }
        }

        #endregion
    }

    /// <summary>
    /// View model for about.
    /// </summary>
    public class AboutPartViewModel : ViewModelBase
    {
        #region Public methods
            
        /// <summary>
        /// Generates an email to the publisher.
        /// </summary>
        public async void SendEmail()
        {
            var sendTo = new EmailRecipient
            {
                Address = Common.Email
            };

            var mail = new EmailMessage { Subject = string.Format("{0} {1}", Common.Title, Common.Version), Body = string.Empty };

            mail.To.Add(sendTo);

            await EmailManager.ShowComposeNewEmailAsync(mail).AsTask();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The logo for the about page.
        /// </summary>
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        /// <summary>
        /// The application display name.
        /// </summary>
        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        /// <summary>
        /// The application publisher.
        /// </summary>
        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        /// <summary>
        /// The application version.
        /// </summary>
        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;

                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        #endregion
    }
}

