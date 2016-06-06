﻿/*
 *  Copyright © 2016 Russell Libby
 */

using DevicePowerCommon;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Devices.Power;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Power;

namespace DevicePowerTask
{
    /// <summary>
    /// Common sync task routine to be called from all the different background tasks.
    /// </summary>
    internal class SyncTask
    {
        #region Private constants

        private const string Status = "Status";

        #endregion

        #region Private fields

        private volatile BackgroundTaskDeferral _deferral;
        private DeviceTriggerType _triggerType;

        #endregion

        #region Private methods

        /// <summary>
        /// Event that is triggered when the task is cancelled.
        /// </summary>
        /// <param name="sender">The task instance cancelling the task.</param>
        /// <param name="reason">The reason for cancellation.</param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                Logging.Append(string.Format("Background task for trigger type '{0}' was cancelled due to '{1}.", _triggerType, reason));

                _deferral?.Complete();
            }
            finally
            {
                _deferral = null;
            }
        }

        /// <summary>
        /// Checks the power change event to see if the user's device is fully charged.
        /// </summary>
        /// <param name="client">The band client to use for notifications.</param>
        /// <param name="report">The current battery report.</param>
        private async Task CheckPowerChange(IBandClient client, BatteryReport report)
        {
            if (client == null) return;

            try
            {
                if (report == null) return;

                var status = ApplicationData.Current.LocalSettings.Values[Status];

                if ((status == null) || !string.Equals(status.ToString(), BatteryStatus.Idle.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if ((report != null) && (report.Status == BatteryStatus.Idle) && (report.Percentage() == 100))
                    {
                       await client.NotificationManager.ShowDialogAsync(new Guid(Common.TileGuid), Common.DeviceFamily, Common.FullCharge);
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.AppendError("CheckPowerChange", exception.Message);
            }
            finally
            {
                ApplicationData.Current.LocalSettings.Values[Status] = (report == null) ? BatteryStatus.NotPresent.ToString() : report.Status.ToString();
            }
        }

        /// <summary>
        /// The background task that performs the band update.
        /// </summary>
        /// <returns>The async task.</returns>
        private async Task RunBackgroundTask()
        {
            try
            { 
                var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                if (_deferral == null) return;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    if (_deferral == null) return;

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    if (!tiles.Any() || (_deferral == null)) return;

                    if (_triggerType == DeviceTriggerType.PowerChange)
                    {
                        var battery = Battery.AggregateBattery;

                        await CheckPowerChange(bandClient, (battery == null) ? null : battery.GetReport());
                    }

                    await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));
                    await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), Data.GeneratePages());
                }
            }
            catch (Exception exception)
            {
                Logging.AppendError("RunBackgroundTask", exception.Message);
            }
        }

        /// <summary>
        /// Event that is triggered when a request is received.
        /// </summary>
        /// <param name="sender">The app service connection.</param>
        /// <param name="args">The app service received event arguments.</param>
        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();

            try
            {
                BackgroundTileEventHandler.Instance.HandleTileEvent(args.Request.Message);

                await args.Request.SendResponseAsync(new ValueSet());
            }
            finally
            {
                messageDeferral.Complete();
            }
        }

        /// <summary>
        /// Event that is triggered when the band tile is opened.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The opened event arguments.</param>
        private async void OnTileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
        {
            await RunBackgroundTask();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="triggerType">The trigger type that this task will service.</param>
        public SyncTask(DeviceTriggerType triggerType)
        {
            _triggerType = triggerType;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            Logging.Append(string.Format("Background task started for trigger type '{0}'.", _triggerType));

            /* Handling for the AppServiceConnection */
            if (_triggerType == DeviceTriggerType.AppService)
            {
                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;

                if ((details != null) && (details.AppServiceConnection != null))
                {
                    BackgroundTileEventHandler.Instance.TileOpened += OnTileOpened;
                    details.AppServiceConnection.RequestReceived += OnRequestReceived;

                    return;
                }
            }

            /* Handling for the timer and power change triggers */
            try
            {
                taskInstance.Progress = 10;

                await RunBackgroundTask();
            }
            finally
            {
                taskInstance.Progress = 100;
                
                _deferral?.Complete();
                _deferral = null;
            }
        }

        #endregion
    }
}
