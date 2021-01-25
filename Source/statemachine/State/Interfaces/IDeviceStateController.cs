using Core.Patterns.Queuing;
using DEVICE_SDK.Sdk;
using Devices.Common.AppConfig;
using Devices.Common.Interfaces;
using StateMachine.Cancellation;
using StateMachine.SerialPort.Interfaces;
using StateMachine.State.Actions;
using StateMachine.State.Actions.Preprocessing;
using StateMachine.State.Providers;
using System.Collections.Generic;

namespace StateMachine.State.Interfaces
{
    internal interface IDeviceStateController : IDeviceStateEventEmitter, IStateControlTrigger<IDeviceStateAction>
    {
        string PluginPath { get; }
        DeviceSection Configuration { get; }
        IDevicePluginLoader DevicePluginLoader { get; set; }
        List<ICardDevice> TargetDevices { get; }
        ISerialPortMonitor SerialPortMonitor { get; }
        PriorityQueue<PriorityQueueDeviceEvents> PriorityQueue { get; set; }
        //ILoggingServiceClient LoggingClient { get; }
        //IListenerConnector Connector { get; }
        List<ICardDevice> AvailableCardDevices { get; }
        void SetTargetDevices(List<ICardDevice> targetDevices);
        void SetPublishEventHandlerAsTask();
        void SendDeviceCommand(object message);
        void SaveState(object stateObject);
        IControllerVisitorProvider GetCurrentVisitorProvider();
        ISubStateManagerProvider GetSubStateManagerProvider();
        IDeviceCancellationBroker GetCancellationBroker();
        void DeviceStatusUpdate();
    }
}
