/*
 *  Copyright © 2016 Russell Libby
 */

using DevicePowerCommon;
using DevicePowerCommon.Model;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
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
        private bool _isPowerChange;
        private bool _isFullCharge;

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
        /// Checks the power change event to see if the user's device is fully charged, and if this is 
        /// a new power state change..
        /// </summary>
        private void CheckPowerChange()
        {
            BatteryReportModel.Update();

            try
            {
                var status = ApplicationData.Current.LocalSettings.Values[Status];

                _isPowerChange = (status == null) || !string.Equals(BatteryReportModel.Status.ToString(), status.ToString());

                if ((status == null) || !string.Equals(status.ToString(), BatteryStatus.Idle.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    _isFullCharge = ((BatteryReportModel.Status == BatteryStatus.Idle) && (BatteryReportModel.Percentage == 100));
                }
            }
            catch (Exception exception)
            {
                Logging.AppendError("CheckPowerChange", exception.Message);
            }
            finally
            {
                ApplicationData.Current.LocalSettings.Values[Status] = BatteryReportModel.Status.ToString();
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

                if ((pairedBands.Length < 1) || (_deferral == null)) return;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    if (_deferral == null) return;

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    if (!tiles.Any() || (_deferral == null)) return;

                    if (_triggerType == DeviceTriggerType.PowerChange)
                    {
                        if (_isFullCharge)
                        {
                            await bandClient.NotificationManager.ShowDialogAsync(new Guid(Common.TileGuid), Common.DeviceFamily, Common.FullCharge);
                        }
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

            /* Filter out multiple power change events (same state) when running on non mobile devices */
            if (_triggerType == DeviceTriggerType.PowerChange)
            {
                CheckPowerChange();

                if (!_isPowerChange)
                {
                    _deferral?.Complete();
                    _deferral = null;

                    return;
                }
            }

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
