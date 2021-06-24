using Core.Patterns.Queuing;
using DEVICE_SDK.Sdk;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Interfaces;
using Devices.Core.Cancellation;
using Devices.Core.SerialPort.Interfaces;
using Devices.Core.State.Actions;
using Devices.Core.State.Actions.Preprocessing;
using Devices.Core.State.Enums;
using Devices.Core.State.Interfaces;
using Devices.Core.State.Management;
using Devices.Core.State.Providers;
using Devices.Core.State.SubWorkflows.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XO.Requests;

namespace Devices.Core.State.TestStubs.Tests
{
    internal class StubDeviceStateManager : IDeviceStateManager, IDeviceStateController
    {
        public string PluginPath => throw new NotImplementedException();

        public ICardDevice TargetDevice => throw new NotImplementedException();

        public List<ICardDevice> TargetDevices => throw new NotImplementedException();

        public DeviceSection Configuration => throw new NotImplementedException();

        public IDevicePluginLoader DevicePluginLoader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool NeedsDeviceRecovery { get; set; }

        //public ILoggingServiceClient LoggingClient => throw new NotImplementedException();

        //public IListenerConnector Connector => throw new NotImplementedException();

        public List<ICardDevice> AvailableCardDevices => throw new NotImplementedException();

        public ISerialPortMonitor SerialPortMonitor => throw new NotImplementedException();

        DeviceEventHandler IDeviceStateEventEmitter.DeviceEventReceived { get; set; }

        ComPortEventHandler IDeviceStateEventEmitter.ComPortEventReceived { get; set; }

        public PriorityQueue<PriorityQueueDeviceEvents> PriorityQueue { get; set; }

        public StateActionRules StateActionRules { get; set; }

        public event OnRequestReceived RequestReceived;
        public event OnWorkflowStopped WorkflowStopped;
        public event OnStateChange StateChange;

        public void Dispose()
        {

        }

        public Task Complete(IDeviceStateAction state) => Task.CompletedTask;

        public Task Error(IDeviceStateAction state) => Task.CompletedTask;

        public IDeviceCancellationBroker GetCancellationBroker()
        {
            return null;
        }

        public IControllerVisitorProvider GetCurrentVisitorProvider()
        {
            return null;
        }

        public IDeviceSubStateManager GetSubStateManagerProvider()
        {
            return null;
        }

        ISubStateManagerProvider IDeviceStateController.GetSubStateManagerProvider()
        {
            return null;
        }

        public void LaunchWorkflow()
        {

        }

        public void SaveState(object stateObject)
        {

        }

        public void SetPluginPath(string pluginPath)
        {

        }

        public void SetPublishEventHandlerAsTask()
        {

        }

        public void SetTargetDevices(List<ICardDevice> targetDevices)
        {

        }

        public void SendDeviceCommand(object message)
        {

        }

        public void StopWorkflow()
        {

        }

        public void SetWorkflow(LinkDeviceActionType action)
        {

        }

        public DeviceWorkflowState GetCurrentWorkflow()
        {
            throw new NotImplementedException();
        }

        public void DisplayDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public void DeviceStatusUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
