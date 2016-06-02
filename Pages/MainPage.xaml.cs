/*
 *  Copyright © 2016, Russell Libby
 */

using DevicePowerCommon;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Email;
using Windows.Devices.Power;
using Windows.Storage;
using Windows.System.Power;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

#pragma warning disable 4014

namespace DevicePower.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Private fields

        private static IBackgroundTaskRegistration _timerRegistration;
        private static IBackgroundTaskRegistration _systemRegistration;
        private App _viewModel;

        #endregion

        #region Private methods

        /// <summary>
        /// Unregisters our background task.
        /// </summary>
        private void UnregisterTask()
        {
            try
            {
                if (_timerRegistration != null)
                {
                    _timerRegistration.Completed -= OnTaskCompleted;
                    _timerRegistration.Progress -= OnTaskProgress;
                    _timerRegistration.Unregister(true);
                }

                if (_systemRegistration != null)
                {
                    _systemRegistration.Completed -= OnTaskCompleted;
                    _systemRegistration.Progress -= OnTaskProgress;
                    _systemRegistration.Unregister(true);
                }
            }
            finally
            {
                _timerRegistration = null;
                _systemRegistration = null;
            }
        }

        /// <summary>
        /// Register the background task.
        /// </summary>
        /// <returns>True if we were able to register the background task.</returns>
        private async Task<bool> RegisterTask()
        {
            if (GetTaskRegistration()) return true;

            var access = await BackgroundExecutionManager.RequestAccessAsync().AsTask();

            if (access != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity) return false;

            if (_timerRegistration == null)
            {
                var taskBuilder = new BackgroundTaskBuilder { Name = Common.TimerTaskName };
                var trigger = new TimeTrigger(60, false);

                taskBuilder.SetTrigger(trigger);
                taskBuilder.TaskEntryPoint = typeof(DevicePowerTask.DevicePowerTimerBandTask).FullName;
                taskBuilder.Register();
            }

            if (_systemRegistration == null)
            {
                var taskBuilder = new BackgroundTaskBuilder { Name = Common.SystemTaskName };
                var trigger = new SystemTrigger(SystemTriggerType.PowerStateChange, false);

                taskBuilder.SetTrigger(trigger);
                taskBuilder.TaskEntryPoint = typeof(DevicePowerTask.DevicePowerSystemBandTask).FullName;
                taskBuilder.Register();
            }

            return GetTaskRegistration();
        }

        /// <summary>
        /// Attempts to get the registered task.
        /// </summary>
        /// <returns>True if the task was aquired, otherwise false.</returns>
        private bool GetTaskRegistration()
        {
            if ((_timerRegistration != null) && (_systemRegistration != null)) return true;

            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (task.Name.Equals(Common.TimerTaskName)) _timerRegistration = task;
                if (task.Name.Equals(Common.SystemTaskName)) _systemRegistration = task;
            }

            if (_timerRegistration != null)
            {
                _timerRegistration.Completed += OnTaskCompleted;
                _timerRegistration.Progress += OnTaskProgress;
            }

            if (_systemRegistration != null)
            {
                _systemRegistration.Completed += OnTaskCompleted;
                _systemRegistration.Progress += OnTaskProgress;
            }

            return ((_timerRegistration != null) && (_systemRegistration != null));
        }

        /// <summary>
        /// Triggered when thhe background task is running and updating progress.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnTaskProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                 _viewModel.IsSyncing = true;
            });
        }

        /// <summary>
        /// Triggered when the background task has completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _viewModel.IsSyncing = false;
            });
        }

        /// <summary>
        /// Loads the png file from storage and creates a band tile icon from it.
        /// </summary>
        /// <param name="uri">The storage uri for the image.</param>
        /// <returns>The band tile icon.</returns>
        private static async Task<BandIcon> LoadIcon(string uri)
        {
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);

                await bitmap.SetSourceAsync(fileStream);

                return bitmap.ToBandIcon();
            }
        }

        /// <summary>
        /// Attempts to refresh / resync with the Microsoft band.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddTile(object sender, RoutedEventArgs e)
        {
            RunBandCheck();
        }

        /// <summary>
        /// Attempts to remove the application tile from the Microsoft band.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RemoveTile(object sender, RoutedEventArgs e)
        {
            DeleteTile();
        }

        /// <summary>
        /// Navigates to the logging page which displays detailed event information.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void LogsClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoggingPage));
        }

        /// <summary>
        /// Checks for version 2 of the band.
        /// </summary>
        /// <param name="version">The version string returned from the band client.</param>
        /// <returns>True if a band 2, otherwise false</returns>
        private bool VersionCheck(string version)
        {
            var v = 0;

            return (!int.TryParse(version, out v)) ? true : (v >= 20);
        }

        /// <summary>
        /// Generates the page layout for the first type of page.
        /// </summary>
        /// <returns>The page layout.</returns>
        private static PageLayout GeneratePageOne()
        {
            var titleBlock = new Microsoft.Band.Tiles.Pages.TextBlock
            {
                ColorSource = ElementColorSource.BandHighlight,
                ElementId = Common.TitleId,
                Rect = new PageRect(0, 0, 0, 35),
                AutoWidth = true,
                Baseline = 30,
                BaselineAlignment = TextBlockBaselineAlignment.Absolute
            };

            var spacerBlock = new Microsoft.Band.Tiles.Pages.TextBlock
            {
                Color = new BandColor(0x77, 0x77, 0x77),
                ElementId = Common.SpacerId,
                Rect = new PageRect(0, 0, 0, 35),
                Margins = new Margins(5, 0, 5, 0),
                AutoWidth = true,
                Baseline = 30,
                BaselineAlignment = TextBlockBaselineAlignment.Absolute
            };

            var secondaryTitleBlock = new Microsoft.Band.Tiles.Pages.TextBlock
            {
                ColorSource = ElementColorSource.BandSecondaryText,
                ElementId = Common.SeondaryTitleId,
                Rect = new PageRect(0, 0, 0, 35),
                AutoWidth = true,
                Baseline = 30,
                BaselineAlignment = TextBlockBaselineAlignment.Absolute
            };

            var topFlowPanel = new FlowPanel(titleBlock, spacerBlock, secondaryTitleBlock)
            {
                Rect = new PageRect(0, 0, 258, 40),
                Orientation = FlowPanelOrientation.Horizontal
            };

            var contentBlock = new Microsoft.Band.Tiles.Pages.TextBlock
            {
                Color = new BandColor(0xff, 0xff, 0xff),
                Font = TextBlockFont.Medium,
                ElementId = Common.ContentId,
                Rect = new PageRect(0, 0, 0, 78),
                AutoWidth = true,
                Baseline = 91,
                VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Bottom,
                HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center,
                BaselineAlignment = TextBlockBaselineAlignment.Absolute
            };

            var bottomFlowPanel = new FlowPanel(contentBlock)
            {
                Rect = new PageRect(0, 0, 258, 68),
                Orientation = FlowPanelOrientation.Horizontal
            };

            var panel = new FlowPanel(topFlowPanel, bottomFlowPanel)
            {
                Rect = new PageRect(15, 0, 258, 128)
            };

            return new PageLayout(panel);
        }

        /// <summary>
        /// Generates the page layout for the second type of page.
        /// </summary>
        /// <returns>The page layout.</returns>
        private static PageLayout GeneratePageTwo()
        {
            var updatedBlock = new WrappedTextBlock
            {
                ColorSource = ElementColorSource.BandSecondaryText,
                ElementId = Common.UpdateId,
                AutoHeight = true,
                Rect = new PageRect(0, 10, 200, 0),
            };

            var panel = new FlowPanel(updatedBlock)
            {
                Rect = new PageRect(15, 0, 230, 128)
            };

            return new PageLayout(panel);
        }

        /// <summary>
        /// Add the additional tile icons to the band tile.
        /// </summary>
        /// <param name="tile">The band tile.</param>
        private static async void AddTileIcons(BandTile tile)
        {
            if (tile == null) return;

            tile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/icon.png"));
        }

        /// <summary>
        /// Async method to remove the application tile from the Microsoft band.
        /// </summary>
        private async void DeleteTile()
        {
            var error = string.Empty;

            try
            {
                _viewModel.IsSyncing = true;

                UnregisterTask();

                var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    _viewModel.IsPaired = _viewModel.IsTileAdded = false;
                    await Dialogs.ShowDialog(this, Dialogs.NotPaired);

                    return;
                }

                _viewModel.IsPaired = true;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    if (tiles.Any()) await bandClient.TileManager.RemoveTileAsync(new Guid(Common.TileGuid));

                    await bandClient.UnsubscribeFromBackgroundTileEventsAsync(new Guid(Common.TileGuid));

                    _viewModel.IsTileAdded = false;
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
            finally
            {
                _viewModel.IsSyncing = false;
            }

            if (string.IsNullOrEmpty(error)) return;

            Logging.AppendError("Delete Tile", error);

            await Dialogs.ShowDialog(this, error);
        }

        /// <summary>
        /// Ensures that a band is paired and attempts to add the tile if not already added.
        /// </summary>
        private async void RunBandCheck()
        {
            var error = string.Empty;

            try
            {
                _viewModel.IsSyncing = true;

                await RegisterTask();

                var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    _viewModel.IsPaired = _viewModel.IsTileAdded = false;
                    await Dialogs.ShowDialog(this, Dialogs.NotPaired);

                    return;
                }

                _viewModel.IsPaired = true;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    if (!VersionCheck(await bandClient.GetHardwareVersionAsync()))
                    {
                        await Dialogs.ShowDialog(this, Dialogs.BadVersion);

                        return;
                    }

                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    try
                    {
                        if (tiles.Any())
                        {
                            _viewModel.IsTileAdded = true;
                            return;
                        }

                        if (tiles.Count() >= 20)
                        {
                            await Dialogs.ShowDialog(this, Dialogs.TooManyTiles);

                            return;
                        }

                        var tile = new BandTile(new Guid(Common.TileGuid))
                        {
                            Name = Common.Title,
                            TileIcon = await LoadIcon("ms-appx:///Assets/TileLarge.png"),
                            SmallIcon = await LoadIcon("ms-appx:///Assets/TileSmall.png"),
                        };

                        AddTileIcons(tile);

                        tile.PageLayouts.Add(GeneratePageOne());
                        tile.PageLayouts.Add(GeneratePageTwo());

                        try
                        {
                            _viewModel.IsTileAdded = await bandClient.TileManager.AddTileAsync(tile);
                        }
                        catch (BandIOException bandex)
                        {
                            _viewModel.IsTileAdded = (bandex.Message.Contains("MissingManifestResource"));

                            if (!_viewModel.IsTileAdded) error = bandex.Message;
                        }
                    }
                    finally
                    {
                        if (_viewModel.IsTileAdded) await bandClient.SubscribeToBackgroundTileEventsAsync(new Guid(Common.TileGuid));
                    }
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
            finally
            {
                _viewModel.IsSyncing = false;
            }

            if (string.IsNullOrEmpty(error)) return;

            Logging.AppendError("RunBandCheck", error);

            await Dialogs.ShowDialog(this, error);
        }

        /// <summary>
        /// Event that occurs when data is being synced to the band.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void Sync(object sender, RoutedEventArgs e)
        {
            var error = string.Empty;

            try
            {
                _viewModel.IsSyncing = true;

                await UpdateStatus();

                var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    _viewModel.IsPaired = _viewModel.IsTileAdded = false;
                    await Dialogs.ShowDialog(this, Dialogs.NotPaired);

                    return;
                }

                _viewModel.IsPaired = true;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    var tiles = await bandClient.TileManager.GetTilesAsync();

                    if (!tiles.Any())
                    {
                        _viewModel.IsTileAdded = false;

                        await Dialogs.ShowDialog(this, Dialogs.TileRemoved);

                        return;
                    }

                    await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), Data.GeneratePages());

                    Logging.Append("manual sync performed.");
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
            finally
            {
                _viewModel.IsSyncing = false;
            }

            if (string.IsNullOrEmpty(error))
            {
                await Dialogs.ShowDialog(this, Dialogs.Synced);

                return;
            }

            Logging.AppendError("Sync", error);

            await Dialogs.ShowDialog(this, error);
        }

        /// <summary>
        /// Event that is triggered when the email hyperlink is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event argument.</param>
        private async void EmailClicked(object sender, RoutedEventArgs e)
        {
            var includeLog = await Dialogs.ShowDialogYesNo(this, "Would you like to include the log in the body of the email?");

            var sendTo = new EmailRecipient
            {
                Address = Common.Email
            };

            var mail = new EmailMessage { Subject = string.Format("{0} {1}", Common.Title, Common.Version), Body = string.Empty };

            if (includeLog)
            {
                var log = Logging.Log();
                var attach = new StringBuilder("\r\n\r\n");

                foreach (var line in log) attach.AppendLine(line);

                attach.AppendLine();
                                
                mail.Body = attach.ToString();
            }

            mail.To.Add(sendTo);

            await EmailManager.ShowComposeNewEmailAsync(mail).AsTask();
        }

        /// <summary>
        /// Updates the status when a power changed event occurs.
        /// </summary>
        /// <returns>The async task.</returns>
        private async Task UpdateStatus()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var battery = Battery.AggregateBattery;
                var report = (battery == null) ? null : battery.GetReport();
                var estimate = PowerManager.RemainingDischargeTime;
                if (report == null) return;
                
                Percentage.Text = string.Format("{0}%", report.Percentage());
                Status.Text = report.StatusDescription();
                Estimate.Visibility = (estimate == TimeSpan.MaxValue) ? Visibility.Collapsed : Visibility.Visible;
                Estimate.Text = (estimate == TimeSpan.MaxValue) ? string.Empty : string.Format("{0:0.00} hrs", estimate.TotalHours);
            });
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = _viewModel = App.Current;

            GetTaskRegistration();

            await UpdateStatus();

            if (e.NavigationMode == NavigationMode.New)
            {
                Logging.Append("application started");
                Battery.AggregateBattery.ReportUpdated += async (sender, args) => await UpdateStatus();

                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RunBandCheck);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        #endregion
    }
}
