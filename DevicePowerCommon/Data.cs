/*
 *  Copyright © 2015 Russell Libby
 */

using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;
using Windows.Devices.Power;
using Windows.System.Profile;

namespace DevicePowerCommon
{
    /// <summary>
    /// Generates page data for the application.
    /// </summary>
    public static class Data
    {
        #region Private methods

        /// <summary>
        /// Generates the main page of the band tile.
        /// </summary>
        /// <param name="report">The current battery report.</param>
        /// <returns>The page data.</returns>
        private static PageData GenerateMainPageData(BatteryReport report)
        {
            var ai = AnalyticsInfo.VersionInfo;
            var family = ai.DeviceFamily.Replace("Windows.", "");
            var percentage = report.Percentage();

            var title = new TextBlockData(Common.TitleId, family);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var secondary = new TextBlockData(Common.SeondaryTitleId, string.Format("{0}%", percentage));
            var content = new TextBlockData(Common.ContentId, report.Status.ToString().ToLower());

            return new PageData(Guid.NewGuid(), 0, title, spacer, secondary, content);
        }

        /// <summary>
        /// Generates the discharge information page.
        /// </summary>
        /// <param name="dischargeInfo">The discharge information string.</param>
        /// <returns>The page data.</returns>
        private static PageData GenerateDischargePageData(string dischargeInfo)
        {
            var ai = AnalyticsInfo.VersionInfo;
            var family = ai.DeviceFamily.Replace("Windows.", "");

            var title = new TextBlockData(Common.TitleId, family);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var secondary = new TextBlockData(Common.SeondaryTitleId, "Estimate");
            var content = new TextBlockData(Common.ContentId, dischargeInfo);

            return new PageData(Guid.NewGuid(), 0, title, spacer, secondary, content);
        }

        /// <summary>
        /// Generates the data for page two of the band tile.
        /// </summary>
        /// <returns>The page data.</returns>
        public static PageData GenerateInfoPageData(bool isApp = false)
        {
            var description = string.Format("{0} Updated\n{1}", (isApp ? "Application" : "Background"), DateTime.Now.ToString(Common.DateFormat));
            var updated = new WrappedTextBlockData(Common.UpdateId, description);

            return new PageData(Guid.NewGuid(), 1, updated);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generates all the pages for the band tile.
        /// </summary>
        /// <returns>An array of page data.</returns>
        public static PageData[] GeneratePages()
        {
            var results = new List<PageData>();
            var settings = new Settings();
            var battery = Battery.AggregateBattery;
            var report = (battery == null) ? null : battery.GetReport();

            if (report != null)
            {
                results.Insert(0, GenerateMainPageData(report));

                settings.Update(report);

                var discharge = settings.GetEstimate();

                if (!string.IsNullOrEmpty(discharge))
                {
                    results.Insert(0, GenerateDischargePageData(discharge));
                }
            }

            results.Insert(0, GenerateInfoPageData());

            return results.ToArray();
        }

        #endregion
    }
}
