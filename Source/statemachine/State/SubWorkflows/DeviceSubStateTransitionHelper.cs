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

        private static DeviceSubWorkflowState ComputeUnlockDeviceConfigStateTransition(bool exception) =>
            exception switch
            {
                true => SanityCheck,
                false => SanityCheck
            };

        private static DeviceSubWorkflowState ComputeLockDeviceConfigStateTransition(bool exception) =>
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
                UnlockDeviceConfig => ComputeUnlockDeviceConfigStateTransition(exception),
                LockDeviceConfig => ComputeLockDeviceConfigStateTransition(exception),
                UpdateHMACKeys => ComputeGetLoadHMACKeysStateTransition(exception),
                GenerateHMAC => ComputeGetGenerateHMACStateTransition(exception),
                SanityCheck => ComputeSanityCheckStateTransition(exception),
                RequestComplete => ComputeRequestCompletedStateTransition(exception),
                _ => throw new StateException($"Invalid state transition '{state}' requested.")
            };
    }
}
