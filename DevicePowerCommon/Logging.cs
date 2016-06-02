﻿/*
 *  Copyright © 2015 Russell Libby
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace DevicePowerCommon
{
    /// <summary>
    /// Logging class.
    /// </summary>
    public static class Logging
    {
        #region Private constants

        private const string Logger = "logger";

        #endregion

        /// <summary>
        /// Creates a formatted message to add to the log.
        /// </summary>
        /// <param name="methodName">The name of the method where the error occurred.</param>
        /// <param name="error">The error description.</param>
        public static void AppendError(string methodName, string error)
        {
            Append(string.Format("error in '{0}'\r\n{1}", methodName, error));
        }

        /// <summary>
        /// Append data to the the current log.
        /// </summary>
        /// <param name="message"></param>
        public static void Append(string message)
        {
            var data = Log();

            data.Insert(0, string.Format("{0} - {1}", DateTime.Now.ToString("MM/dd/yy HH:mm:ss"), message));

            while (data.Count > 64) data.RemoveAt(data.Count - 1);

            ApplicationData.Current.LocalSettings.Values[Logger] = JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Obtains the current logging data.
        /// </summary>
        /// <returns></returns>
        public static IList<string> Log()
        {
            var data = ApplicationData.Current.LocalSettings.Values[Logger];

            try
            {
                return ((data == null) || string.IsNullOrEmpty(data.ToString())) ? new List<string>() : JsonConvert.DeserializeObject<IList<string>>(data.ToString());
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
