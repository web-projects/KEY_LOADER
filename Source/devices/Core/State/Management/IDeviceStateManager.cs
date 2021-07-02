using Devices.Core.State.Enums;
using System;
using Common.XO.Requests;

namespace Devices.Core.State.Management
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
