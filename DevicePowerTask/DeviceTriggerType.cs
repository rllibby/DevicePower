/*
 *  Copyright © 2016 Russell Libby
 */

namespace DevicePowerTask
{
    /// <summary>
    /// Enumeration for background task trigger types.
    /// </summary>
    public enum DeviceTriggerType
    {
        /// <summary>
        /// Timer trigger.
        /// </summary>
        Timer,

        /// <summary>
        /// System trigger based on power change.
        /// </summary>
        PowerChange,

        /// <summary>
        /// AppService event trigger.
        /// </summary>
        AppService
    }
}
