/*
 *  Copyright © 2016, Russell Libby
 */

using System;
using Windows.Devices.Power;
using Windows.System.Power;

namespace DevicePowerCommon.Model
{
    /// <summary>
    /// Singleton model class for handling the battery report.
    /// </summary>
    public static class BatteryReportModel
    {
        #region Private constants

        private const string Unavailable = "unavailable";

        #endregion

        #region Private fields

        private static BatteryStatus _status;
        private static object _lock = new object();
        private static int _percentage;
        private static string _statusDescription;
        private static string _estimate;

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the current battery percentage.
        /// </summary>
        /// <param name="report">The battery report.</param>
        /// <returns>The battery level as a percentage.</returns>
        private static int GetPercentage(BatteryReport report)
        {
            if ((report == null) || (report.RemainingCapacityInMilliwattHours == null) || (report.FullChargeCapacityInMilliwattHours == null)) return 0;

            return Math.Min(100, Convert.ToInt32(((double)report.RemainingCapacityInMilliwattHours / (double)report.FullChargeCapacityInMilliwattHours) * 100));
        }

        /// <summary>
        /// Returns the status description.
        /// </summary>
        /// <param name="report">The battery report.</param>
        /// <returns>The status of the battery report.</returns>
        private static string GetStatusDescription(BatteryReport report)
        {
            if ((report == null) || (report.Status == BatteryStatus.NotPresent)) return Unavailable;

            return report.Status.ToString().ToLower();
        }

        /// <summary>
        /// Returns the battery estimate as a string when discharging.
        /// </summary>
        /// <returns>The battery estimate.</returns>
        private static string GetEstimate()
        {
            if (_status != BatteryStatus.Discharging) return string.Empty;

            var estimate = PowerManager.RemainingDischargeTime;

            return (estimate == TimeSpan.MaxValue) ? string.Empty : string.Format("{0:0.00} hrs", estimate.TotalHours);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the current model based on the battery report and power manager.
        /// </summary>
        public static void Update()
        {
            lock (_lock)
            {
                var battery = Battery.AggregateBattery;
                var report = (battery == null) ? null : battery.GetReport();
                var estimate = PowerManager.RemainingDischargeTime;

                _percentage = GetPercentage(report);
                _status = (report == null) ? BatteryStatus.NotPresent : report.Status;
                _statusDescription = GetStatusDescription(report);
                _estimate = GetEstimate();
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Returns the battery percentage.
        /// </summary>
        public static int Percentage
        {
            get { return _percentage; }
        }

        /// <summary>
        /// Returns the battery status.
        /// </summary>
        public static BatteryStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Returns the description for the battery status.
        /// </summary>
        public static string StatusDescription
        {
            get { return _statusDescription; }
        }

        /// <summary>
        /// Returns the battery estimate when discharging.
        /// </summary>
        public static string Estimate
        {
            get { return _estimate; }
        }

        #endregion
    }
}
