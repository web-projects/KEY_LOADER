using StateMachine.State.Enums;

using static StateMachine.State.Enums.DeviceSubWorkflowState;

namespace StateMachine.State.SubWorkflows
{
    public static class DeviceSubStateTransitionHelper
    {
        private static DeviceSubWorkflowState ComputeGetStatusStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeGetActiveKeySlotStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeGetSecurityConfigurationStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeDeviceAbortStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeDeviceResetStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeDeviceRebootStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeConfigurationStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeFeatureEnablementTokenStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeLockDeviceConfig0StateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeLockDeviceConfig8StateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeUnlockDeviceConfigStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeGetLoadHMACKeysStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeGetGenerateHMACStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeSanityCheckStateTransition(bool exception) =>
            exception switch
            {
                true => RequestComplete,
                false => RequestComplete
            };

        private static DeviceSubWorkflowState ComputeRequestCompletedStateTransition(bool exception) =>
            exception switch
            {
                true => Undefined,
                false => Undefined
            };

        public static DeviceSubWorkflowState GetNextState(DeviceSubWorkflowState state, bool exception) =>
            state switch
            {
                GetStatus => ComputeGetStatusStateTransition(exception),
                GetActiveKeySlot => ComputeGetActiveKeySlotStateTransition(exception),
                GetSecurityConfiguration => ComputeGetSecurityConfigurationStateTransition(exception),
                AbortCommand => ComputeDeviceAbortStateTransition(exception),
                ResetCommand => ComputeDeviceResetStateTransition(exception),
                RebootDevice => ComputeDeviceRebootStateTransition(exception),
                Configuration => ComputeConfigurationStateTransition(exception),
                FeatureEnablementToken => ComputeFeatureEnablementTokenStateTransition(exception),
                LockDeviceConfig0 => ComputeLockDeviceConfig0StateTransition(exception),
                LockDeviceConfig8 => ComputeLockDeviceConfig8StateTransition(exception),
                UnlockDeviceConfig => ComputeUnlockDeviceConfigStateTransition(exception),
                UpdateHMACKeys => ComputeGetLoadHMACKeysStateTransition(exception),
                GenerateHMAC => ComputeGetGenerateHMACStateTransition(exception),
                SanityCheck => ComputeSanityCheckStateTransition(exception),
                RequestComplete => ComputeRequestCompletedStateTransition(exception),
                _ => throw new StateException($"Invalid state transition '{state}' requested.")
            };
    }
}
