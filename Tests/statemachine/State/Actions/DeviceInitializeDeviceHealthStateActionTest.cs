﻿using Devices.Core.State.Enums;
using Devices.Core.State.Interfaces;
using Devices.Core.Tests;
using Moq;
using System;
using Xunit;

namespace Devices.Core.State.Actions.Tests
{
    public class DeviceInitializeDeviceHealthStateActionTest : IDisposable
    {
        readonly DeviceInitializeDeviceHealthStateAction subject;
        readonly Mock<IDeviceStateController> mockController;
        //readonly Mock<ILoggingServiceClient> mockLoggingClient;

        readonly DeviceStateMachineAsyncManager asyncManager;

        public DeviceInitializeDeviceHealthStateActionTest()
        {
            //mockLoggingClient = new Mock<ILoggingServiceClient>();

            mockController = new Mock<IDeviceStateController>();
            //mockController.SetupGet(e => e.LoggingClient).Returns(mockLoggingClient.Object);

            subject = new DeviceInitializeDeviceHealthStateAction(mockController.Object);

            asyncManager = new DeviceStateMachineAsyncManager(ref mockController, subject);
        }

        public void Dispose() => asyncManager.Dispose();

        [Fact]
        public void WorkflowStateType_Should_Equal_InitializeDeviceHealth()
            => Assert.Equal(DeviceWorkflowState.InitializeDeviceHealth, subject.WorkflowStateType);

        [Fact]
        public async void DoWork_ShouldCallSetPublishEventHandlerAsTask_When_Called()
        {
            await subject.DoWork();

            Assert.True(asyncManager.WaitFor());

            //mockLoggingClient.Verify(e => e.LogInfoAsync(It.IsAny<string>(), null), Times.Once());

            mockController.Verify(e => e.SetPublishEventHandlerAsTask(), Times.Once());
            mockController.Verify(e => e.Complete(subject));
        }
    }
}
