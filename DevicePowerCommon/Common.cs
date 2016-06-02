/*
 *  Copyright © 2016, Russell Libby
 */

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
        /// The application title.
        /// </summary>
        public static string Title
        {
            get { return @"Device Power"; }
        }

        /// <summary>
        /// The application version.
        /// </summary>
        public static string Version
        {
            get { return "1.1.0.0"; }
        }

        /// <summary>
        /// The authors email address.
        /// </summary>
        public static string Email
        {
            get { return "rllibby@gmail.com"; }
        }

        /// <summary>
        /// Guid id for the band tile.
        /// </summary>
        public static string TileGuid
        {
            get { return "84FC1467-7F57-4EA2-A1D2-46EFD844AC5E"; }
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
    }
}
