/*
 *  Copyright © 2016, Russell Libby
 */

namespace DevicePowerCommon
{
    /// <summary>
    /// Dialog related constants shared between the UI and tasks runtime component.
    /// </summary>
    public static class Dialogs
    {
        #region Public properties

        /// <summary>
        /// Ok
        /// </summary>
        public static string Ok
        {
            get { return "ok"; }
        }

        /// <summary>
        /// The message to display when the band tile limit has been reached.
        /// </summary>
        public static string TooManyTiles
        {
            get
            {
                return @"Sorry, the band tile limit has been reached. You can change the current band tiles from the Microsoft Health application.";
            }
        }

        /// <summary>
        /// The message to display when the band version is incorrect.
        /// </summary>
        public static string BadVersion
        {
            get
            {
                return @"Sorry, this application requires a Microsoft Band version 2.";
            }
        }

        /// <summary>
        /// The message content to display when a band is not paired.
        /// </summary>
        public static string NotPaired
        {
            get
            {
                return @"This application requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band.";
            }
        }

        /// <summary>
        /// The message content to display when a tile has been removed (by external application).
        /// </summary>
        public static string TileRemoved
        {
            get { return @"The band tile for this application has been removed."; }
        }

        /// <summary>
        /// The message content to display when buoy data has been sucessfully synced.
        /// </summary>
        public static string Synced
        {
            get { return @"Successfully synced data to your Microsoft Band."; }
        }

         #endregion
    }
}
