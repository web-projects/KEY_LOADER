using Devices.Core.State.SubWorkflows;
using Devices.Core.State.SubWorkflows.Actions;
using Moq;
using System.Threading;

namespace Devices.Core.State.Actions.SubWorkflows.Tests
{
    class DeviceSubStateMachineAsyncManager
    {
        readonly ManualResetEvent resetEvent;

        public DeviceSubStateMachineAsyncManager()
            => resetEvent = new ManualResetEvent(false);

        public DeviceSubStateMachineAsyncManager(ref Mock<IDeviceSubStateController> mockController, IDeviceSubStateAction stateAction)
            : this()
        {
            mockController.Setup(e => e.Complete(stateAction)).Callback(() => resetEvent.Set());
            mockController.Setup(e => e.Error(stateAction)).Callback(() => resetEvent.Set());
        }

        public void Trigger() => resetEvent.Set();

        public bool WaitFor(int timeout = 2000) => resetEvent.WaitOne(timeout);

        public void Dispose() => resetEvent.Dispose();
    }
}
