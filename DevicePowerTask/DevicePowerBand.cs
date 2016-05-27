﻿/*
 *  Copyright © 2015 Russell Libby
 */

using DevicePowerCommon;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

#pragma warning disable 4014, 1998

namespace DevicePowerTask
{
    /// <summary>
    /// Background task for synchronizing data to the band.
    /// </summary>
    public sealed class DevicePowerBand : IBackgroundTask
    {
        #region Private fields

        private BackgroundTaskDeferral _backgroundTaskDeferral;
        private AppServiceConnection _appServiceConnection;
        private volatile bool _cancelled;

        #endregion

        #region Private methods

        /// <summary>
        /// Event that is triggered when the task is cancelled.
        /// </summary>
        /// <param name="sender">The task instance cancelling the task.</param>
        /// <param name="reason">The reason for cancellation.</param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (_backgroundTaskDeferral != null)
            {
                _backgroundTaskDeferral.Complete();
                _backgroundTaskDeferral = null;
            }
        }

        /// <summary>
        /// Runs the task to update the band, and waits for completion.
        /// </summary>
        /// <returns>The task.</returns>
        private async Task RunTask()
        {
            _cancelled = false;

            try
            {
                var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                if ((pairedBands.Length < 1) || _cancelled) return;

                using (var bandClient = await SmartConnect.ConnectAsync(pairedBands[0], 2000))
                {
                    if (_cancelled) return;

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    if (!tiles.Any() || _cancelled) return;

                    var mainPage = Data.GeneratePageOneData();
                    var secondaryPage = Data.GeneratePageTwoData();

                    if (_cancelled) return;

                    await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));
                    await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), new[] { secondaryPage, mainPage });
                }
            }
            catch
            {
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
                var request = args.Request.Message;

                _cancelled = false;

                BackgroundTileEventHandler.Instance.HandleTileEvent(request);
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
        private void OnTileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
        {
            RunTask().Wait();
        }

        /// <summary>
        /// Event that is triggered when the band tile is closed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The closed event arguments.</param>
        private void OnTileClosed(object sender, BandTileEventArgs<IBandTileClosedEvent> e)
        {
            _cancelled = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _backgroundTaskDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;

            BackgroundTileEventHandler.Instance.TileOpened += OnTileOpened;
            BackgroundTileEventHandler.Instance.TileClosed += OnTileClosed;

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            _appServiceConnection = details.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        #endregion
    }
}