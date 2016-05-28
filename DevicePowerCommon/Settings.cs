/*
 * 
 */

using System;
using Windows.Storage;
using Windows.Devices.Power;
using Windows.System.Power;

namespace DevicePowerCommon
{
    /// <summary>
    /// Class to maintain the persisted settings.
    /// </summary>
    public class Settings
    {
        #region Private constants

        private const string Time = "SnapshotTime";
        private const string BatteryLevel = "SnapshotLevel";
        private const string EstimatedTime = "SnapshotEstimate";

        #endregion

        #region Private fields

        private static readonly object _lock = new object();

        #endregion

        #region Private methods

        /// <summary>
        /// Removes the local settings.
        /// </summary>
        private void Remove()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values[Time] = null;
            localSettings.Values[BatteryLevel] = null;
            localSettings.Values[EstimatedTime] = null;
        }

        #endregion

        /// <summary>
        /// Updates the local settings based on the current battery report.
        /// </summary>
        /// <param name="report">The current battery report.</param>
        public void Update(BatteryReport report)
        {
            lock (_lock)
            {
                if ((report == null) || (report.Status != BatteryStatus.Discharging) || (report.RemainingCapacityInMilliwattHours == null))
                {
                    Remove();
                    return;
                }

                var localSettings = ApplicationData.Current.LocalSettings;
                var time = localSettings.Values[Time];
                var level = localSettings.Values[BatteryLevel];

                if ((time == null) || (level == null))
                {
                    localSettings.Values[Time] = DateTime.Now.ToString();
                    localSettings.Values[BatteryLevel] = report.RemainingCapacityInMilliwattHours.Value;

                    return;
                }

                var snapshotTime = DateTime.Parse(time.ToString());
                var elapsed = DateTime.Now - snapshotTime;
                var discharged = (int)level - report.RemainingCapacityInMilliwattHours.Value;

                if ((elapsed.TotalSeconds < 60) || (discharged == 0)) return;

                var mwps = ((double)discharged / elapsed.TotalSeconds);
                var estimate = ((double)report.RemainingCapacityInMilliwattHours.Value / mwps);
                var display = string.Format("{0:dd\\.hh\\:mm}", TimeSpan.FromSeconds(estimate));

                localSettings.Values[EstimatedTime] = display;
            }
        }

        /// <summary>
        /// Gets the calculated estimate of time for the battery.
        /// </summary>
        /// <returns>The estimate if stored, otherwise null.</returns>
        public string GetEstimate()
        {
            lock(_lock)
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                var value = localSettings.Values[EstimatedTime];

                return (value == null) ? null : value.ToString();
            }
        }
    }
}
