/*
 *  Copyright © 2015 Russell Libby
 */

using DevicePowerCommon;
using Microsoft.Band;
using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Power;
using Windows.Storage;
using Windows.System.Power;

namespace DevicePowerTask
{
    /// <summary>
    /// Common sync task routine to be called from all the different background tasks.
    /// </summary>
    internal static class SyncTask
    {
        #region Private constants

        private const string Status = "Status";

        #endregion

        #region Private fields

        private static BackgroundTaskDeferral _deferral;
        private static bool _cancelled;

        #endregion

        #region Private methods

        /// <summary>
        /// Event that is triggered when the task is cancelled.
        /// </summary>
        /// <param name="sender">The task instance cancelling the task.</param>
        /// <param name="reason">The reason for cancellation.</param>
        private static void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelled = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public static async void Run(IBackgroundTaskInstance taskInstance, DeviceTriggerType triggerType)
        {
            taskInstance.Canceled += OnTaskCanceled;

            _deferral = taskInstance.GetDeferral();
            _cancelled = false;

            try
            {
                taskInstance.Progress = 10;

                var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                taskInstance.Progress = 20;
                if ((pairedBands.Length < 1) || _cancelled) return;

                using (var bandClient = await SmartConnect.ConnectAsync(pairedBands[0], 5000))
                {
                    taskInstance.Progress = 40;
                    if (_cancelled) return;

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    taskInstance.Progress = 60;
                    if (!tiles.Any() || _cancelled) return;

                    if (triggerType == DeviceTriggerType.PowerChange) 
                    {
                        var battery = Battery.AggregateBattery;
                        var report = (battery == null) ? null : battery.GetReport();

                        try
                        {
                            var status = ApplicationData.Current.LocalSettings.Values[Status];

                            if ((status == null) || !string.Equals(status.ToString(), BatteryStatus.Idle.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                if ((report != null) && (report.Status == BatteryStatus.Idle) && (report.Percentage() == 100))
                                {
                                    await bandClient.NotificationManager.ShowDialogAsync(new Guid(Common.TileGuid), "Mobile", "Fully Charged");

                                    taskInstance.Progress = 80;
                                    if (_cancelled) return;
                                }
                            }
                        }
                        finally
                        {
                            ApplicationData.Current.LocalSettings.Values[Status] = (report == null) ? BatteryStatus.NotPresent.ToString() : report.Status.ToString();
                        }
                    }

                    await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));

                    taskInstance.Progress = 90;

                    await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), Data.GeneratePages());

                    taskInstance.Progress = 100;

                    Logging.Append(string.Format("sync trigger({0}).", triggerType.ToString()));
                }
            }
            catch (Exception exception)
            {
                Logging.Append(exception.ToString());
            }
            finally
            {
                taskInstance.Canceled -= OnTaskCanceled;

                _deferral.Complete();
                _deferral = null;
            }
        }

        #endregion
    }
}
