﻿/*
 *  Copyright © 2015 Russell Libby
 */

using Windows.ApplicationModel.Background;

#pragma warning disable 4014, 1998

namespace DevicePowerTask
{
    /// <summary>
    /// Background task for synchronizing data to the band.
    /// </summary>
    public sealed class DevicePowerSystemBandTask : IBackgroundTask
    {
        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            SyncTask.Run(taskInstance);
        }

        #endregion
    }
}
