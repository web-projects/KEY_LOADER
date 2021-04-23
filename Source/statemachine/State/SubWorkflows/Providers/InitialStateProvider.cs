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
                LinkDeviceActionType.GetEMVKernelChecksum => DeviceSubWorkflowState.GetEMVKernelChecksum,
                LinkDeviceActionType.GetSecurityConfiguration => DeviceSubWorkflowState.GetSecurityConfiguration,
                LinkDeviceActionType.AbortCommand => DeviceSubWorkflowState.AbortCommand,
                LinkDeviceActionType.ResetCommand => DeviceSubWorkflowState.ResetCommand,
                LinkDeviceActionType.RebootDevice => DeviceSubWorkflowState.RebootDevice,
                LinkDeviceActionType.Configuration => DeviceSubWorkflowState.Configuration,
                LinkDeviceActionType.FeatureEnablementToken => DeviceSubWorkflowState.FeatureEnablementToken,
                LinkDeviceActionType.LockDeviceConfig0 => DeviceSubWorkflowState.LockDeviceConfig0,
                LinkDeviceActionType.LockDeviceConfig8 => DeviceSubWorkflowState.LockDeviceConfig8,
                LinkDeviceActionType.UnlockDeviceConfig => DeviceSubWorkflowState.UnlockDeviceConfig,
                LinkDeviceActionType.GenerateHMAC => DeviceSubWorkflowState.GenerateHMAC,
                LinkDeviceActionType.UpdateHMACKeys => DeviceSubWorkflowState.UpdateHMACKeys,
                LinkDeviceActionType.UpdateIdleScreen => DeviceSubWorkflowState.UpdateIdleScreen,
                LinkDeviceActionType.DisplayCustomScreen => DeviceSubWorkflowState.DisplayCustomScreen,
                _ => DeviceSubWorkflowState.Undefined
            });

            return proposedState;
        }
    }
}
