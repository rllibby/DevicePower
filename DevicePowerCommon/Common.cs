/*
 *  Copyright © 2016, Russell Libby
 */

using System;
using Windows.ApplicationModel;
using Windows.System.Profile;

namespace DevicePowerCommon
{
    /// <summary>
    /// Common constants.
    /// </summary>
    public static class Common
    {
        #region Public properties

        /// <summary>
        /// Returns the simplified device family.
        /// </summary>
        public static string DeviceFamily
        {
            get { return AnalyticsInfo.VersionInfo.DeviceFamily.Replace("Windows.", ""); }
        }

        /// <summary>
        /// Description message for full charge.
        /// </summary>
        public static string FullCharge
        {
            get { return @"Fully Charged"; }
        }

        /// <summary>
        /// The application title.
        /// </summary>
        public static string Title
        {
            get { return @"Device Power"; }
        }

        /// <summary>
        /// Message for tile added.
        /// </summary>
        public static string TileAdded
        {
            get { return @"Device Power tile added to band."; }
        }

        /// <summary>
        /// Message for tile removed.
        /// </summary>
        public static string TileRemoved
        {
            get { return @"Device Power tile removed from band."; }
        }

        /// <summary>
        /// The application version.
        /// </summary>
        public static string Version
        {
            get { return "1.2.0.0"; }
        }

        /// <summary>
        /// The authors email address.
        /// </summary>
        public static string Email
        {
            get { return "rllibby@gmail.com"; }
        }

        /// <summary>
        /// The message to display when running band check.
        /// </summary>
        public static string Checking
        {
            get { return @"checking band..."; }
        }

        /// <summary>
        /// The message to display when updating.
        /// </summary>
        public static string Updating
        {
            get { return @"updating band...";  }
        }

        /// <summary>
        /// Guid id for the band tile.
        /// </summary>
        public static string TileGuid
        {
            get { return "7F000974-67BB-4B5F-B56E-3E1AE2530C14"; }
        }

        /// <summary>
        /// The background timer task name.
        /// </summary>
        public static string TimerTaskName
        {
            get { return "DevicePowerBandTimerTask"; }
        }

        /// <summary>
        /// The background system task name.
        /// </summary>
        public static string SystemTaskName
        {
            get { return "DevicePowerBandSystemTask"; }
        }

        /// <summary>
        /// Element id for title.
        /// </summary>
        public static short TitleId
        {
            get { return 1; }
        }

        /// <summary>
        /// Element id for spacer.
        /// </summary>
        public static short SpacerId
        {
            get { return 2; }
        }

        /// <summary>
        /// Element id for ssecondary title.
        /// </summary>
        public static short SeondaryTitleId
        {
            get { return 3; }
        }

        /// <summary>
        /// Element id for icon.
        /// </summary>
        public static short IconId
        {
            get { return 4; }
        }

        /// <summary>
        /// Element id for content.
        /// </summary>
        public static short ContentId
        {
            get { return 5; }
        }

        /// <summary>
        /// Element id for updated content.
        /// </summary>
        public static short UpdateId
        {
            get { return 6; }
        }

        /// <summary>
        /// The date format used by application.
        /// </summary>
        public static string DateFormat
        {
            get { return "MM/dd h:mm tt"; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines if we are running in an emulator.
        /// </summary>
        /// <returns></returns>
        public static bool IsEmulator()
        {
#if DEBUG
            if (!DeviceFamily.Equals("Mobile", StringComparison.OrdinalIgnoreCase)) return false;

            var package = Package.Current;

            return (package.Id.Architecture != Windows.System.ProcessorArchitecture.Arm);
#else
            return false;
#endif
        }

        #endregion
    }
}
