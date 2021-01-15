using StateMachine.State.Enums;
using System;
using XO.Requests;

namespace StateMachine.State.Management
{
    public interface IDeviceStateManager : IDisposable
    {
        void SetPluginPath(string pluginPath);
        void SetWorkflow(LinkDeviceActionType action);
        void LaunchWorkflow();
        DeviceWorkflowState GetCurrentWorkflow();
        void StopWorkflow();
        void DisplayDeviceStatus();
    }
}
