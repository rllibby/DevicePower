/*
 *  Copyright © 2015 Russell Libby
 */

using Microsoft.Band.Tiles.Pages;
using System;
using Windows.Devices.Power;
using Windows.System.Profile;

namespace DevicePowerCommon
{
    /// <summary>
    /// Generates page data for the application.
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Generates the data for page one of the band tile.
        /// </summary>
        /// <returns>The page data.</returns>
        public static PageData GeneratePageOneData()
        {
            var ai = AnalyticsInfo.VersionInfo;
            var family = ai.DeviceFamily.Replace("Windows.", "");
            var battery = Battery.AggregateBattery;
            var report = battery.GetReport();
            var percentage = report.Percentage();
            var title = new TextBlockData(Common.TitleId, family);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var secondary = new TextBlockData(Common.SeondaryTitleId, string.Format("{0}%", percentage));
            var content = new TextBlockData(Common.ContentId, report.Status.ToString().ToLower());

            return new PageData(Guid.NewGuid(), 0, title, spacer, secondary, content);
        }

        /// <summary>
        /// Generates the data for page two of the band tile.
        /// </summary>
        /// <returns>The page data.</returns>
        public static PageData GeneratePageTwoData(bool isApp = false)
        {
            var description = string.Format("{0} Updated\n{1}", (isApp ? "Application" : "Background"), DateTime.Now.ToString(Common.DateFormat));
            var updated = new WrappedTextBlockData(Common.UpdateId, description);

            return new PageData(Guid.NewGuid(), 1, updated);
        }
    }
}
