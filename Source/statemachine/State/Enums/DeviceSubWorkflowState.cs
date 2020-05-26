using System;

namespace StateMachine.State.Enums
{
    /// <summary>
    /// Represents a set of sub-workflow states that represent certain specific
    /// processes that need to be completed before a transition occurs to send us
    /// back to the Manage state (Idle).
    /// </summary>
    public enum DeviceSubWorkflowState
    {
        /// <summary>
        /// Default state for all SubWorkflows.
        /// </summary>
        Undefined,

        /// <summary>
        /// Represents a state when DAL starts getting status information from the device
        /// </summary>
        GetStatus,

        /// <summary>
        /// Represents a state when DAL gets the active ADE KEY SLOT from the device
        /// </summary>
        GetActiveKeySlot,

        /// <summary>
        /// Represents a state when DAL gets security status information from the device
        /// </summary>
        GetSecurityConfiguration,

        /// <summary>
        /// Represents a state when DAL aborts pending device commands
        /// </summary>
        AbortCommand,

        /// <summary>
        /// Represents a state when DAL resets the device
        /// </summary>
        ResetCommand,

        /// <summary>
        /// Represents a state when DAL updates HMAC keys to the device
        /// </summary>
        UpdateHMACKeys,

        /// <summary>
        /// Represents a state when DAL generates HMAC from the device
        /// </summary>
        GenerateHMAC,

        /// <summary>
        /// Represents a state when DAL reboots the device
        /// </summary>
        RebootDevice,

        /// <summary>
        /// Represents a state when DAL updates Feature Enablement Token to device
        /// </summary>
        FeatureEnablementToken,

        /// <summary>
        /// Represents a state when DAL unlocks key updates on the device
        /// </summary>
        UnlockDeviceConfig,

        /// <summary>
        /// Represents a state when DAL locks key updates on the device
        /// </summary>
        LockDeviceConfig,

        /// <summary>
        /// Represents a state where a sanity check is performed to ensure that the DAL
        /// is in an operational state ready to receive the next command before a response
        /// is sent back to the caller.
        /// </summary>
        SanityCheck,

        /// <summary>
        /// Represents a state when SubWorkflow Completes
        /// </summary>
        RequestComplete
    }
}
