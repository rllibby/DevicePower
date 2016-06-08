/*
 *  Copyright © 2016, Russell Libby
 */

using DevicePower.Commands;
using DevicePower.Views;
using DevicePowerCommon;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Background;
using Windows.Devices.Power;
using Windows.Storage;
using Windows.System.Power;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DevicePower.ViewModels
{
    /// <summary>
    /// Main page view model base.
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        #region Private fields

        private Services.SettingsServices.SettingsService _settings;
        private static IBackgroundTaskRegistration _timerRegistration;
        private static IBackgroundTaskRegistration _systemRegistration;
        private RelayCommand _canAdd;
        private RelayCommand _canRemove;
        private RelayCommand _canSync;
        private static bool _syncing = true;
        private static bool _paired;
        private static bool _tileAdded;
        private static bool _bandCheck;
        private double _percentage;
        private string _estimate;
        private string _status;
        private int _percent;

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
            Dispatcher.DispatchAsync(() =>
            {
                IsSyncing = true;
            });
        }

        /// <summary>
        /// Triggered when the background task has completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Dispatcher.DispatchAsync(() =>
            {
                IsSyncing = false;
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
        /// Ensures that a band is paired and attempts to add the tile if not already added.
        /// </summary>
        private async Task RunBandCheck()
        {
            await Dispatcher.DispatchAsync(async () =>
            {
                var error = string.Empty;

                try
                {
                    IsSyncing = true;

                    await RegisterTask();

                    var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                    if (pairedBands.Length < 1)
                    {
                        IsPaired = IsTileAdded = false;
                        await Dialogs.ShowDialog(null, Dialogs.NotPaired);

                        return;
                    }

                    IsPaired = true;

                    using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                    {
                        if (!VersionCheck(await bandClient.GetHardwareVersionAsync()))
                        {
                            await Dialogs.ShowDialog(null, Dialogs.BadVersion);

                            return;
                        }

                        var tiles = await bandClient.TileManager.GetTilesAsync();

                        try
                        {
                            if (tiles.Any())
                            {
                                IsTileAdded = true;
                                return;
                            }

                            if (tiles.Count() >= 20)
                            {
                                await Dialogs.ShowDialog(null, Dialogs.TooManyTiles);

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
                                IsTileAdded = await bandClient.TileManager.AddTileAsync(tile);
                            }
                            catch (BandIOException bandex)
                            {
                                IsTileAdded = (bandex.Message.Contains("MissingManifestResource"));

                                if (!IsTileAdded) error = bandex.Message;
                            }
                        }
                        finally
                        {
                            if (IsTileAdded) await bandClient.SubscribeToBackgroundTileEventsAsync(new Guid(Common.TileGuid));
                        }
                    }
                }
                catch (Exception exception)
                {
                    error = exception.Message;
                }
                finally
                {
                    IsSyncing = false;
                }

                if (string.IsNullOrEmpty(error)) return;

                Logging.Append(error);

                await Dialogs.ShowDialog(null, error);
            });
        }

        /// <summary>
        /// Async method to remove the application tile from the Microsoft band.
        /// </summary>
        private async Task DeleteTile()
        {
            await Dispatcher.DispatchAsync(async () =>
            {
                var error = string.Empty;

                try
                {
                    IsSyncing = true;

                    UnregisterTask();

                    var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                    if (pairedBands.Length < 1)
                    {
                        IsPaired = IsTileAdded = false;
                        await Dialogs.ShowDialog(null, Dialogs.NotPaired);

                        return;
                    }

                    IsPaired = true;

                    using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                    {
                        var tiles = await bandClient.TileManager.GetTilesAsync();

                        if (tiles.Any()) await bandClient.TileManager.RemoveTileAsync(new Guid(Common.TileGuid));

                        await bandClient.UnsubscribeFromBackgroundTileEventsAsync(new Guid(Common.TileGuid));

                        IsTileAdded = false;
                    }
                }
                catch (Exception exception)
                {
                    error = exception.Message;
                }
                finally
                {
                    IsSyncing = false;
                }

                if (string.IsNullOrEmpty(error)) return;

                Logging.Append(error);

                await Dialogs.ShowDialog(null, error);
            });
        }

        /// <summary>
        /// Async method to sync data to the Microsoft band.
        /// </summary>
        private async Task SyncToBand()
        {
            await Dispatcher.DispatchAsync(async () =>
            {

                var error = string.Empty;

                try
                {
                    IsSyncing = true;

                    await UpdateStatus();

                    var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                    if (pairedBands.Length < 1)
                    {
                        IsPaired = IsTileAdded = false;
                        await Dialogs.ShowDialog(null, Dialogs.NotPaired);

                        return;
                    }

                    IsPaired = true;

                    using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                    {
                        var tiles = await bandClient.TileManager.GetTilesAsync();

                        if (!tiles.Any())
                        {
                            IsTileAdded = false;

                            await Dialogs.ShowDialog(null, Dialogs.TileRemoved);

                            return;
                        }

                        await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), Data.GeneratePages(true));

                        Logging.Append("SyncToBand performed.");
                    }
                }
                catch (Exception exception)
                {
                    error = exception.Message;
                }
                finally
                {
                    IsSyncing = false;
                }

                if (string.IsNullOrEmpty(error))
                {
                    await Dialogs.ShowDialog(null, Dialogs.Synced);

                    return;
                }

                Logging.Append(error);

                await Dialogs.ShowDialog(null, error);
            });
        }

        /// <summary>
        /// Determines if tile can be added.
        /// </summary>
        /// <returns>True if the tile can be added.</returns>
        private bool CanAddTile()
        {
            return (_paired && !_tileAdded && !_syncing); ;
        }

        /// <summary>
        /// Determines if the tile can be removed.
        /// </summary>
        /// <returns>True if the tile can be removed.</returns>
        private bool CanRemoveTile()
        {
            return (_paired && _tileAdded && !_syncing);
        }

        /// <summary>
        /// Determines if data can be synced to the band.
        /// </summary>
        /// <returns>True if data can be synced.</returns>
        private bool CanSync()
        {
            return (_paired && _tileAdded && !_syncing);
        }

        /// <summary>
        /// Performs a data sync to the connected band.
        /// </summary>
        private async void Sync()
        {
            await SyncToBand();
        }

        /// <summary>
        /// Attempts to refresh / resync with the Microsoft band.
        /// </summary>
        public async void AddTile()
        {
            Busy.SetBusy(true, Common.Updating);

            try
            {
                await RunBandCheck();
            }
            finally
            {
                Busy.SetBusy(false);
            }
        }

        /// <summary>
        /// Attempts to remove the application tile from the Microsoft band.
        /// </summary>
        private async void RemoveTile()
        {
            Busy.SetBusy(true, Common.Updating);

            try
            {
                await DeleteTile();
            }
            finally
            {
                Busy.SetBusy(false);
            }
        }

        /// <summary>
        /// Event that is triggered when the battery report changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private async void OnBatteryReportUpdated(Battery sender, object args)
        {
            await UpdateStatus();
        }

        /// <summary>
        /// Updates the status when a power changed event occurs.
        /// </summary>
        /// <returns>The async task.</returns>
        private async Task UpdateStatus()
        {
            await Dispatcher.DispatchAsync(() =>
            {
                var battery = Battery.AggregateBattery;
                var report = (battery == null) ? null : battery.GetReport();
                var estimate = PowerManager.RemainingDischargeTime;

                if (report == null) return;

                _percent = (Common.IsEmulator() ? 98 : report.Percentage());

                Percentage = _percent;
                Status = report.StatusDescription();
                Estimate = (estimate == TimeSpan.MaxValue) ? string.Empty : string.Format("{0:0.00} hrs", estimate.TotalHours);
            });
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPageViewModel()
        {
            _settings = Services.SettingsServices.SettingsService.Instance;
            _canAdd = new RelayCommand(new Action(AddTile), CanAddTile);
            _canRemove = new RelayCommand(new Action(RemoveTile), CanRemoveTile);
            _canSync = new RelayCommand(new Action(Sync), CanSync);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Event that is triggered when this view model is navigated to.
        /// </summary>
        /// <param name="parameter">The sender of the event.</param>
        /// <param name="mode">The navigation mode.</param>
        /// <param name="suspensionState">The dictionary of application state.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            try
            {
                GetTaskRegistration();
                Battery.AggregateBattery.ReportUpdated += OnBatteryReportUpdated;

                await UpdateStatus();

                if ((mode == NavigationMode.New) && !_bandCheck)
                {
                    _bandCheck = true;

                    Busy.SetBusy(true, Common.Checking);

                    try
                    {
                        await RunBandCheck();
                    }
                    finally
                    {
                        Busy.SetBusy(false);
                    }

                }
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Event that is triggered when this view model is navigated from.
        /// </summary>
        /// <param name="suspensionState">The dictionary of application state.</param>
        /// <param name="suspending">True if application is suspending.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Event that is triggered when this view model is about to be navigated from.
        /// </summary>
        /// <param name="args">The navigating event arguments.</param>
        /// <returns>The async task.</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            try
            {
                args.Cancel = false;
                Battery.AggregateBattery.ReportUpdated -= OnBatteryReportUpdated;
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsPaired
        {
            get { return _paired; }
            set
            {
                _paired = value;

                RaisePropertyChanged("IsPaired");
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsTileAdded
        {
            get { return _tileAdded; }
            set
            {
                _tileAdded = value;

                RaisePropertyChanged("IsTileAdded");

                _canAdd.RaiseCanExecuteChanged();
                _canRemove.RaiseCanExecuteChanged();
                _canSync.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Returns the visibility state for the progress bar when syncing.
        /// </summary>
        public Visibility SyncVisibility
        {
            get { return (_syncing ? Visibility.Visible : Visibility.Collapsed); }
        }

        /// <summary>
        /// Returns true if we are already syncing with the band.
        /// </summary>
        public bool IsSyncing
        {
            get { return _syncing; }
            set
            {
                _syncing = value;

                RaisePropertyChanged("IsSyncing");
                RaisePropertyChanged("SyncVisibility");

                _canAdd.RaiseCanExecuteChanged();
                _canRemove.RaiseCanExecuteChanged();
                _canSync.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Add command.
        /// </summary>
        public RelayCommand AddCommand
        {
            get { return _canAdd; }
        }

        /// <summary>
        /// Remove command.
        /// </summary>
        public RelayCommand RemoveCommand
        {
            get { return _canRemove; }
        }

        /// <summary>
        /// Sync command.
        /// </summary>
        public RelayCommand SyncCommand
        {
            get { return _canSync; }
        }

        /// <summary>
        /// Battery percentage.
        /// </summary>
        public double Percentage
        {
            get { return _percentage;  }
            set
            {
                _percentage = value;
                RaisePropertyChanged("Percentage");
                RaisePropertyChanged("PercentageColor");
            }
        }

        /// <summary>
        /// Battery percentage brush color.
        /// </summary>
        public SolidColorBrush PercentageColor
        {
            get
            {
                var percent = Convert.ToInt32(Percentage);
                var warn = _settings.WarningLevel;
                var critical = _settings.CriticalLevel;
                
                if (percent == 0) return Application.Current.Resources["AppBarItemForegroundThemeBrush"] as SolidColorBrush;
                if (percent > warn) return new SolidColorBrush(Colors.DarkGreen);
                if (percent > critical) return new SolidColorBrush(Color.FromArgb(255, 252, 187, 28));

                return new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Battery status.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                base.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Battery estimate.
        /// </summary>
        public string Estimate
        {
            get { return _estimate; }
            set
            {
                _estimate = value;

                RaisePropertyChanged("EstimateVisible");
                base.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// True if the battery estimate should be shown, otherwise false.
        /// </summary>
        public bool EstimateVisible
        {
            get { return (string.IsNullOrEmpty(_estimate) ? false : true); }
            set
            {
                base.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Returns the version.
        /// </summary>
        public string Version
        {
            get { return Common.Version; }
        }

        /// <summary>
        /// Returns the email address.
        /// </summary>
        public string Email
        {
            get { return Common.Email; }
        }

        #endregion
    }
}