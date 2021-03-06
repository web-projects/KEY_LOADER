﻿using Common.Config;
using Common.Core.Patterns.Queuing;
using DEVICE_SDK.Sdk;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Newtonsoft.Json;
using Ninject;
using Devices.Core.Cancellation;
using Devices.Core.Providers;
using Devices.Core.SerialPort.Interfaces;
using Devices.Core.State.Actions;
using Devices.Core.State.Actions.Controllers;
using Devices.Core.State.Actions.Preprocessing;
using Devices.Core.State.Enums;
using Devices.Core.State.Interfaces;
using Devices.Core.State.Providers;
using Devices.Core.State.SubWorkflows;
using Devices.Core.State.SubWorkflows.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.XO.Requests;
using Common.XO.Device;

namespace Devices.Core.State.Management
{
    internal class DeviceStateManagerImpl : IInitializable, IDeviceStateController, IDeviceStateManager, ISubWorkflowHook
    {
        private bool disposed;

        //[Inject]
        //public IListenerConnectorProvider ListenerConnectorProvider { get; set; }

        [Inject]
        public IDeviceConfigurationProvider DeviceConfigurationProvider { get; set; }

        [Inject]
        public IDevicePluginLoader DevicePluginLoader { get; set; }

        //[Inject]
        //public ILoggingServiceClientProvider LoggingServiceClientProvider { get; set; }

        [Inject]
        public IDeviceStateActionControllerProvider DeviceStateActionControllerProvider { get; set; }

        [Inject]
        public IControllerVisitorProvider ControllerVisitorProvider { get; set; }

        [Inject]
        public ISubStateManagerProvider SubStateManagerProvider { get; set; }

        [Inject]
        public ISerialPortMonitor SerialPortMonitor { get; set; }

        [Inject]
        public IDeviceCancellationBrokerProvider DeviceCancellationBrokerProvider { get; set; }

        public DeviceSection Configuration { get; private set; }

        //public ILoggingServiceClient LoggingClient { get; private set; }

        //public IListenerConnector Connector { get; private set; }

        public List<ICardDevice> AvailableCardDevices { get; private set; } = new List<ICardDevice>();

        public string PluginPath { get; private set; }

        public List<ICardDevice> TargetDevices { get; private set; }

        public DeviceEventHandler DeviceEventReceived { get; set; }

        public ComPortEventHandler ComPortEventReceived { get; set; }

        public PriorityQueue<PriorityQueueDeviceEvents> PriorityQueue { get; set; }

        public StateActionRules StateActionRules { get; private set; }

        public bool NeedsDeviceRecovery { get; set; }

        public bool DeviceListenerIsOnline { get; set; }

        private static bool subscribed { get; set; }
        private bool deviceNeedsToBeAdded;

        private IDeviceSubStateController subStateController;
        private IDeviceStateAction currentStateAction;
        private IDeviceStateActionController stateActionController;
        private readonly Stack<object> savedStackState = new Stack<object>();

        public event OnStateChange StateChange;
        public event OnWorkflowStopped WorkflowStopped;
        public event OnRequestReceived RequestReceived;

        public DeviceStateManagerImpl()
        {
            PriorityQueue = new PriorityQueue<PriorityQueueDeviceEvents>();
        }

        public void Initialize()
        {
            DeviceEventReceived = OnDeviceEventReceived;
            ComPortEventReceived = OnComPortEventReceivedAsync;

            SerialPortMonitor.ComportEventOccured += OnComPortEventReceivedAsync;
            SerialPortMonitor.StartMonitoring();

            StateActionRules = new StateActionRules();

            DeviceConfigurationProvider.InitializeConfiguration();
            Configuration = DeviceConfigurationProvider.GetAppConfig();
            //LoggingClient = LoggingServiceClientProvider.GetLoggingServiceClient();
            //Connector = ListenerConnectorProvider.GetConnector(DeviceConfigurationProvider.GetConfiguration());

            stateActionController = DeviceStateActionControllerProvider.GetStateActionController(this);

            InitializeConnectorEvents();
        }

        public void SetPluginPath(string pluginPath) => (PluginPath) = (pluginPath);

        public void SetTargetDevices(List<ICardDevice> targetDevices)
        {
            if (TargetDevices != null)
            {
                foreach (var device in TargetDevices)
                {
                    device?.Disconnect();
                }
            }
            TargetDevices = targetDevices;

            if (targetDevices != null)
            {
                foreach (var device in targetDevices)
                {
                    device.SetDeviceSectionConfig(Configuration);
                }
            }
        }

        public void SetPublishEventHandlerAsTask()
        {
            if (TargetDevices != null)
            {
                foreach (var device in TargetDevices)
                {
                    NeedsDeviceRecovery = false;
                    //device.PublishEvent += PublishEventHandlerAsTask;
                    device.DeviceEventOccured += OnDeviceEventReceived;
                }
            }
        }

        public void SaveState(object stateObject) => savedStackState.Push(stateObject);

        public async Task Recovery(IDeviceStateAction state, object stateObject)
        {
            SaveState(stateObject);
            await AdvanceActionWithState(state);
        }

        public void Hook(IDeviceSubStateController controller) => subStateController = (controller);

        public void UnHook() => subStateController = (null);

        public IControllerVisitorProvider GetCurrentVisitorProvider() => ControllerVisitorProvider;

        public IDeviceCancellationBroker GetCancellationBroker() => DeviceCancellationBrokerProvider.GetDeviceCancellationBroker();

        public ISubStateManagerProvider GetSubStateManagerProvider() => SubStateManagerProvider;

        protected void RaiseOnRequestReceived(string data)
        {
            try
            {
                LogMessage("Request received from", data);
                LinkRequest linkRequest = JsonConvert.DeserializeObject<LinkRequest>(data.ToString());
                RequestReceived?.Invoke(linkRequest);
            }
            catch (Exception e)
            {
                LogError(e.Message, data);
                //if (Connector != null)
                {
                    //Connector.Publish(LinkRequestResponseError(null, "DALError", e.Message), new TopicOption[] { TopicOption.Servicer }).ConfigureAwait(false);
                    Console.WriteLine($"RaisedOnRequestReceived: exception='{e.Message}'");
                }
            }
        }

        protected void RaiseOnWorkflowStopped(DeviceWorkflowStopReason reason) => WorkflowStopped?.Invoke(reason);

        private void OnDeviceEventReceived(DeviceEvent deviceEvent, DeviceInformation deviceInformation)
        {
            if (currentStateAction.WorkflowStateType == DeviceWorkflowState.SubWorkflowIdleState)
            {
                if (subStateController != null)
                {
                    IDeviceSubStateManager subStateManager = subStateController as IDeviceSubStateManager;
                    subStateManager.DeviceEventReceived(deviceEvent, deviceInformation);
                }
            }
        }

        private async Task<bool> DisconnectAllDevices(PortEventType comPortEvent, string portNumber)
        {
            bool peformDeviceDiscovery = false;

            if (TargetDevices != null)
            {
                // dispose of all existing connections so that device recovery re-validates them
                ICardDevice deviceDisconnected = TargetDevices.Where(a => a.DeviceInformation.ComPort.Equals(portNumber, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (deviceDisconnected != null)
                {
                    deviceDisconnected.Disconnect();
                    peformDeviceDiscovery = true;
                    foreach (var device in TargetDevices)
                    {
                        if (comPortEvent == PortEventType.Removal)
                        {
                            if (device == deviceDisconnected)
                            {
                                Console.WriteLine($"\n\nDEVICE: Comport unplugged: '{portNumber}', " +
                                   $"DeviceType '{device.ManufacturerConfigID}', SerialNumber '{device.DeviceInformation?.SerialNumber}'");

                                //PublishDeviceDisconnectEvent(device, portNumber);
                            }
                        }
                        device.Disconnect();
                    }

                    // Is the device rebooting?
                    //if (currentStateAction.WorkflowStateType != DeviceWorkflowState.Manage)
                    //{
                    //    Debug.WriteLine($"Currently in {currentStateAction.WorkflowStateType}: forcing state transition...");
                    //    await Complete(currentStateAction);
                    //}
                }
            }
            return peformDeviceDiscovery;
        }

        private void OnComPortEventReceivedAsync(PortEventType comPortEvent, string portNumber)
        {
            Task<bool> peformDeviceDiscovery = Task.FromResult(false);

            if (comPortEvent == PortEventType.Insertion)
            {
                Console.WriteLine($"DEVICE: Comport Plugged. ComportNumber '{portNumber}'. Detecting a new connection...");
                peformDeviceDiscovery = DisconnectAllDevices(comPortEvent, portNumber);
                deviceNeedsToBeAdded = true;
            }
            else if (comPortEvent == PortEventType.Removal)
            {
                peformDeviceDiscovery = DisconnectAllDevices(comPortEvent, portNumber);
            }
            else
            {
                Console.WriteLine($"DEVICE: Comport Event '{comPortEvent}' is not implemented ");
            }

            // only perform discovery when an existing device is disconnected or a new connection is detected
            if (peformDeviceDiscovery.Result)
            {
                Console.WriteLine($"DEVICE: discovery in progress...");

                // wait for USB driver to detach/reattach device
                //await Task.Delay(Configuration.DeviceDiscoveryDelay * 1024);
                Task.Delay(5000);

                currentStateAction.DoDeviceDiscovery();

                //else
                //{
                //    StateActionRules.NeedsDeviceRecovery = true;

                //    if (subStateController != null)
                //    {
                //        IDALSubStateManager subStateManager = subStateController as IDALSubStateManager;
                //        _ = subStateManager.ComportEventReceived(comPortEvent, portNumber);
                //    }
                //}
            }
        }

        private void OnQueueEventOcurred()
        {
            //TODO: EventChecker will handle PriorityQueue event dequeuing
        }

        private void LogError(string message, object data)
        {
            string messageId = "Unknown";
            try
            {
                //var request = JsonConvert.DeserializeObject<LinkRequest>(data as string);
                //if (!string.IsNullOrWhiteSpace(request?.MessageID))
                //{
                //    messageId = request.MessageID;
                //}
            }
            finally
            {
                //_ = LoggingClient.LogInfoAsync($"Error from MessageId: '{messageId}':{message}").ConfigureAwait(false);
                Console.WriteLine($"Error from MessageId: '{messageId}':{message}");
            }
        }

        //private LinkRequest LinkRequestResponseError(LinkRequest linkRequest, string codeType, string description)
        //{
        //    if (linkRequest == null)
        //    {
        //        linkRequest = new LinkRequest()
        //        {
        //            LinkObjects = new LinkRequestIPA5Object() { LinkActionResponseList = new List<LinkActionResponse>() },
        //            Actions = new List<LinkActionRequest>() { new LinkActionRequest() { MessageID = "Unknown" } }
        //        };
        //    }
        //    linkRequest.LinkObjects.LinkActionResponseList.Add(new LinkActionResponse()
        //    {
        //        MessageID = linkRequest.Actions[0].MessageID,
        //        Errors = new List<LinkErrorValue>() { new LinkErrorValue() { Code = codeType, Type = codeType, Description = description } }
        //    });
        //    return linkRequest;
        //}

        private void LogMessage(string sendReceive, object data)
        {
            var request = JsonConvert.DeserializeObject<LinkRequest>(data as string);
            string messageId = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(request?.MessageID))
                {
                    messageId = request.MessageID;
                }
            }
            finally
            {
                //_ = LoggingClient.LogInfoAsync($"Request of MessageID: '{messageId}' {sendReceive} Listener.").ConfigureAwait(false);
                Debug.WriteLine($"Request of MessageID: '{messageId}' {sendReceive} Listener.");
            }
        }

        private void InitializeConnectorEvents()
        {
            //    Connector.MessageReceived += ListenerConnector_MessageReceived;
            //    Connector.OfflineConnectivity += ListenerConnector_OfflineConnectivity;
            //    Connector.OnlineConnectivity += ListenerConnector_OnlineConnectivity;
            //    Connector.ChannelClient.ChannelConnected += ChannelClient_ChannelConnected;
            //    Connector.ChannelClient.ChannelDisconnected += ChannelClient_ChannelDisconnected;
            //    Connector.ChannelClient.ChannelReconnected += ChannelClient_ChannelReconnected;
        }

        private void DestroyComportMonitoring()
        {
            if (SerialPortMonitor != null)
            {
                SerialPortMonitor.ComportEventOccured -= OnComPortEventReceivedAsync;
                SerialPortMonitor.StopMonitoring();
            }
        }

        private void DestroyConnectorEvents()
        {
            //    if (Connector != null)
            //    {
            //        Connector.MessageReceived -= ListenerConnector_MessageReceived;
            //        Connector.OfflineConnectivity -= ListenerConnector_OfflineConnectivity;
            //        Connector.OnlineConnectivity -= ListenerConnector_OnlineConnectivity;

            //        if (Connector.ChannelClient != null)
            //        {
            //            Connector.ChannelClient.ChannelConnected -= ChannelClient_ChannelConnected;
            //            Connector.ChannelClient.ChannelDisconnected -= ChannelClient_ChannelDisconnected;
            //            Connector.ChannelClient.ChannelReconnected -= ChannelClient_ChannelReconnected;
            //        }
            //    }
        }

        private void DisconnectFromListener()
        {
            //    Connector.Unsubscribe().Wait(5000);
            //    Connector.Dispose();
            //    Connector = null;
        }

        //private void ChannelClient_ChannelReconnected()
        //{
        //    _ = LoggingClient.LogInfoAsync("DAL is currently reconnected to Listener.");
        //}

        //internal void ChannelClient_ChannelDisconnected(Guid channelId)
        //{
        //    subscribed = false;

        //    _ = LoggingClient.LogInfoAsync($"DAL is currently disconnected from Listener with client id {GetShortClientId(channelId)}");
        //}

        //internal void ChannelClient_ChannelConnected(Guid channelId)
        //{
        //    _ = LoggingClient.LogInfoAsync($"DAL is currently connected to the Listener with client id {GetShortClientId(channelId)}.");

        //    if (!subscribed)
        //    {
        //        Connector.Subscribe(new TopicOption[] { TopicOption.DAL });

        //        subscribed = true;

        //        _ = LoggingClient.LogInfoAsync("DAL is subscribed to the Listener.");
        //    }
        //}

        //private void ListenerConnector_OnlineConnectivity()
        //{
        //    DeviceListenerIsOnline = true;
        //    _ = LoggingClient.LogInfoAsync("Network connectivity is online.");
        //}

        //private void ListenerConnector_OfflineConnectivity()
        //{
        //    DeviceListenerIsOnline = false;
        //    _ = LoggingClient.LogInfoAsync("Network connectivity has gone offline.");
        //}

        //private void ListenerConnector_MessageReceived(Listener.Common.Packets.ListenerPacketHeader header, object message)
        //=> RaiseOnRequestReceived(message as string);

        public void SendDeviceCommand(object message)
            => RaiseOnRequestReceived(message as string);

        //internal void PublishEventHandler(LinkEventResponse.EventTypeType eventType, LinkEventResponse.EventCodeType eventCode,
        //    List<LinkDeviceResponse> devices, LinkRequest request, string message)
        //{
        //    string sessionId = request.Actions?[0]?.SessionID;
        //    if (!string.IsNullOrWhiteSpace(sessionId))
        //    {
        //        try
        //        {
        //            var eventToPublish = ComposeEvent(sessionId, eventType, eventCode, devices, request, new List<string>() { (message ?? string.Empty) }, online: DALListenerIsOnline);
        //            string jsonToPublish = JsonConvert.SerializeObject(eventToPublish);
        //            Connector.Publish(jsonToPublish, new string[] { TopicOption.Event.ToString() });
        //        }
        //        catch (Exception xcp)
        //        {
        //            LoggingClient.LogErrorAsync(xcp.Message);
        //        }
        //    }
        //}

        //internal void PublishEventHandlerAsTask(LinkEventResponse.EventTypeType eventType, LinkEventResponse.EventCodeType eventCode,
        //    List<LinkDeviceResponse> devices, LinkRequest request, string message)
        //{
        //    _ = Task.Run(() => PublishEventHandler(eventType, eventCode, devices, request, message)).ConfigureAwait(false);
        //}

        //public LinkResponse ComposeEvent(string sessionId, LinkEventResponse.EventTypeType eventType,
        //    LinkEventResponse.EventCodeType eventCode, List<LinkDeviceResponse> devices, LinkRequest request, List<string> eventData,
        //    bool online = false)
        //{
        //    var eventResponse = new LinkResponse()
        //    {
        //        MessageID = request?.MessageID,
        //        Responses = new List<LinkActionResponse>(1)
        //         {
        //              new LinkActionResponse()
        //              {
        //                  MessageID = request.Actions?[0].MessageID,
        //                  DALResponse = new LinkDALResponse()
        //                   {
        //                        Devices = devices,
        //                        DALIdentifier = request?.Actions?[0].DALRequest?.DALIdentifier ?? DalIdentifier.GetDALIdentifier(),
        //                        OnlineStatus = online
        //                   },
        //                    EventResponse = new LinkEventResponse()
        //                    {
        //                         EventCode = eventCode.ToString(),
        //                         EventType = eventType.ToString(),
        //                         EventID = Guid.NewGuid(),
        //                         EventData = eventData == null ? null : eventData.ToArray(),
        //                         OrdinalID = 0
        //                     },
        //                    SessionResponse = new LinkSessionResponse()
        //                    {
        //                        SessionID = sessionId
        //                    }
        //               }
        //         }
        //    };

        //    if (eventResponse.Responses[0].DALResponse == null)
        //    {
        //        eventResponse.Responses[0].DALResponse = new LinkDALResponse();
        //    }
        //    if (eventResponse.Responses[0].DALResponse.DALIdentifier == null)
        //    {
        //        eventResponse.Responses[0].DALResponse.DALIdentifier = request.Actions?[0].LinkObjects?.ActionResponse?.DALResponse?.DALIdentifier;
        //    }
        //    if (eventResponse.Responses[0].DALResponse.Devices?[0] == null)
        //    {
        //        eventResponse.Responses[0].DALResponse.Devices = request.Actions?[0].LinkObjects?.ActionResponse?.DALResponse?.Devices ?? new List<LinkDeviceResponse>() { null };
        //    }

        //    return eventResponse;
        //}

        public void DisplayDeviceStatus()
        {
            if (TargetDevices is null || TargetDevices.Count == 0)
            {
                Console.WriteLine("NO DEVICE FOUND!!!");
            }
            else
            {
                Console.WriteLine($"SERIAL: ON PORT={TargetDevices[0].DeviceInformation?.ComPort} - CONNECTION OPEN");
                Console.WriteLine($"DEVICE FOUND: name='{TargetDevices[0]?.Name}', model='{TargetDevices[0]?.DeviceInformation?.Model}', " +
                    $"serial='{TargetDevices[0]?.DeviceInformation?.SerialNumber}'\n");
            }
        }

        public void DeviceStatusUpdate()
        {
            if (deviceNeedsToBeAdded)
            {
                deviceNeedsToBeAdded = false;
                Console.WriteLine();
                DisplayDeviceStatus();
            }
        }

        #region --- state machine management ---

        public Task Complete(IDeviceStateAction state) => AdvanceStateActionTransition(state);

        public void SetWorkflow(LinkDeviceActionType action)
        {
            if (TargetDevices != null)
            {
                LinkRequest linkRequest = new LinkRequest()
                {
                    MessageID = RandomGenerator.BuildRandomString(12),
                    Actions = new List<LinkActionRequest>()
                    {
                        new LinkActionRequest()
                        {
                            Action = LinkAction.DALAction,
                            DeviceActionRequest = new LinkDeviceActionRequest()
                            {
                                DeviceAction = action
                            },
                            DeviceRequest = new LinkDeviceRequest()
                            {
                                DeviceIdentifier = new LinkDeviceIdentifier()
                                {
                                    Manufacturer = TargetDevices[0].DeviceInformation?.Manufacturer,
                                    Model = TargetDevices[0].DeviceInformation?.Model,
                                    SerialNumber = TargetDevices[0].DeviceInformation?.SerialNumber
                                }
                            }
                        }
                    }
                };

                if (action == LinkDeviceActionType.Reboot24Hour)
                {
                    linkRequest.Actions.First().DeviceActionRequest.Reboot24Hour = Configuration.Verifone.Reboot24Hour;
                }
                else if (action == LinkDeviceActionType.SetTerminalDateTime)
                {
                    DateTimeOffset dateNow = DateTimeOffset.UtcNow;
                    string pattern = @"\d";
                    StringBuilder sb = new StringBuilder();

                    // pattern YYYYMMDDHHMMSS
                    foreach (Match m in Regex.Matches(dateNow.ToString("u"), pattern))
                    {
                        sb.Append(m);
                    }
                    // reset seconds
                    if (sb.Length == 14)
                    {
                        sb[12] = '0';
                        sb[13] = '0';
                    }
                    linkRequest.Actions.First().DeviceActionRequest.TerminalDateTime = sb.ToString();
                }

                SendDeviceCommand(JsonConvert.SerializeObject(linkRequest));
            }
            else
            {
                Console.WriteLine("NO TARGET DEVICE IDENTIFIED - CHECK USB/SERIAL CONNECTION SETUP");
            }
        }

        public void LaunchWorkflow() => stateActionController.GetNextAction(DeviceWorkflowState.None).DoWork();

        public DeviceWorkflowState GetCurrentWorkflow()
        {
            return currentStateAction?.WorkflowStateType ?? DeviceWorkflowState.None;
        }

        public void StopWorkflow()
        {
            if (!disposed)
            {
                disposed = true;

                //_ = LoggingClient.LogInfoAsync("Currently shutting down DEVICE Workflow...");
                Console.WriteLine("Currently shutting down DEVICE Workflow...");

                currentStateAction?.Dispose();

                DestroyComportMonitoring();
                DestroyConnectorEvents();
                DisconnectFromListener();

                ExecuteFinalState();
            }
        }

        private async Task AdvanceStateActionTransition(IDeviceStateAction oldState)
        {
            IDeviceStateAction newState = stateActionController.GetNextAction(oldState);

            if (savedStackState.Count > 0)
            {
                newState.SetState(savedStackState.Pop());
            }

            oldState.Dispose();

            currentStateAction = newState;

            LogStateChange(oldState.WorkflowStateType, newState.WorkflowStateType);

            await newState.DoWork();
        }

        private async Task AdvanceActionWithState(IDeviceStateAction oldState)
        {
            IDeviceStateAction newState = stateActionController.GetNextAction(oldState);

            if (savedStackState.Count > 0)
            {
                newState.SetState(savedStackState.Pop());
            }

            oldState.Dispose();

            currentStateAction = newState;

            RaiseStateChange(oldState.WorkflowStateType, newState.WorkflowStateType);

            if (StateActionRules.NeedsDeviceRecovery)
            {
                if (currentStateAction.DoDeviceDiscovery())
                {
                    StateActionRules.NeedsDeviceRecovery = false;
                }
            }

            await newState.DoWork();
        }

        protected void RaiseStateChange(DeviceWorkflowState oldState, DeviceWorkflowState newState)
                    => StateChange?.Invoke(oldState, newState);

        private void ExecuteFinalState()
        {
            using IDeviceStateAction lastAction = stateActionController.GetFinalState();
            lastAction.DoWork().Wait(2000);
        }

        private void LogStateChange(DeviceWorkflowState oldState, DeviceWorkflowState newState)
            //=> _ = LoggingClient.LogInfoAsync($"Workflow State change from '{oldState}' to '{newState}' detected.");
            => Debug.WriteLine($"Workflow State change from '{oldState}' to '{newState}' detected.");

        public async Task Error(IDeviceStateAction state)
        {
            if (state.WorkflowStateType == DeviceWorkflowState.None)
            {
                // TODO: Modify this workflow so that it follows the pattern and simply loops back around
                // to the same final state. In this way, we would run through Shutdown once and then simply
                // decide at that point to stop the workflow because we have no more states to advance to.
                StopWorkflow();
                RaiseOnWorkflowStopped(state.StopReason);
            }
            else
            {
                await AdvanceActionWithState(state);
            }
        }

        public void Dispose() => StopWorkflow();

        #endregion --- state machine management ---
    }
}
