﻿using StateMachine.State.SubWorkflows.Management;
using StateMachine.State.Enums;
using System;
using System.Collections.Generic;

using static StateMachine.State.Enums.DeviceSubWorkflowState;

namespace StateMachine.State.SubWorkflows.Actions.Controllers
{
    internal class DeviceStateActionSubControllerImpl : IDeviceSubStateActionController
    {
        private readonly IDeviceSubStateManager manager;

        private Dictionary<DeviceSubWorkflowState, Func<IDeviceSubStateController, IDeviceSubStateAction>> workflowMap =
            new Dictionary<DeviceSubWorkflowState, Func<IDeviceSubStateController, IDeviceSubStateAction>>(
                new Dictionary<DeviceSubWorkflowState, Func<IDeviceSubStateController, IDeviceSubStateAction>>
                {
                    [GetStatus] = (IDeviceSubStateController _) => new DeviceGetStatusSubStateAction(_),
                    [GetActiveKeySlot] = (IDeviceSubStateController _) => new DeviceGetActiveKeySlotSubStateAction(_),
                    [GetEMVKernelChecksum] = (IDeviceSubStateController _) => new DeviceGetEMVKernelChecksumSubStateAction(_),
                    [GetSecurityConfiguration] = (IDeviceSubStateController _) => new DeviceGetSecurityConfigurationSubStateAction(_),
                    [Configuration] = (IDeviceSubStateController _) => new DeviceConfigurationSubStateAction(_),
                    [FeatureEnablementToken] = (IDeviceSubStateController _) => new DeviceFeatureEnablementTokenSubStateAction(_),
                    [LockDeviceConfig0] = (IDeviceSubStateController _) => new DeviceLockConfiguration0SubStateAction(_),
                    [LockDeviceConfig8] = (IDeviceSubStateController _) => new DeviceLockConfiguration8SubStateAction(_),
                    [UnlockDeviceConfig] = (IDeviceSubStateController _) => new DeviceUnlockConfigurationSubStateAction(_),
                    [UpdateIdleScreen] = (IDeviceSubStateController _) => new DeviceUpdateIdleScreenSubStateAction(_),
                    [AbortCommand] = (IDeviceSubStateController _) => new DeviceAbortCommandSubStateAction(_),
                    [ResetCommand] = (IDeviceSubStateController _) => new DeviceResetCommandSubStateAction(_),
                    [RebootDevice] = (IDeviceSubStateController _) => new DeviceRebootSubStateAction(_),
                    [UpdateHMACKeys] = (IDeviceSubStateController _) => new DeviceUpdateHMACKeysSubStateAction(_),
                    [GenerateHMAC] = (IDeviceSubStateController _) => new DeviceGenerateHMACSubStateAction(_),
                    [SanityCheck] = (IDeviceSubStateController _) => new DeviceSanityCheckSubStateAction(_),
                    [RequestComplete] = (IDeviceSubStateController _) => new DeviceRequestCompleteSubStateAction(_)
                }
        );

        private IDeviceSubStateAction currentStateAction;

        public DeviceStateActionSubControllerImpl(IDeviceSubStateManager manager) => (this.manager) = (manager);

        public IDeviceSubStateAction GetFinalState()
            => workflowMap[RequestComplete](manager as IDeviceSubStateController);

        public IDeviceSubStateAction GetNextAction(IDeviceSubStateAction stateAction)
            => GetNextAction(stateAction.WorkflowStateType);

        public IDeviceSubStateAction GetNextAction(DeviceSubWorkflowState state)
        {
            IDeviceSubStateController controller = manager as IDeviceSubStateController;
            if (currentStateAction == null)
            {
                return (currentStateAction = workflowMap[state](controller));
            }

            DeviceSubWorkflowState proposedState = DeviceSubStateTransitionHelper.GetNextState(state, currentStateAction.LastException != null);
            if (proposedState == currentStateAction.WorkflowStateType)
            {
                return currentStateAction;
            }

            return (currentStateAction = workflowMap[proposedState](controller));
        }
    }
}
