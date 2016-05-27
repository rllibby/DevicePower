using System;
/*
 *  Copyright © 2016, Russell Libby
 */

using Windows.Devices.Power;

namespace DevicePowerCommon
{
    /// <summary>
    /// Extension class for BatteryReport to provide safe handling of percentage.
    /// </summary>
    public static class BatteryReportExtension
    {
        /// <summary>
        /// Get the percentage of battery based on the report.
        /// </summary>
        /// <param name="value">The battery report.</param>
        /// <returns>A percentage value from 0 to 100.</returns>
        public static int Percentage(this BatteryReport value)
        {
            if (((value.RemainingCapacityInMilliwattHours == null) || (value.FullChargeCapacityInMilliwattHours == null))) return 0;

            return Math.Min(100, Convert.ToInt32(((double)value.RemainingCapacityInMilliwattHours / (double)value.FullChargeCapacityInMilliwattHours) * 100));
        }
    }
}
