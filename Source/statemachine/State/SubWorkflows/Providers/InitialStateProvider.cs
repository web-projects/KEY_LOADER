using StateMachine.State.Enums;
using System;
using System.Linq;
using XO.Requests;

namespace StateMachine.State.SubWorkflows.Providers
{
    internal class InitialStateProvider
    {
        public DeviceSubWorkflowState DetermineInitialState(LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            LinkActionRequest linkActionRequest = request.Actions.First();
            DeviceSubWorkflowState proposedState = ((linkActionRequest.DeviceActionRequest?.DeviceAction) switch
            {
                LinkDeviceActionType.GetStatus => DeviceSubWorkflowState.GetStatus,
                LinkDeviceActionType.GetActiveKeySlot => DeviceSubWorkflowState.GetActiveKeySlot,
                LinkDeviceActionType.GetSecurityConfiguration => DeviceSubWorkflowState.GetSecurityConfiguration,
                LinkDeviceActionType.AbortCommand => DeviceSubWorkflowState.AbortCommand,
                LinkDeviceActionType.ResetCommand => DeviceSubWorkflowState.ResetCommand,
                LinkDeviceActionType.RebootDevice => DeviceSubWorkflowState.RebootDevice,
                LinkDeviceActionType.UnlockDeviceConfig => DeviceSubWorkflowState.UnlockDeviceConfig,
                LinkDeviceActionType.LockDeviceConfig => DeviceSubWorkflowState.LockDeviceConfig,
                LinkDeviceActionType.GenerateHMAC => DeviceSubWorkflowState.GenerateHMAC,
                LinkDeviceActionType.UpdateHMACKeys => DeviceSubWorkflowState.UpdateHMACKeys,
                _ => DeviceSubWorkflowState.Undefined
            });

            return proposedState;
        }
    }
}
