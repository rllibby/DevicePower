/*
 *  Copyright © 2016 Russell Libby
 */

using DevicePowerCommon.Model;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;
using Windows.Devices.Power;
using Windows.System.Power;

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
        /// <returns>The page data.</returns>
        private static PageData GenerateMainPageData()
        {
            var title = new TextBlockData(Common.TitleId, Common.DeviceFamily);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var secondary = new TextBlockData(Common.SeondaryTitleId, string.Format("{0}%", BatteryReportModel.Percentage));
            var content = new TextBlockData(Common.ContentId, BatteryReportModel.StatusDescription);

            return new PageData(Guid.NewGuid(), 0, title, spacer, secondary, content);
        }

        /// <summary>
        /// Generates the estimate page of the band tile.
        /// </summary>
        /// <param name="estimate">The estimated remaining time.</param>
        /// <returns>The page data.</returns>
        private static PageData GenerateEstimatePageData(string estimate)
        {
            var title = new TextBlockData(Common.TitleId, Common.DeviceFamily);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var secondary = new TextBlockData(Common.SeondaryTitleId, "Estimate");
            var content = new TextBlockData(Common.ContentId, estimate);

            return new PageData(Guid.NewGuid(), 0, title, spacer, secondary, content);
        }

        /// <summary>
        /// Generates the data for page two of the band tile.
        /// </summary>
        /// <param name="applicationUpdate">True if being updated by the application.</param>
        /// <returns>The page data.</returns>
        private static PageData GenerateInfoPageData(bool applicationUpdate = false)
        {
            var description = string.Format("{0} Updated\n{1}", (applicationUpdate ? "Application" : "Background"), DateTime.Now.ToString(Common.DateFormat));
            var updated = new WrappedTextBlockData(Common.UpdateId, description);

            return new PageData(Guid.NewGuid(), 1, updated);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generates all the pages for the band tile.
        /// </summary>
        /// <param name="applicationUpdate">True if being updated by the application.</param>
        /// <returns>An array of page data.</returns>
        public static PageData[] GeneratePages(bool applicationUpdate = false)
        {
            BatteryReportModel.Update();

            var results = new List<PageData>();
            var estimate = BatteryReportModel.Estimate;

            results.Insert(0, GenerateMainPageData());
            if (!string.IsNullOrEmpty(estimate)) results.Insert(0, GenerateEstimatePageData(estimate));
            results.Insert(0, GenerateInfoPageData(applicationUpdate));

            return results.ToArray();
        }

        #endregion
    }
}
