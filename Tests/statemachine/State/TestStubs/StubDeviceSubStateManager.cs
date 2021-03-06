﻿using Common.Config;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Devices.Core.Cancellation;
using Devices.Core.State.Interfaces;
using Devices.Core.State.SubWorkflows;
using Devices.Core.State.SubWorkflows.Actions;
using Devices.Core.State.SubWorkflows.Management;
using Devices.Core.State.Visitors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.XO.Requests;

namespace Devices.Core.State.TestStubs.Tests
{
    internal class StubDeviceSubStateManager : IDeviceSubStateManager, IDeviceSubStateController
    {
        public DeviceSection Configuration => throw new NotImplementedException();

        // public ILoggingServiceClient LoggingClient => throw new NotImplementedException();

        //public IListenerConnector Connector => throw new NotImplementedException();

        public List<ICardDevice> TargetDevices => throw new NotImplementedException();

        public bool DidTimeoutOccur => throw new NotImplementedException();

        public DeviceEvent DeviceEvent => throw new NotImplementedException();

        public event OnSubWorkflowCompleted SubWorkflowComplete;
        public event OnSubWorkflowError SubWorkflowError;

        public void Accept(IStateControllerVisitor<ISubWorkflowHook, IDeviceSubStateController> visitor)
        {

        }

        public Task Complete(IDeviceSubStateAction state) => Task.CompletedTask;

        public void ComportEventReceived(PortEventType comPortEvent, string portNumber)
        {

        }

        public void DeviceEventReceived(DeviceEvent deviceEvent, DeviceInformation deviceInformation)
        {

        }

        public void Dispose()
        {

        }

        public Task Error(IDeviceSubStateAction state) => Task.CompletedTask;

        public IDeviceCancellationBroker GetDeviceCancellationBroker()
        {
            return null;
        }

        public void LaunchWorkflow(WorkflowOptions launchOptions)
        {

        }

        public void RequestReceived(LinkRequest request)
        {

        }

        public void SaveState(object stateObject)
        {

        }

        public void SaveState(LinkRequest stateObject)
        {

        }
    }
}
