/*
 *  Copyright © 2015 Russell Libby
 */

using Microsoft.Band;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevicePowerCommon
{
    /// <summary>
    /// Class for handling retry connections on the band client connect. This is important because if a 
    /// connection is already established by another application, our attempts to open a connection will fail.
    /// </summary>
    public sealed class SmartConnect
    {
        /// <summary>
        /// Attempts a band connect with the ability to retry using exponential backoff.
        /// </summary>
        /// <param name="bandInfo">The band info that represents the band to connect to.</param>
        /// <param name="timeout">The maximum time to retry the connection.</param>
        /// <returns>The band client on success, throws on failure.</returns>
        public static async Task<IBandClient> ConnectAsync(IBandInfo bandInfo, int timeout)
        {
            if (bandInfo == null) throw new ArgumentNullException("bandInfo");

            var maxBackoff = timeout;
            var exponent = 1;
            var delay = 0;

            while (true)
            {
                await Task.Delay(delay);

                var mark = Environment.TickCount;

                try
                {
                    return await BandClientManager.Instance.ConnectAsync(bandInfo);
                }
                catch (BandIOException)
                {
                    timeout -= (Environment.TickCount - mark);

                    if (timeout <= 0) throw;

                    var backoff = Math.Min(maxBackoff, (int)(50 * Math.Pow(2, exponent++)));

                    delay = Math.Min(timeout, backoff);
                    timeout -= delay;

                    continue;
                }
            }
        }
    }
}
