﻿using Newtonsoft.Json;
using StateMachine.Cancellation;
using StateMachine.Helpers;
using StateMachine.State.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XO.Requests;

namespace StateMachine.State.SubWorkflows.Actions
{
    internal class DeviceUpdateIdleScreenSubStateAction : DeviceBaseSubStateAction
    {
        public override DeviceSubWorkflowState WorkflowStateType => DeviceSubWorkflowState.UpdateIdleScreen;

        public DeviceUpdateIdleScreenSubStateAction(IDeviceSubStateController _) : base(_) { }

        public override SubStateActionLaunchRules LaunchRules => new SubStateActionLaunchRules
        {
            RequestCancellationToken = true
        };

        public override async Task DoWork()
        {
            if (StateObject is null)
            {
                //_ = Controller.LoggingClient.LogErrorAsync("Unable to find a state object while attempting to update device idle screen.");
                Console.WriteLine("Unable to find a state object while attempting to update device idle screen.");
                _ = Error(this);
            }
            else
            {
                LinkRequest linkRequest = StateObject as LinkRequest;
                IDeviceCancellationBroker cancellationBroker = Controller.GetDeviceCancellationBroker();

                List<LinkRequest> devicesRequest = new List<LinkRequest>();

                foreach (var device in Controller.TargetDevices)
                {
                    devicesRequest.Add(JsonConvert.DeserializeObject<LinkRequest>(JsonConvert.SerializeObject(linkRequest)));

                    var timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => device.UpdateIdleScreen(devicesRequest.Last()),
                        DeviceConstants.CardCaptureTimeout,
                        System.Threading.CancellationToken.None);

                    if (timeoutPolicy.Outcome == Polly.OutcomeType.Failure)
                    {
                        //_ = Controller.LoggingClient.LogErrorAsync($"Unable to update device idle screen - '{Controller.DeviceEvent}'.");
                        Console.WriteLine($"Unable to update device idle screen - '{Controller.DeviceEvent}'.");
                        BuildSubworkflowErrorResponse(linkRequest, device.DeviceInformation, Controller.DeviceEvent);
                    }
                }

                /*if (linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse == null)
                {
                    linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse = new LinkDeviceResponse();
                }

                if (linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices == null)
                {
                    linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices = new List<LinkDeviceResponse>();
                }

                foreach (var response in devicesRequest)
                {
                    if (response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices != null)
                    {
                        linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices.Add(new LinkDeviceResponse
                        {
                            Manufacturer = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Manufacturer,
                            Model = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Model,
                            SerialNumber = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].SerialNumber
                        });
                    }
                }*/

                Controller.SaveState(linkRequest);

                _ = Complete(this);
            }
        }
    }
}
