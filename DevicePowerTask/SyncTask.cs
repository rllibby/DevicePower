/*
 *  Copyright © 2015 Russell Libby
 */

using DevicePowerCommon;
using Microsoft.Band;
using System;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace DevicePowerTask
{
    /// <summary>
    /// Common sync task routine to be called from all the different background tasks.
    /// </summary>
    internal static class SyncTask
    {
        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public static async void Run(IBackgroundTaskInstance taskInstance, DeviceTriggerType triggerType)
        {
            var deferral = taskInstance.GetDeferral();
            var isCancelled = false;

            BackgroundTaskCanceledEventHandler cancelled = (sender, reason) =>
            {
                isCancelled = true;
            };

            try
            {
                taskInstance.Progress = 10;

                var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                taskInstance.Progress = 20;
                if ((pairedBands.Length < 1) || isCancelled) return;

                using (var bandClient = await SmartConnect.ConnectAsync(pairedBands[0], 5000))
                {
                    taskInstance.Progress = 40;
                    if (isCancelled) return;

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    taskInstance.Progress = 60;
                    if (!tiles.Any() || isCancelled) return;

                    await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));

                    taskInstance.Progress = 80;

                    await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), Data.GeneratePages());

                    taskInstance.Progress = 100;

                    Logging.Append(string.Format("sync trigger({0}).", triggerType.ToString()));
                }
            }
            catch
            {
            }
            finally
            {
                taskInstance.Canceled -= cancelled;
                deferral.Complete();
            }
        }

        #endregion
    }
}
