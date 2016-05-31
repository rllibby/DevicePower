/*
 *  Copyright © 2015 Russell Libby
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
        /// Tile event trigger.
        /// </summary>
        Tile
    }
}
