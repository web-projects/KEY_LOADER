﻿using Common.Config.Config;
using Common.LoggerManager;
using Common.XO.Device;
using Common.XO.Private;
using Common.XO.Responses;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Config;
using Devices.Common.Helpers;
using Devices.Verifone.Connection;
using Devices.Verifone.Helpers;
using Devices.Verifone.TLV;
using Devices.Verifone.VIPA.Templates;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Devices.Verifone.Helpers.Messages;

namespace Devices.Verifone.VIPA
{
    public class VIPAImpl : IVIPADevice, IDisposable
    {
        #region --- enumerations ---
        public enum VIPADisplayMessageValue
        {
            Custom = 0x00,
            Idle = 0x01,
            ProcessingTransaction = 0x02,
            Authorising = 0x03,
            RequestRejected = 0x04,
            InsertCardWithBeeps = 0x0D,
            RemoveCardWithBeeps = 0x0E,
            Processing = 0x0F
        }
        private enum ResetDeviceCfg
        {
            ReturnSerialNumber = 1 << 0,
            ScreenDisplayState = 1 << 1,
            SlideShowStartsNormalTiming = 1 << 2,
            BeepDuringReset = 1 << 3,
            ResetImmediately = 1 << 4,
            NoAdditionalInformation = 1 << 5,
            ReturnPinpadConfiguration = 1 << 6,
            AddVOSComponentsInformation = 1 << 7
        }

        #endregion --- enumerations ---

        #region --- attributes ---
        public DeviceInformation DeviceInformation { get; set; }

        // Optimal Packet Size for READ/WRITE operations on device
        const int PACKET_SIZE = 1024;

        private int ResponseTagsHandlerSubscribed = 0;

        public TaskCompletionSource<int> ResponseCodeResult = null;

        public delegate void ResponseTagsHandlerDelegate(List<TLVImpl> tags, int responseCode, bool cancelled = false);
        internal ResponseTagsHandlerDelegate ResponseTagsHandler = null;

        public delegate void ResponseTaglessHandlerDelegate(byte[] data, int responseCode, bool cancelled = false);
        internal ResponseTaglessHandlerDelegate ResponseTaglessHandler = null;

        public delegate void ResponseCLessHandlerDelegate(List<TLVImpl> tags, int responseCode, int pcb, bool cancelled = false);
        internal ResponseCLessHandlerDelegate ResponseCLessHandler = null;

        public TaskCompletionSource<(DevicePTID devicePTID, int VipaResponse)> DeviceResetConfiguration = null;

        public TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)> DeviceIdentifier = null;
        public TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)> DeviceSecurityConfiguration = null;
        public TaskCompletionSource<(KernelConfigurationObject kernelConfigurationObject, int VipaResponse)> DeviceKernelConfiguration = null;

        public TaskCompletionSource<(string HMAC, int VipaResponse)> DeviceGenerateHMAC = null;
        public TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)> DeviceBinaryStatusInformation = null;

        public TaskCompletionSource<(LinkDALRequestIPA5Object linkDALRequestIPA5Object, int VipaResponse)> DeviceInteractionInformation { get; set; } = null;

        public TaskCompletionSource<(string Timestamp, int VipaResponse)> Reboot24HourInformation = null;

        public TaskCompletionSource<(string Timestamp, int VipaResponse)> TerminalDateTimeInformation = null;

        #endregion --- attributes ---

        #region --- connection ---
        private SerialConnection serialConnection { get; set; }

        public bool Connect(SerialConnection connection, DeviceInformation deviceInformation)
        {
            serialConnection = connection;
            DeviceInformation = deviceInformation;
            return serialConnection.Connect(DeviceInformation.ComPort);
        }

        public void Dispose()
        {
            serialConnection?.Dispose();
        }

        #endregion --- connection ---

        #region --- resources ---
        private bool FindEmbeddedResourceByName(string fileName, string fileTarget)
        {
            bool result = false;

            // Main Assembly contains embedded resources
            Assembly mainAssembly = Assembly.GetEntryAssembly();
            foreach (string name in mainAssembly.GetManifestResourceNames())
            {
                if (name.EndsWith(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    using (Stream stream = mainAssembly.GetManifestResourceStream(name))
                    {
                        BinaryReader br = new BinaryReader(stream);
                        // always create working file
                        FileStream fs = File.Open(fileTarget, FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        byte[] ba = new byte[stream.Length];
                        stream.Read(ba, 0, ba.Length);
                        bw.Write(ba);
                        br.Close();
                        bw.Close();
                        stream.Close();
                        result = true;
                    }
                    break;

                }
            }
            return result;
        }

        #endregion --- resources ---

        private void WriteSingleCmd(VIPACommand command)
        {
            serialConnection?.WriteSingleCmd(new VIPAResponseHandlers
            {
                responsetagshandler = ResponseTagsHandler,
                responsetaglesshandler = ResponseTaglessHandler,
                responsecontactlesshandler = ResponseCLessHandler
            }, command);
        }

        private void WriteRawBytes(byte[] buffer)
        {
            serialConnection?.WriteRaw(buffer);
        }

        private void SendVipaCommand(VIPACommandType commandType, byte p1, byte p2, byte[] data = null, byte nad = 0x1, byte pcb = 0x0)
        {
            Debug.WriteLine($"Send VIPA Command:[{commandType}]");
            VIPACommand command = new VIPACommand(commandType) { nad = nad, pcb = pcb, p1 = p1, p2 = p2, data = data };
            WriteSingleCmd(command);
        }

        #region --- VIPA commands ---
        public bool DisplayMessage(VIPADisplayMessageValue displayMessageValue = VIPADisplayMessageValue.Idle, bool enableBacklight = false, string customMessage = "")
        {
            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            SendVipaCommand(VIPACommandType.Display, (byte)displayMessageValue, (byte)(enableBacklight ? 0x01 : 0x00), Encoding.ASCII.GetBytes(customMessage));

            int displayCommandResponseCode = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return displayCommandResponseCode == (int)VipaSW1SW2Codes.Success;
        }

        internal (int VipaData, int VipaResponse) DeviceCommandAbort()
        {
            (int VipaData, int VipaResponse) deviceResponse = (-1, (int)VipaSW1SW2Codes.Failure);

            ResponseCodeResult = new TaskCompletionSource<int>();

            DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);
            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            SendVipaCommand(VIPACommandType.Abort, 0x00, 0x00);

            deviceResponse = ((int)VipaSW1SW2Codes.Success, ResponseCodeResult.Task.Result);

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceResponse;
        }

        public (DeviceInfoObject deviceInfoObject, int VipaResponse) VIPARestart()
        {
            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);

            // abort previous user entries in progress
            (int VipaData, int VipaResponse) vipaResult = DeviceCommandAbort();

            if (vipaResult.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);

                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += GetDeviceInfoResponseHandler;

                // VIPA restart with beep
                byte p2 = (byte)(ResetDeviceCfg.ReturnSerialNumber | ResetDeviceCfg.ScreenDisplayState | ResetDeviceCfg.BeepDuringReset);
                SendVipaCommand(VIPACommandType.ResetDevice, 0x02, p2);

                deviceResponse = DeviceIdentifier.Task.Result;

                ResponseTagsHandler -= GetDeviceInfoResponseHandler;
                ResponseTagsHandlerSubscribed--;
            }

            return deviceResponse;
        }

        public (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceCommandReset()
        {
            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);

            // abort previous user entries in progress
            (int VipaData, int VipaResponse) vipaResult = DeviceCommandAbort();

            if (vipaResult.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);

                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += GetDeviceInfoResponseHandler;

                SendVipaCommand(VIPACommandType.ResetDevice, 0x00,
                    (byte)(ResetDeviceCfg.ReturnSerialNumber | ResetDeviceCfg.ReturnPinpadConfiguration));

                deviceResponse = DeviceIdentifier.Task.Result;

                ResponseTagsHandler -= GetDeviceInfoResponseHandler;
                ResponseTagsHandlerSubscribed--;
            }

            return deviceResponse;
        }

        public (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceExtendedReset()
        {
            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);

            // abort previous user entries in progress
            (int VipaData, int VipaResponse) vipaResult = DeviceCommandAbort();

            if (vipaResult.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);

                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += GetDeviceInfoResponseHandler;

                // Bit  1 – 0 PTID in serial response
                //        – 1 PTID plus serial number (tag 9F1E) in serial response
                //        - The following flags are only taken into account when P1 = 0x00:
                // Bit  2 - 0 — Leave screen display unchanged, 1 — Clear screen display to idle display state
                // Bit  3 - 0 — Slide show starts with normal timing, 1 — Start Slide-Show as soon as possible
                // Bit  4 - 0 — No beep, 1 — Beep during reset as audio indicator
                // Bit  5 - 0 — ‘Immediate’ reset, 1 — Card Removal delayed reset
                // Bit  6 - 1 — Do not add any information in the response, except serial number if Bit 1 is set.
                // Bit  7 - 0 — Do not return PinPad configuration, 1 — return PinPad configuration (warning: it can take a few seconds)
                // Bit  8 - 1 — Add V/OS components information (Vault, OpenProtocol, OS_SRED, AppManager) to response (V/OS only).
                // Bit  9 – 1 - Force contact EMV configuration reload
                // Bit 10 – 1 – Force contactless EMV configuration reload
                // Bit 11 – 1 – Force contactless CAPK reload
                // Bit 12 – 1 – Returns OS components version (requires OS supporting this feature)
                // Bit 13 - 1 - Return communication mode (tag DFA21F) (0 - SERIAL, 1 - TCPIP, 3 - USB, 4 - BT, 5
                //            - PIPE_INTERNAL, 6 - WIFI, 7 - GPRS)
                // Bit 14 - 1 - Connect to external pinpad (PP1000SEV3) and set EXTERNAL_PINPAD to ON
                // Bit 15 - 1 - Disconnect external pinpad (PP1000SEV3) and set EXTERNAL_PINPAD to OFF
                var dataForReset = new List<TLVImpl>
                {
                    new TLVImpl
                    {
                        Tag = new byte[] { 0xE0 },
                        InnerTags = new List<TLVImpl>
                        {
                            new TLVImpl(new byte[] { 0xDF, 0xED, 0x0D }, new byte[] { 0x0F, 0x0F })
                        }
                    }
                };

                byte[] dataForResetData = TLVImpl.Encode(dataForReset);

                SendVipaCommand(VIPACommandType.ExtendedSoftwareResetDevice, 0x00, 0x00, dataForResetData);

                deviceResponse = DeviceIdentifier.Task.Result;

                ResponseTagsHandler -= GetDeviceInfoResponseHandler;
                ResponseTagsHandlerSubscribed--;
            }

            return deviceResponse;
        }

        private (DevicePTID devicePTID, int VipaResponse) DeviceRebootWithResponse()
        {
            (DevicePTID devicePTID, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);
            DeviceResetConfiguration = new TaskCompletionSource<(DevicePTID devicePTID, int VipaResponse)>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += DeviceResetResponseHandler;

            SendVipaCommand(VIPACommandType.ResetDevice, 0x01, 0x03);

            deviceResponse = DeviceResetConfiguration.Task.Result;

            ResponseTagsHandler -= DeviceResetResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceResponse;
        }

        private (DevicePTID devicePTID, int VipaResponse) DeviceRebootWithoutResponse()
        {
            (DevicePTID devicePTID, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            SendVipaCommand(VIPACommandType.ResetDevice, 0x01, 0x00);

            ResponseCodeResult = new TaskCompletionSource<int>();

            deviceResponse = (null, (int)VipaSW1SW2Codes.Success);

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceResponse;
        }

        public (DevicePTID devicePTID, int VipaResponse) DeviceReboot()
        {
            return DeviceRebootWithoutResponse();
        }

        public (int VipaResult, int VipaResponse) GetActiveKeySlot()
        {
            // check for access to the file
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = GetBinaryStatus(BinaryStatusObject.MAPP_SRED_CONFIG);

            // When the file cannot be accessed, VIPA returns SW1SW2 equal to 9F13
            if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
            {
                Console.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                    BinaryStatusObject.MAPP_SRED_CONFIG, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));
                return (-1, fileStatus.VipaResponse);
            }

            // Setup for FILE OPERATIONS
            fileStatus = SelectFileForOps(BinaryStatusObject.MAPP_SRED_CONFIG);
            if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
            {
                Console.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                    BinaryStatusObject.MAPP_SRED_CONFIG, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));
                return (-1, fileStatus.VipaResponse);
            }

            // Read File Contents at OFFSET 240
            fileStatus = ReadBinaryDataFromSelectedFile(0xF0, 0x20);
            if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
            {
                Console.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                    BinaryStatusObject.MAPP_SRED_CONFIG, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));

                // Clean up pool allocation, clearing the array
                if (fileStatus.binaryStatusObject.ReadResponseBytes != null)
                {
                    ArrayPool<byte>.Shared.Return(fileStatus.binaryStatusObject.ReadResponseBytes, true);
                }

                return (-1, fileStatus.VipaResponse);
            }

            (int VipaResult, int VipaResponse) response = (-1, (int)VipaSW1SW2Codes.Success);

            // Obtain SLOT number
            string slotReported = Encoding.UTF8.GetString(fileStatus.binaryStatusObject.ReadResponseBytes);
            MatchCollection match = Regex.Matches(slotReported, "slot=[0-9]", RegexOptions.Compiled);
            if (match.Count == 1)
            {
                string[] result = match[0].Value.Split('=');
                if (result.Length == 2)
                {
                    response.VipaResult = Convert.ToInt32(result[1]);
                }
            }

            // Clean up pool allocation, clearing the array
            ArrayPool<byte>.Shared.Return(fileStatus.binaryStatusObject.ReadResponseBytes, true);

            return response;
        }

        public (KernelConfigurationObject kernelConfigurationObject, int VipaResponse) GetEMVKernelChecksum()
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetKernelInformationResponseHandler;

            DeviceKernelConfiguration = new TaskCompletionSource<(KernelConfigurationObject kernelConfigurationObject, int VipaResponse)>();

            var aidRequestedTransaction = new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(new byte[] { 0x9F, 0x06, 0x0E }, /* AID A000000003101001 */ new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x03, 0x10, 0x10 })
                    }
                }
            };
            var aidRequestedTransactionData = TLVImpl.Encode(aidRequestedTransaction);

            SendVipaCommand(VIPACommandType.GetEMVHashValues, 0x00, 0x00, aidRequestedTransactionData);

            var deviceKernelConfigurationInfo = DeviceKernelConfiguration.Task.Result;

            ResponseTagsHandler -= GetKernelInformationResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceKernelConfigurationInfo;
        }

        public (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetSecurityConfiguration(byte hostID, byte vssSlot)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetSecurityInformationResponseHandler;

            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            SendVipaCommand(VIPACommandType.GetSecurityConfiguration, hostID, vssSlot);

            var deviceSecurityConfigurationInfo = DeviceSecurityConfiguration.Task.Result;

            ResponseTagsHandler -= GetSecurityInformationResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceSecurityConfigurationInfo;
        }

        //[Obsolete]
        public int ConfigurationFiles(string deviceModel)
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);

            Debug.WriteLine(ConsoleMessages.UpdateDeviceUpdate.GetStringValue());

            bool IsEngageDevice = BinaryStatusObject.ENGAGE_DEVICES.Any(x => x.Contains(deviceModel.Substring(0, 4)));

            foreach (var configFile in BinaryStatusObject.binaryStatus)
            {
                // search for partial matches in P200 vs P200Plus
                if (configFile.Value.deviceTypes.Any(x => x.Contains(deviceModel.Substring(0, 4))))
                {
                    string fileName = configFile.Value.fileName;
                    if (BinaryStatusObject.EMV_CONFIG_FILES.Any(x => x.Contains(configFile.Value.fileName)))
                    {
                        fileName = (IsEngageDevice ? "ENGAGE." : "UX301.") + configFile.Value.fileName;
                    }

                    string targetFile = Path.Combine(Constants.TargetDirectory, configFile.Value.fileName);
                    if (FindEmbeddedResourceByName(fileName, targetFile))
                    {
                        fileStatus = PutFile(configFile.Value.fileName, targetFile);
                        if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                        {
                            if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                            {
                                if (fileStatus.binaryStatusObject.FileSize == configFile.Value.fileSize ||
                                    fileStatus.binaryStatusObject.FileSize == configFile.Value.reBooted.size)
                                {
                                    string formattedStr = string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13));
                                    //Console.WriteLine(formattedStr);
                                    Console.Write(string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13)));
                                }
                                else
                                {
                                    Console.WriteLine($"VIPA: {configFile.Value.fileName} SIZE MISMATCH!");
                                }

                                if (fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase) ||
                                    fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.reBooted.hash, StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine(", HASH MATCH");
                                }
                                else
                                {
                                    Console.WriteLine($", HASH MISMATCH!");
                                }
                            }
                        }
                        else
                        {
                            string formattedStr = string.Format("VIPA: FILE '{0}' FAILED TRANSFERRED WITH ERROR=0x{1:X4}",
                                configFile.Value.fileName.PadRight(13), fileStatus.VipaResponse);
                            Console.WriteLine(formattedStr);
                        }
                        // clean up
                        if (File.Exists(targetFile))
                        {
                            File.Delete(targetFile);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: RESOURCE '{configFile.Value.fileName}' NOT FOUND!");
                    }
                }
            }

            return fileStatus.VipaResponse;
        }

        public int ConfigurationPackage(string deviceModel, bool activeSigningMethodIsSphere)
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);

            Debug.WriteLine(ConsoleMessages.UpdateDeviceUpdate.GetStringValue());

            bool IsEngageDevice = BinaryStatusObject.ENGAGE_DEVICES.Any(x => x.Contains(deviceModel.Substring(0, 4)));

            foreach (var configFile in BinaryStatusObject.configurationPackages)
            {
                // search for partial matches in P200 vs P200Plus
                if (configFile.Value.deviceTypes.Any(x => x.Contains(deviceModel.Substring(0, 4))))
                {
                    // validate signing method
                    if (activeSigningMethodIsSphere)
                    {
                        if (!configFile.Value.fileName.StartsWith("sphere.sphere"))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!configFile.Value.fileName.StartsWith("verifone.njt"))
                        {
                            continue;
                        }
                    }

                    string fileName = configFile.Value.fileName;
                    if (BinaryStatusObject.EMV_CONFIG_FILES.Any(x => x.Contains(configFile.Value.fileName)))
                    {
                        fileName = (IsEngageDevice ? "ENGAGE." : "UX301.") + configFile.Value.fileName;
                    }

                    string targetFile = Path.Combine(Constants.TargetDirectory, configFile.Value.fileName);
                    if (FindEmbeddedResourceByName(fileName, targetFile))
                    {
                        fileStatus = PutFile(configFile.Value.fileName, targetFile);
                        if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                        {
                            if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                            {
                                if (fileStatus.binaryStatusObject.FileSize == configFile.Value.fileSize)
                                {
                                    string formattedStr = string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13));
                                    //Console.WriteLine(formattedStr);
                                    Console.Write(string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13)));
                                }
                                else
                                {
                                    Console.WriteLine($"VIPA: {configFile.Value.fileName} SIZE MISMATCH!");
                                }

                                if (fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine(", HASH MATCH");
                                }
                                else
                                {
                                    Console.WriteLine($", HASH MISMATCH!");
                                }
                            }
                        }
                        else
                        {
                            string formattedStr = string.Format("VIPA: FILE '{0}' FAILED TRANSFERRED WITH ERROR=0x{1:X4}",
                                configFile.Value.fileName.PadRight(13), fileStatus.VipaResponse);
                            Console.WriteLine(formattedStr);
                        }
                        // clean up
                        if (File.Exists(targetFile))
                        {
                            File.Delete(targetFile);
                        }

                        break;
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: RESOURCE '{configFile.Value.fileName}' NOT FOUND!");
                    }
                }
            }

            return fileStatus.VipaResponse;
        }

        public int ValidateConfiguration(string deviceModel, bool activeSigningMethodIsSphere)
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);

            foreach (var configFile in BinaryStatusObject.binaryStatus)
            {
                // search for partial matches in P200 vs P200Plus
                if (configFile.Value.deviceTypes.Any(x => x.Contains(deviceModel.Substring(0, 4))))
                {
                    fileStatus = GetBinaryStatus(configFile.Value.fileName);
                    Debug.WriteLine($"VIPA: RESOURCE '{configFile.Value.fileName}' STATUS=0x{string.Format("{0:X4}", fileStatus.VipaResponse)}");
                    if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
                    {
                        break;
                    }
                    // 20201012 - ONLY CHECK FOR FILE PRESENCE
                    Debug.WriteLine("FILE FOUND !!!");
                    // FILE SIZE
                    //if (fileStatus.binaryStatusObject.FileSize == configFile.Value.fileSize ||
                    //    fileStatus.binaryStatusObject.FileSize == configFile.Value.reBooted.size)
                    //{
                    //    string formattedStr = string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13));
                    //    Debug.Write(string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13)));
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"VIPA: {configFile.Value.fileName} SIZE MISMATCH!");
                    //    fileStatus.VipaResponse = (int)VipaSW1SW2Codes.Failure;
                    //    break;
                    //}
                    //// HASH
                    //if (fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase) ||
                    //    fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.reBooted.hash, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    Debug.WriteLine(", HASH MATCH");
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($", HASH MISMATCH!");
                    //    fileStatus.VipaResponse = (int)VipaSW1SW2Codes.Failure;
                    //    break;
                    //}
                }
            }
            return fileStatus.VipaResponse;
        }

        public int FeatureEnablementToken()
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);
            Debug.WriteLine(ConsoleMessages.UpdateDeviceUpdate.GetStringValue());
            string targetFile = Path.Combine(Constants.TargetDirectory, BinaryStatusObject.FET_BUNDLE);
            if (FindEmbeddedResourceByName(BinaryStatusObject.FET_BUNDLE, targetFile))
            {
                fileStatus = PutFile(BinaryStatusObject.FET_BUNDLE, targetFile);
                if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                {
                    if (fileStatus.binaryStatusObject.FileSize == BinaryStatusObject.FET_SIZE)
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.FET_BUNDLE} SIZE MATCH");
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.FET_BUNDLE} SIZE MISMATCH!");
                    }

                    if (fileStatus.binaryStatusObject.FileCheckSum.Equals(BinaryStatusObject.FET_HASH, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.FET_BUNDLE} HASH MATCH");
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.FET_BUNDLE} HASH MISMATCH!");
                    }
                }
                // clean up
                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }
            }
            else
            {
                Console.WriteLine($"VIPA: RESOURCE '{BinaryStatusObject.FET_BUNDLE}' NOT FOUND!");
            }
            return fileStatus.VipaResponse;
        }

        private int LockDeviceConfiguration(Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)> configurationBundle,
            bool activeConfigurationIsEpic, bool activeSigningMethodIsSphere)
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);
            foreach (var configFile in configurationBundle)
            {
                bool configurationBundleMatches = activeConfigurationIsEpic ? configFile.Key.Contains("EPIC") : configFile.Key.Contains("NJT");
                if (DeviceInformation.FirmwareVersion.StartsWith(configFile.Value.configType, StringComparison.OrdinalIgnoreCase) && configurationBundleMatches)
                {
                    // validate signing method
                    if (activeSigningMethodIsSphere)
                    {
                        if (activeConfigurationIsEpic)
                        { 
                            if (!configFile.Value.fileName.StartsWith("sphere.sphere"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!configFile.Value.fileName.StartsWith("sphere.njt"))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (!configFile.Value.fileName.StartsWith("verifone.njt"))
                        {
                            continue;
                        }
                    }

                    string fileName = configFile.Value.fileName;
                    string targetFile = Path.Combine(Constants.TargetDirectory, configFile.Value.fileName);
                    if (FindEmbeddedResourceByName(fileName, targetFile))
                    {

                        fileStatus = PutFile(configFile.Value.fileName, targetFile);
                        if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                        {
                            if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                            {
                                if (fileStatus.binaryStatusObject.FileSize == configFile.Value.fileSize)
                                {
                                    string formattedStr = string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13));
                                    //Console.WriteLine(formattedStr);
                                    Console.Write(string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13)));
                                }
                                else
                                {
                                    Console.WriteLine($"VIPA: {configFile.Value.fileName} SIZE MISMATCH!");
                                }

                                if (fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine(", HASH MATCH");
                                }
                                else
                                {
                                    Console.WriteLine($", HASH MISMATCH!");
                                }
                            }
                        }
                        else
                        {
                            string formattedStr = string.Format("VIPA: FILE '{0}' FAILED TRANSFERRED WITH ERROR=0x{1:X4}",
                                configFile.Value.fileName.PadRight(13), fileStatus.VipaResponse);
                            Console.WriteLine(formattedStr);
                        }
                        // clean up
                        if (File.Exists(targetFile))
                        {
                            File.Delete(targetFile);
                        }

                        break;
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: RESOURCE '{configFile.Value.fileName}' NOT FOUND!");
                    }
                }
            }
            return fileStatus.VipaResponse;
        }

        public int LockDeviceConfiguration0(bool activeConfigurationIsEpic, bool activeSigningMethodIsSphere)
        {
            return LockDeviceConfiguration(BinaryStatusObject.configBundlesSlot0, activeConfigurationIsEpic, activeSigningMethodIsSphere);
        }

        public int LockDeviceConfiguration8(bool activeConfigurationIsEpic, bool activeSigningMethodIsSphere)
        {
            return LockDeviceConfiguration(BinaryStatusObject.configBundlesSlot8, activeConfigurationIsEpic, activeSigningMethodIsSphere);
        }

        public int UnlockDeviceConfiguration()
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);
            Debug.WriteLine(ConsoleMessages.UnlockDeviceUpdate.GetStringValue());
            string targetFile = Path.Combine(Constants.TargetDirectory, BinaryStatusObject.UNLOCK_CONFIG_BUNDLE);
            if (FindEmbeddedResourceByName(BinaryStatusObject.UNLOCK_CONFIG_BUNDLE, targetFile))
            {
                fileStatus = PutFile(BinaryStatusObject.UNLOCK_CONFIG_BUNDLE, targetFile);
                if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                {
                    if (fileStatus.binaryStatusObject.FileSize == BinaryStatusObject.UNLOCK_CONFIG_SIZE)
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.UNLOCK_CONFIG_BUNDLE} SIZE MATCH");
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.UNLOCK_CONFIG_BUNDLE} SIZE MISMATCH!");
                    }

                    if (fileStatus.binaryStatusObject.FileCheckSum.Equals(BinaryStatusObject.UNLOCK_CONFIG_HASH, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.UNLOCK_CONFIG_BUNDLE} HASH MATCH");
                    }
                    else
                    {
                        Console.WriteLine($"VIPA: {BinaryStatusObject.UNLOCK_CONFIG_BUNDLE} HASH MISMATCH!");
                    }
                }
            }
            else
            {
                Console.WriteLine($"VIPA: RESOURCE '{BinaryStatusObject.UNLOCK_CONFIG_BUNDLE}' NOT FOUND!");
            }
            return fileStatus.VipaResponse;
        }

        public (string HMAC, int VipaResponse) GenerateHMAC()
        {
            CancelResponseHandlers();

            (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) securityConfig = (new SecurityConfigurationObject(), 0);

            // HostId 06
            securityConfig = GetGeneratedHMAC(securityConfig.securityConfigurationObject.PrimarySlot,
                            HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryPANSalt), HMACValidator.MACSecondaryKeyHASH));

            if (securityConfig.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                if (securityConfig.securityConfigurationObject.GeneratedHMAC.Equals(HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH),
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    // HostId 07
                    securityConfig = GetGeneratedHMAC(securityConfig.securityConfigurationObject.SecondarySlot, securityConfig.securityConfigurationObject.GeneratedHMAC);
                    if (securityConfig.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        if (securityConfig.securityConfigurationObject.GeneratedHMAC.Equals(HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACSecondaryHASHSalt), HMACValidator.MACPrimaryKeyHASH),
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("DEVICE: HMAC IS VALID +++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: HMAC SECONDARY SLOT MISMATCH=0x{0:X}", securityConfig.securityConfigurationObject.GeneratedHMAC));
                        }
                    }
                    else
                    {
                        Console.WriteLine(string.Format("DEVICE: HMAC PRIMARY SLOT MISMATCH=0x{0:X}", securityConfig.securityConfigurationObject.GeneratedHMAC));
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("DEVICE: HMAC PRIMARY SLOT MISMATCH=0x{0:X}", securityConfig.securityConfigurationObject.GeneratedHMAC));
                }
            }
            else
            {
                Console.WriteLine(string.Format("DEVICE: HMAC GENERATION FAILED WITH ERROR=0x{0:X}", securityConfig.VipaResponse));
            }

            return (securityConfig.securityConfigurationObject?.GeneratedHMAC, securityConfig.VipaResponse);
        }

        private (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetGeneratedHMAC(int hostID, string MAC)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetGeneratedHMACResponseHandler;

            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            var dataForHMAC = new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x0E }, ConversionHelper.HexToByteArray(MAC)),
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x23 }, new byte[] { Convert.ToByte(hostID) } )
                    }
                }
            };
            var dataForHMACData = TLVImpl.Encode(dataForHMAC);

            SendVipaCommand(VIPACommandType.GenerateHMAC, 0x00, 0x00, dataForHMACData);

            var deviceSecurityConfigurationInfo = DeviceSecurityConfiguration.Task.Result;

            ResponseTagsHandler -= GetGeneratedHMACResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceSecurityConfigurationInfo;
        }

        public int UpdateHMACKeys()
        {
            string generatedHMAC = GetCurrentKSNHMAC();

            // KEY 06 Generation
            byte[] hmac_generated_key = ConversionHelper.HexToByteArray(generatedHMAC);

            // Signature = HMAC_old(old XOR new) - array1 is smaller or equal in size as array2
            byte[] hmac_signature_06 = ConversionHelper.XORArrays(hmac_generated_key, HMACValidator.HMACKEY06);

            var dataKey06HMAC = FormatE0Tag(HMACValidator.HMACKEY06, hmac_signature_06);
            byte[] dataForHMACData = TLVImpl.Encode(dataKey06HMAC);

            // key slot 06
            int vipaResponse = UpdateHMACKey(0x06, dataForHMACData);

            if (vipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                // KEY 07 Generation
                byte[] hmac_signature_07 = ConversionHelper.XORArrays(hmac_generated_key, HMACValidator.HMACKEY07);

                var dataKey07HMAC = FormatE0Tag(HMACValidator.HMACKEY07, hmac_signature_07);

                dataForHMACData = TLVImpl.Encode(dataKey07HMAC);

                // key slot 07
                vipaResponse = UpdateHMACKey(0x07, dataForHMACData);
            }

            return vipaResponse;
        }

        private int IdleScreenUpdate(Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)> configObject,
            string deviceModel, bool activeSigningMethodIsSphere)
        {
            (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = (null, (int)VipaSW1SW2Codes.Failure);

            Debug.WriteLine(ConsoleMessages.UpdateIdleScreen.GetStringValue());

            bool IsEngageDevice = BinaryStatusObject.ENGAGE_DEVICES.Any(x => x.Contains(deviceModel.Substring(0, 4)));

            if (IsEngageDevice)
            {
                foreach (var configFile in configObject)
                {
                    // search for partial matches in P200 vs P200Plus
                    if (configFile.Value.deviceTypes.Any(x => x.Contains(deviceModel.Substring(0, 4))))
                    {
                        // validate signing method
                        if (activeSigningMethodIsSphere)
                        {
                            if (!configFile.Value.fileName.StartsWith("sphere.sphere"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!configFile.Value.fileName.StartsWith("verifone.njt"))
                            {
                                continue;
                            }
                        }
                        string targetFile = Path.Combine(Constants.TargetDirectory, configFile.Value.fileName);
                        if (FindEmbeddedResourceByName(configFile.Value.fileName, targetFile))
                        {
                            fileStatus = PutFile(configFile.Value.fileTargetName, targetFile);
                            if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                            {
                                if (fileStatus.VipaResponse == (int)VipaSW1SW2Codes.Success && fileStatus.binaryStatusObject != null)
                                {
                                    if (fileStatus.binaryStatusObject.FileSize == configFile.Value.fileSize)
                                    {
                                        string formattedStr = string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13));
                                        //Console.WriteLine(formattedStr);
                                        Console.Write(string.Format("VIPA: '{0}' SIZE MATCH", configFile.Value.fileName.PadRight(13)));
                                    }
                                    else
                                    {
                                        Console.WriteLine($"VIPA: {configFile.Value.fileName} SIZE MISMATCH!");
                                    }

                                    if (fileStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Console.WriteLine(", HASH MATCH");
                                    }
                                    else
                                    {
                                        Console.WriteLine($", HASH MISMATCH!");
                                    }
                                }
                            }
                            else
                            {
                                string formattedStr = string.Format("VIPA: FILE '{0}' FAILED TRANSFERRED WITH ERROR=0x{1:X4}",
                                    configFile.Value.fileName.PadRight(13), fileStatus.VipaResponse);
                                Console.WriteLine(formattedStr);
                            }
                            // clean up
                            if (File.Exists(targetFile))
                            {
                                File.Delete(targetFile);
                            }

                            break;
                        }
                        else
                        {
                            Console.WriteLine($"VIPA: RESOURCE '{configFile.Value.fileName}' NOT FOUND!");
                        }
                    }
                }
            }
            else
            {
                return (int)VipaSW1SW2Codes.DeviceNotSupported;
            }

            return fileStatus.VipaResponse;
        }

        public int UpdateIdleScreen(string deviceModel, bool activeSigningMethodIsSphere, string activeCustomerId) => activeCustomerId switch
        {
            "199" => IdleScreenUpdate(BinaryStatusObject.RaptorIdleScreenTGZ_199, deviceModel, activeSigningMethodIsSphere),
            "250" => IdleScreenUpdate(BinaryStatusObject.RaptorIdleScreenTGZ_250, deviceModel, activeSigningMethodIsSphere),
            _ => throw new Exception($"Invalid customer identifier '{activeCustomerId}'.")
        };

        public (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) DisplayCustomScreen(string displayMessage)
        {
            Debug.WriteLine(ConsoleMessages.DisplayCustomScreen.GetStringValue());

            (int vipaResponse, int vipaData) verifyResult = VerifyAmountScreen(displayMessage);
            LinkDALRequestIPA5Object linkActionRequestIPA5Object = new LinkDALRequestIPA5Object()
            {
                DALResponseData = new LinkDALActionResponse()
                {
                    Value = verifyResult.vipaData.ToString()
                }
            };
            return (linkActionRequestIPA5Object, verifyResult.vipaResponse);
        }

        public (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) DisplayCustomScreenHTML(string displayMessage)
        {
            Debug.WriteLine(ConsoleMessages.DisplayCustomScreenHTML.GetStringValue());

            (int vipaResponse, int vipaData) verifyResult = VerifyAmountScreenHTML(displayMessage);
            LinkDALRequestIPA5Object linkActionRequestIPA5Object = new LinkDALRequestIPA5Object()
            {
                DALResponseData = new LinkDALActionResponse()
                {
                    Value = verifyResult.vipaData.ToString()
                }
            };
            return (linkActionRequestIPA5Object, verifyResult.vipaResponse);
        }

        private LinkDALRequestIPA5Object ReportVIPAVersions(Dictionary<string, (string configVersion, BinaryStatusObject.DeviceConfigurationTypes configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)> configObject,
            string deviceModel, string activeCustomerId)
        {
            Debug.WriteLine(ConsoleMessages.VIPAVersions.GetStringValue());

            LinkDALRequestIPA5Object linkActionRequestIPA5Object = new LinkDALRequestIPA5Object()
            {
                DALCdbData = new DALCDBData()
                {
                    VIPAVersion = new DALBundleVersioning(),
                    EMVVersion = new DALBundleVersioning(),
                    IdleVersion = new DALBundleVersioning()
                }
            };

            const string bundleNotFound = "NONE";
            Dictionary<string, string> versions = new Dictionary<string, string>();

            foreach (var configFile in configObject)
            {
                // VIPA version matching
                if (configFile.Value.configVersion.Equals(DeviceInformation.FirmwareVersion, StringComparison.OrdinalIgnoreCase))
                {
                    // Device model matching
                    if (!configFile.Value.deviceTypes.Any(x => x.Contains(deviceModel.Substring(0, 4))))
                    {
                        continue;
                    }

                    // Configuration type matching for idle screens
                    if (configFile.Value.configType == BinaryStatusObject.DeviceConfigurationTypes.IdleConfiguration)
                    {
                        if (!configFile.Key.Contains(activeCustomerId))
                        {
                            continue;
                        }
                    }

                    Debug.WriteLine($"VIPA: PROCESSING FILE=[{configFile.Value.fileName}]");

                    // assume version string is not found
                    versions[configFile.Value.fileName] = bundleNotFound;

                    // check for access to the file
                    (BinaryStatusObject binaryStatusObject, int VipaResponse) fileStatus = GetBinaryStatus(configFile.Value.fileName);

                    // When the file cannot be accessed, VIPA returns SW1SW2 equal to 9F13
                    if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
                    {
                        Debug.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                            configFile.Value.fileName, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));
                        continue;
                    }
                    else if (fileStatus.binaryStatusObject?.FileSize == 0x00)
                    {
                        Debug.WriteLine(string.Format("VIPA {0} SIZE=0x0000 ERROR=0x{1:X4} - '{2}'",
                            configFile.Value.fileName, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));
                        continue;
                    }

                    // Setup for FILE OPERATIONS
                    fileStatus = SelectFileForOps(configFile.Value.fileName);

                    if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
                    {
                        Debug.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                            configFile.Value.fileName, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));
                        continue;
                    }

                    // Get Binary Status
                    (BinaryStatusObject binaryStatusObject, int VipaResponse) fileBinaryStatus = GetBinaryStatus(configFile.Value.fileName);

                    if (fileBinaryStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
                    {
                        Debug.WriteLine(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                            configFile.Value.fileName, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));

                        // Clean up pool allocation, clearing the array
                        if (fileStatus.binaryStatusObject.ReadResponseBytes != null)
                        {
                            ArrayPool<byte>.Shared.Return(fileStatus.binaryStatusObject.ReadResponseBytes, true);
                        }

                        continue;
                    }

                    // Check for size match
                    if (fileBinaryStatus.binaryStatusObject.FileSize != configFile.Value.fileSize)
                    {
                        Logger.error($"VIPA: {configFile.Value.fileName} SIZE MISMATCH! - actual={fileBinaryStatus.binaryStatusObject.FileSize}");

                        // requires CustId to process proper image version
                        //if (configFile.Value.configType != BinaryStatusObject.DeviceConfigurationTypes.IdleConfiguration)
                        //{
                        //    continue;
                        //}
                    }

                    // Check for HASH Match
                    if (!fileBinaryStatus.binaryStatusObject.FileCheckSum.Equals(configFile.Value.fileHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.error($"VIPA: {configFile.Value.fileName} HASH MISMATCH! - actual={fileBinaryStatus.binaryStatusObject.FileCheckSum}");
                        // requires CustId to process proper image version
                        //if (configFile.Value.configType != BinaryStatusObject.DeviceConfigurationTypes.IdleConfiguration)
                        //{
                        //    continue;
                        //}
                    }

                    // Read File Contents
                    fileStatus = ReadBinaryDataFromSelectedFile(0x00, (byte)fileStatus.binaryStatusObject.FileSize);

                    if (fileStatus.VipaResponse != (int)VipaSW1SW2Codes.Success)
                    {
                        Logger.error(string.Format("VIPA {0} ACCESS ERROR=0x{1:X4} - '{2}'",
                            configFile.Value.fileName, fileStatus.VipaResponse, ((VipaSW1SW2Codes)fileStatus.VipaResponse).GetStringValue()));

                        // Clean up pool allocation, clearing the array
                        if (fileStatus.binaryStatusObject.ReadResponseBytes != null)
                        {
                            ArrayPool<byte>.Shared.Return(fileStatus.binaryStatusObject.ReadResponseBytes, true);
                        }
                    }

                    //Console.WriteLine(", HASH MATCH");
                    versions.Remove(configFile.Value.fileName, out _);
                    versions.Add(configFile.Value.fileName,
                        Encoding.UTF8.GetString(fileStatus.binaryStatusObject.ReadResponseBytes).Replace("\0", string.Empty));
                }
            }

            // populate response appropriately
            foreach ((string key, string value) in versions)
            {
                _ = key switch
                {
                    BinaryStatusObject.VIPA_VER_FW => ProcessVersionString(linkActionRequestIPA5Object.DALCdbData.VIPAVersion, value),
                    BinaryStatusObject.VIPA_VER_EMV => ProcessVersionString(linkActionRequestIPA5Object.DALCdbData.EMVVersion, value),
                    BinaryStatusObject.VIPA_VER_IDLE => ProcessVersionString(linkActionRequestIPA5Object.DALCdbData.IdleVersion, value),
                    _ => throw new Exception($"Invalid key identifier '{key}'.")
                };
            }

            int bundleFoundcount = versions.Where(x => !x.Value.Equals(bundleNotFound)).Count();

            return linkActionRequestIPA5Object;
        }

        public LinkDALRequestIPA5Object VIPAVersions(string deviceModel, bool hmacEnabled, string activeCustomerId) => hmacEnabled switch
        {
            true => ReportVIPAVersions(BinaryStatusObject.verifoneVipaVersions, deviceModel, activeCustomerId),
            false => ReportVIPAVersions(BinaryStatusObject.sphereVipaVersions, deviceModel, activeCustomerId)
        };

        public (string Timestamp, int VipaResponse) Get24HourReboot()
        {
            return GetPCIRebootTime();
        }

        public (string Timestamp, int VipaResponse) Reboot24Hour(string timestamp)
        {
            Console.WriteLine($"VIPA: SET 24 HOUR REBOOT TO [{timestamp}]");
            (string Timestamp, int VipaResponse) reboot24HourInformationObject = GetPCIRebootTime();

            if (reboot24HourInformationObject.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                if (!timestamp.Equals(reboot24HourInformationObject.Timestamp))
                {
                    reboot24HourInformationObject = SetPCIRebootTime(timestamp);
                }
            }

            return reboot24HourInformationObject;
        }

        public (string Timestamp, int VipaResponse) GetTerminalDateTime()
        {
            return GetDeviceDateTime();
        }

        public (string Timestamp, int VipaResponse) SetTerminalDateTime(string timestamp)
        {
            Console.WriteLine($"VIPA: SET TERMINAL DATE-TIME TO [{timestamp}]");
            (string Timestamp, int VipaResponse) terminalDateTimeInformationObject = GetDeviceDateTime();

            if (terminalDateTimeInformationObject.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                if (!timestamp.Equals(terminalDateTimeInformationObject.Timestamp))
                {
                    int vipaResponse = SetDeviceDateTime(timestamp);

                    if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        terminalDateTimeInformationObject = GetDeviceDateTime();
                    }
                }
            }

            return terminalDateTimeInformationObject;
        }

        private (int vipaResponse, int vipaData) VerifyAmountScreen(string displayMessage)
        {
            CancelResponseHandlers();

            string[] messageFormat = displayMessage.Split(new char[] { '|' });

            if (messageFormat.Length != 4)
            {
                return ((int)VipaSW1SW2Codes.Failure, 0);
            }

            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            List<TLVImpl> customScreenData = new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(E0Template.DisplayText, Encoding.ASCII.GetBytes($"\t{messageFormat[0]}")),
                        new TLVImpl(E0Template.DisplayText, Encoding.ASCII.GetBytes($"\t{messageFormat[1]}")),
                        new TLVImpl(E0Template.DisplayText, Encoding.ASCII.GetBytes(" ")),
                        new TLVImpl(E0Template.DisplayText, Encoding.ASCII.GetBytes($"\t1. {messageFormat[2]}")),
                        new TLVImpl(E0Template.DisplayText, Encoding.ASCII.GetBytes($"\t2. {messageFormat[3]}"))
                    }
                }
            };
            byte[] customScreenDataData = TLVImpl.Encode(customScreenData);

            SendVipaCommand(VIPACommandType.DisplayText, 0x00, 0x01, customScreenDataData);

            int displayCommandResponseCode = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            (int vipaResponse, int vipaData) commandResult = (displayCommandResponseCode, 0);

            if (displayCommandResponseCode == (int)VipaSW1SW2Codes.Success)
            {
                // Setup reader to accept user input
                DeviceInteractionInformation = new TaskCompletionSource<(LinkDALRequestIPA5Object linkDALRequestIPA5Object, int VipaResponse)>();

                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += GetDeviceInteractionKeyboardResponseHandler;

                // Bit 0 - Enter, Cancel, Clear keys
                // Bit 1 - function keys
                // Bit 2 - numeric keys
                SendVipaCommand(VIPACommandType.KeyboardStatus, 0x07, 0x00);

                LinkDALRequestIPA5Object cardInfo = null;

                do
                {
                    cardInfo = DeviceInteractionInformation.Task.Result.linkDALRequestIPA5Object;
                    commandResult.vipaResponse = DeviceInteractionInformation.Task.Result.VipaResponse;

                    if (cardInfo?.DALResponseData?.Status?.Equals("UserKeyPressed") ?? false)
                    {
                        Debug.WriteLine($"KEY PRESSED: {cardInfo.DALResponseData.Value}");
                        Console.WriteLine($"KEY PRESSED: {cardInfo.DALResponseData.Value}");
                        // <O> == 1 : YES
                        // <X> == 2 : NO
                        if (cardInfo.DALResponseData.Value.Equals("KEY_1") || cardInfo.DALResponseData.Value.Equals("KEY_GREEN"))
                        {
                            commandResult.vipaData = 1;
                        }
                        else if (cardInfo.DALResponseData.Value.Equals("KEY_2") || cardInfo.DALResponseData.Value.Equals("KEY_RED"))
                        {
                            commandResult.vipaData = 0;
                        }
                        else
                        {
                            commandResult.vipaResponse = (int)VipaSW1SW2Codes.Failure;
                            DeviceInteractionInformation = new TaskCompletionSource<(LinkDALRequestIPA5Object linkDALRequestIPA5Object, int VipaResponse)>();
                        }
                    }
                } while (commandResult.vipaResponse == (int)VipaSW1SW2Codes.Failure);

                ResponseTagsHandler -= GetDeviceInteractionKeyboardResponseHandler;
                ResponseTagsHandlerSubscribed--;
            }

            return commandResult;
        }

        private (int vipaResponse, int vipaData) VerifyAmountScreenHTML(string displayMessage)
        {
            CancelResponseHandlers();

            string[] messageFormat = displayMessage.Split(new char[] { '|' });

            if (messageFormat.Length != 5)
            {
                return ((int)VipaSW1SW2Codes.Failure, 0);
            }

            // Setup keyboard reader
            if ((int)VipaSW1SW2Codes.Success != StartKeyboardReader())
            {
                return ((int)VipaSW1SW2Codes.Failure, 0);
            }

            byte[] htmlResource = Encoding.ASCII.GetBytes("mapp/verify_amount.html");
            byte[] screenTitle = Encoding.ASCII.GetBytes($"\t{messageFormat[0]}");
            byte[] item1 = Encoding.ASCII.GetBytes($"\t{messageFormat[1]}");
            byte[] item2 = Encoding.ASCII.GetBytes($"\t{messageFormat[2]}");
            byte[] item3 = Encoding.ASCII.GetBytes($"\t{messageFormat[3]}");
            byte[] totalAmount = Encoding.ASCII.GetBytes($"\t{messageFormat[4]}");

            List<TLVImpl> customScreenData = new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(E0Template.HTMLResourceName, htmlResource),
                        new TLVImpl(E0Template.HTMLKeyName, Encoding.ASCII.GetBytes("title")), new TLVImpl(E0Template.HTMLValueName, screenTitle),
                        new TLVImpl(E0Template.HTMLKeyName, Encoding.ASCII.GetBytes("item1")), new TLVImpl(E0Template.HTMLValueName, item1),
                        new TLVImpl(E0Template.HTMLKeyName, Encoding.ASCII.GetBytes("item2")), new TLVImpl(E0Template.HTMLValueName, item2),
                        new TLVImpl(E0Template.HTMLKeyName, Encoding.ASCII.GetBytes("item3")), new TLVImpl(E0Template.HTMLValueName, item3),
                        new TLVImpl(E0Template.HTMLKeyName, Encoding.ASCII.GetBytes("total")), new TLVImpl(E0Template.HTMLValueName, totalAmount),
                    }
                }
            };
            byte[] customScreenDataData = TLVImpl.Encode(customScreenData);

            SendVipaCommand(VIPACommandType.DisplayHTML, 0x00, 0x01, customScreenDataData);

            (int vipaResponse, int vipaData) commandResult = (ResponseCodeResult.Task.Result, 0);

            if (commandResult.vipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                do
                {
                    LinkDALRequestIPA5Object cardInfo = DeviceInteractionInformation.Task.Result.linkDALRequestIPA5Object;
                    commandResult.vipaResponse = DeviceInteractionInformation.Task.Result.VipaResponse;

                    if (cardInfo?.DALResponseData?.Status?.Equals("UserKeyPressed") ?? false)
                    {
                        Debug.WriteLine($"KEY PRESSED: {cardInfo.DALResponseData.Value}");
                        Console.WriteLine($"KEY PRESSED: {cardInfo.DALResponseData.Value}");
                        // <O> == 1 : YES
                        // <X> == 2 : NO
                        if (cardInfo.DALResponseData.Value.Equals("KEY_1") || cardInfo.DALResponseData.Value.Equals("KEY_GREEN"))
                        {
                            commandResult.vipaData = 1;
                        }
                        else if (cardInfo.DALResponseData.Value.Equals("KEY_2") || cardInfo.DALResponseData.Value.Equals("KEY_RED"))
                        {
                            commandResult.vipaData = 0;
                        }
                        else
                        {
                            commandResult.vipaResponse = (int)VipaSW1SW2Codes.Failure;
                            DeviceInteractionInformation = new TaskCompletionSource<(LinkDALRequestIPA5Object linkDALRequestIPA5Object, int VipaResponse)>();
                        }
                    }
                } while (commandResult.vipaResponse == (int)VipaSW1SW2Codes.Failure);
            }

            // Stop keyboard reader
            StopKeyboardReader();

            return commandResult;
        }

        private int StartKeyboardReader()
        {
            CancelResponseHandlers();

            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            // Setup reader to accept user input
            DeviceInteractionInformation = new TaskCompletionSource<(LinkDALRequestIPA5Object linkDALRequestIPA5Object, int VipaResponse)>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetDeviceInteractionKeyboardResponseHandler;

            // collect response from user
            // Bit 0 - Enter, Cancel, Clear keys
            // Bit 1 - function keys
            // Bit 2 - numeric keys
            SendVipaCommand(VIPACommandType.KeyboardStatus, 0x07, 0x00);

            return ResponseCodeResult.Task.Result;
        }

        private int StopKeyboardReader()
        {
            if (ResponseTagsHandlerSubscribed > 0)
            {
                SendVipaCommand(VIPACommandType.KeyboardStatus, 0x00, 0x00);

                int response = DeviceInteractionInformation.Task.Result.VipaResponse;

                ResponseTagsHandler -= GetDeviceInteractionKeyboardResponseHandler;
                ResponseTagsHandlerSubscribed--;

                return response;
            }

            return (int)VipaSW1SW2Codes.Failure;
        }

        private List<TLVImpl> FormatE0Tag(byte[] hmackey, byte[] generated_hmackey)
        {
            return new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x46 }, new byte[] { 0x03 }),
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x2E }, hmackey),
                        new TLVImpl(new byte[] { 0xDF, 0xED, 0x15 }, generated_hmackey)
                    }
                }
            };
        }

        private string GetCurrentKSNHMAC()
        {
            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetGeneratedHMACResponseHandler;

            var dataForHMAC = new List<TLVImpl>
            {
                new TLVImpl
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLVImpl>
                    {
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x0E },new byte[] { 0x00 }),
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x23 }, new byte[] { 0x06 }),
                        new TLVImpl(new byte[] { 0xDF, 0xEC, 0x23 }, new byte[] { 0x07 })
                    }
                }
            };

            byte[] dataForHMACData = TLVImpl.Encode(dataForHMAC);

            SendVipaCommand(VIPACommandType.GenerateHMAC, 0x00, 0x00, dataForHMACData);

            var deviceSecurityConfigurationInfo = DeviceSecurityConfiguration.Task.Result;

            ResponseTagsHandler -= GetGeneratedHMACResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceSecurityConfigurationInfo.securityConfigurationObject.GeneratedHMAC;
        }

        private int UpdateHMACKey(byte keyId, byte[] dataForHMACData)
        {
            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            SendVipaCommand(VIPACommandType.UpdateKey, keyId, 0x01, dataForHMACData);

            int vipaResponse = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return vipaResponse;
        }

        private (BinaryStatusObject binaryStatusObject, int VipaResponse) PutFile(string fileName, string targetFile)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return (null, (int)VipaSW1SW2Codes.Failure);
            }

            (BinaryStatusObject binaryStatusObject, int VipaResponse) deviceBinaryStatus = (null, (int)VipaSW1SW2Codes.Failure);

            if (File.Exists(targetFile))
            {
                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += GetBinaryStatusResponseHandler;

                FileInfo fileInfo = new FileInfo(targetFile);
                long fileLength = fileInfo.Length;
                byte[] fileSize = new byte[4];
                Array.Copy(BitConverter.GetBytes(fileLength), 0, fileSize, 0, fileSize.Length);
                Array.Reverse(fileSize);

                // File information
                var fileInformation = new List<TLVImpl>
                {
                    new TLVImpl
                    {
                        Tag = new byte[] { 0x6F },
                        InnerTags = new List<TLVImpl>
                        {
                            new TLVImpl(new byte[] { 0x84 }, Encoding.ASCII.GetBytes(fileName.ToLower())),
                            new TLVImpl(new byte[] { 0x80 }, fileSize)
                        }
                    }
                };

                byte[] fileInformationData = TLVImpl.Encode(fileInformation);

                DeviceBinaryStatusInformation = new TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)>();

                SendVipaCommand(VIPACommandType.StreamUpload, 0x05, 0x81, fileInformationData);

                // Tag 6F with size and checksum is returned on success
                deviceBinaryStatus = DeviceBinaryStatusInformation.Task.Result;

                //if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                if (deviceBinaryStatus.VipaResponse == (int)VipaSW1SW2Codes.Success)
                {
                    using (FileStream fs = File.OpenRead(targetFile))
                    {
                        int numBytesToRead = (int)fs.Length;

                        while (numBytesToRead > 0)
                        {
                            byte[] readBytes = new byte[PACKET_SIZE];
                            int bytesRead = fs.Read(readBytes, 0, PACKET_SIZE);
                            WriteRawBytes(readBytes);
                            numBytesToRead -= bytesRead;
                        }
                    }

                    // wait for device reponse
                    DeviceBinaryStatusInformation = new TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)>();
                    deviceBinaryStatus = DeviceBinaryStatusInformation.Task.Result;
                }

                ResponseTagsHandler -= GetBinaryStatusResponseHandler;
                ResponseTagsHandlerSubscribed--;
            }

            return deviceBinaryStatus;
        }

        private (BinaryStatusObject binaryStatusObject, int VipaResponse) GetBinaryStatus(string fileName)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetBinaryStatusResponseHandler;

            DeviceBinaryStatusInformation = new TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)>();

            var data = Encoding.ASCII.GetBytes(fileName);
            byte reportMD5 = 0x80;
            SendVipaCommand(VIPACommandType.GetBinaryStatus, 0x00, reportMD5, Encoding.ASCII.GetBytes(fileName));

            var deviceBinaryStatus = DeviceBinaryStatusInformation.Task.Result;

            ResponseTagsHandler -= GetBinaryStatusResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceBinaryStatus;
        }

        private (BinaryStatusObject binaryStatusObject, int VipaResponse) SelectFileForOps(string fileName)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetBinaryStatusResponseHandler;

            // When the file cannot be accessed, VIPA returns SW1SW2 equal to 9F13
            DeviceBinaryStatusInformation = new TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)>();

            var data = Encoding.ASCII.GetBytes(fileName);

            // P1 Bit 2:  1 - Selection by DF name
            SendVipaCommand(VIPACommandType.SelectFile, 0x04, 0x00, Encoding.ASCII.GetBytes(fileName));

            var deviceBinaryStatus = DeviceBinaryStatusInformation.Task.Result;

            ResponseTagsHandler -= GetBinaryStatusResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceBinaryStatus;
        }

        private (BinaryStatusObject binaryStatusObject, int VipaResponse) ReadBinaryDataFromSelectedFile(byte readOffset, byte bytesToRead)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTaglessHandler += GetBinaryDataResponseHandler;

            // When the file cannot be accessed, VIPA returns SW1SW2 equal to 9F13
            DeviceBinaryStatusInformation = new TaskCompletionSource<(BinaryStatusObject binaryStatusObject, int VipaResponse)>();

            // P1 bit 8 = 0: P1 and P2 are the offset at which to read the data from (15-bit addressing)
            // P1 bit 8 = 1: data size 2 bytes, first byte is low-order offset byte, 2nd byte is number of bytes to read
            // DATA - If P1 bit 8 = 0, data size 1 byte, contains the number of bytes to read
            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0x00, ins = 0xB0, p1 = 0x00, p2 = readOffset };
            command.includeLE = true;
            command.le = bytesToRead;
            WriteSingleCmd(command);

            var deviceBinaryStatus = DeviceBinaryStatusInformation.Task.Result;

            ResponseTaglessHandler -= GetBinaryDataResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceBinaryStatus;
        }

        private (string Timestamp, int VipaResponse) GetPCIRebootTime()
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += Get24HourRebootResponseHandler;

            Reboot24HourInformation = new TaskCompletionSource<(string Timestamp, int VipaResponse)>();

            SendVipaCommand(VIPACommandType.Terminal24HourReboot, 0x00, 0x00);

            var device24HourStatus = Reboot24HourInformation.Task.Result;

            ResponseTagsHandler -= Get24HourRebootResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return device24HourStatus;
        }

        private (string Timestamp, int VipaResponse) SetPCIRebootTime(string timestamp)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += Get24HourRebootResponseHandler;

            Reboot24HourInformation = new TaskCompletionSource<(string Timestamp, int VipaResponse)>();

            byte[] timestampForRebootData = Encoding.ASCII.GetBytes(timestamp);

            SendVipaCommand(VIPACommandType.Terminal24HourReboot, 0x00, 0x00, timestampForRebootData);

            var device24HourStatus = Reboot24HourInformation.Task.Result;

            ResponseTagsHandler -= Get24HourRebootResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return device24HourStatus;
        }

        private (string Timestamp, int VipaResponse) GetDeviceDateTime()
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTaglessHandler += GetTerminalDateTimeResponseHandler;

            TerminalDateTimeInformation = new TaskCompletionSource<(string Timestamp, int VipaResponse)>();

            SendVipaCommand(VIPACommandType.TerminalSetGetDataTime, 0x01, 0x00);

            (string Timestamp, int VipaResponse) deviceDateTimeStatus = TerminalDateTimeInformation.Task.Result;

            ResponseTaglessHandler -= GetTerminalDateTimeResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceDateTimeStatus;
        }

        private int SetDeviceDateTime(string timestamp)
        {
            CancelResponseHandlers();

            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            byte[] timestampData = Encoding.ASCII.GetBytes(timestamp);

            SendVipaCommand(VIPACommandType.TerminalSetGetDataTime, 0x00, 0x00, timestampData);

            int deviceCommandResponseCode = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceCommandResponseCode;
        }

        private int ProcessVersionString(DALBundleVersioning bundle, string value)
        {
            if (!value.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            {
                string[] elements = value.Split(new char[] { '.' });

                if (elements.Length != 9)
                {
                    return (int)VipaSW1SW2Codes.Failure;
                }

                bundle.Signature = elements[(int)VerifoneSchemaIndex.Sig];
                bundle.Application = elements[((int)VerifoneSchemaIndex.App)];
                bundle.Type = elements[(int)VerifoneSchemaIndex.Type];
                bundle.TerminalType = elements[(int)VerifoneSchemaIndex.TerminalType];
                bundle.FrontEnd = elements[(int)VerifoneSchemaIndex.FrontEnd];
                bundle.Entity = elements[(int)VerifoneSchemaIndex.Entity];
                bundle.Model = elements[(int)VerifoneSchemaIndex.Model];
                bundle.Version = elements[(int)VerifoneSchemaIndex.Version];
                bundle.DateCode = elements[(int)VerifoneSchemaIndex.DateCode];
            }
            return (int)VipaSW1SW2Codes.Success;
        }

        #endregion --- VIPA commands ---

        #region --- response handlers ---

        public void CancelResponseHandlers(int retries = 1)
        {
            int count = 0;

            while (ResponseTagsHandlerSubscribed != 0 && count++ <= retries)
            {
                ResponseTagsHandler?.Invoke(null, (int)VipaSW1SW2Codes.Success, true);
                Thread.Sleep(1);
            }
            //count = 0;
            //while (ResponseTaglessHandlerSubscribed != 0 && count++ <= retries)
            //{
            //    ResponseTaglessHandler?.Invoke(null, -1, true);
            //    Thread.Sleep(1);
            //}
            //count = 0;
            //while (ResponseContactlessHandlerSubscribed != 0 && count++ <= retries)
            //{
            //    ResponseCLessHandler?.Invoke(null, -1, 0, true);
            //    Thread.Sleep(1);
            //}

            ResponseTagsHandlerSubscribed = 0;
            ResponseTagsHandler = null;
            //ResponseTaglessHandlerSubscribed = 0;
            ResponseTaglessHandler = null;
            //ResponseContactlessHandlerSubscribed = 0;
            ResponseCLessHandler = null;
        }

        public void ResponseCodeHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            ResponseCodeResult?.TrySetResult(cancelled ? -1 : responseCode);
        }

        public void DeviceResetResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled || tags == null)
            {
                DeviceResetConfiguration?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new DevicePTID();

            if (tags.FirstOrDefault().Tag.SequenceEqual(E0Template.PtidTag))
            {
                deviceResponse.PTID = BitConverter.ToString(tags.FirstOrDefault().Data).Replace("-", "");
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                if (tags.Count == 1)
                {
                    DeviceResetConfiguration?.TrySetResult((deviceResponse, responseCode));
                }
            }
            else
            {
                DeviceResetConfiguration?.TrySetResult((null, responseCode));
            }
        }

        private void GetDeviceInfoResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled)
            {
                DeviceIdentifier?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new LinkDeviceResponse
            {
                // TODO: rework to be values reflecting actual device capabilities
                /*CardWorkflowControls = new XO.Common.DAL.LinkCardWorkflowControls
                {
                    CardCaptureTimeout = 90,
                    ManualCardTimeout = 5,
                    DebitEnabled = false,
                    EMVEnabled = false,
                    ContactlessEnabled = false,
                    ContactlessEMVEnabled = false,
                    CVVEnabled = false,
                    VerifyAmountEnabled = false,
                    AVSEnabled = false,
                    SignatureEnabled = false
                }*/
            };

            LinkDALRequestIPA5Object cardInfo = new LinkDALRequestIPA5Object();

            foreach (var tag in tags)
            {
                if (tag.Tag.SequenceEqual(EETemplate.EETemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(EETemplate.TerminalNameTag) && string.IsNullOrEmpty(deviceResponse.Model))
                        {
                            deviceResponse.Model = Encoding.UTF8.GetString(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(EETemplate.SerialNumberTag) && string.IsNullOrWhiteSpace(deviceResponse.SerialNumber))
                        {
                            deviceResponse.SerialNumber = Encoding.UTF8.GetString(dataTag.Data);
                            //deviceInformation.SerialNumber = deviceResponse.SerialNumber ?? string.Empty;
                        }
                        else if (dataTag.Tag.SequenceEqual(EETemplate.TamperStatus))
                        {
                            //DF8101 = 00 no tamper detected
                            //DF8101 = 01 tamper detected
                            //cardInfo.TamperStatus = Encoding.UTF8.GetString(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(EETemplate.ArsStatus))
                        {
                            //DF8102 = 00 ARS not active
                            //DF8102 = 01 ARS active
                            //cardInfo.ArsStatus = Encoding.UTF8.GetString(dataTag.Data);
                        }
                    }
                }
                else if (tag.Tag.SequenceEqual(EETemplate.TerminalIdTag))
                {
                    //deviceResponse.TerminalId = Encoding.UTF8.GetString(tag.Data);
                }
                else if (tag.Tag.SequenceEqual(EFTemplate.EFTemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(EFTemplate.WhiteListHash))
                        {
                            //cardInfo.WhiteListHash = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(EFTemplate.FirmwareVersion) && string.IsNullOrWhiteSpace(deviceResponse.FirmwareVersion))
                        {
                            deviceResponse.FirmwareVersion = Encoding.UTF8.GetString(dataTag.Data);
                        }
                    }
                }
                else if (tag.Tag.SequenceEqual(E6Template.E6TemplateTag))
                {
                    deviceResponse.PowerOnNotification = new LinkDevicePowerOnNotification();

                    var _tags = TLVImpl.Decode(tag.Data, 0, tag.Data.Length);

                    foreach (var dataTag in _tags)
                    {
                        if (dataTag.Tag.SequenceEqual(E6Template.TransactionStatusTag))
                        {
                            deviceResponse.PowerOnNotification.TransactionStatus = BCDConversion.BCDToInt(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(E6Template.TransactionStatusMessageTag))
                        {
                            deviceResponse.PowerOnNotification.TransactionStatusMessage = Encoding.UTF8.GetString(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(EETemplate.TerminalIdTag))
                        {
                            deviceResponse.PowerOnNotification.TerminalID = Encoding.UTF8.GetString(dataTag.Data);
                        }
                    }
                }
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                if (tags?.Count > 0)
                {
                    DeviceInfoObject deviceInfoObject = new DeviceInfoObject
                    {
                        LinkDeviceResponse = deviceResponse,
                        LinkDALRequestIPA5Object = cardInfo
                    };
                    DeviceIdentifier?.TrySetResult((deviceInfoObject, responseCode));
                }
                //else
                //{
                //    deviceIdentifier?.TrySetResult((null, responseCode));
                //}
            }
        }

        public void GetSecurityInformationResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled || tags == null)
            {
                DeviceSecurityConfiguration?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new SecurityConfigurationObject();

            foreach (var tag in tags)
            {
                if (tag.Tag.SequenceEqual(E0Template.E0TemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(E0Template.OnlinePINKSNTag))
                        {
                            if (DeviceInformation.ConfigurationHostId == VerifoneSettingsSecurityConfiguration.ConfigurationHostId)
                            {
                                string ksn = ConversionHelper.ByteArrayCodedHextoString(dataTag.Data);
                                deviceResponse.OnlinePinKSN = ksn.PadLeft(20, 'F');
                            }
                            else
                            {
                                deviceResponse.OnlinePinKSN = BitConverter.ToString(dataTag.Data).Replace("-", "");
                            }
                        }
                        if (dataTag.Tag.SequenceEqual(E0Template.KeySlotNumberTag))
                        {
                            deviceResponse.KeySlotNumber = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(E0Template.SRedCardKSNTag))
                        {
                            deviceResponse.SRedCardKSN = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(E0Template.InitVectorTag))
                        {
                            deviceResponse.InitVector = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(E0Template.EncryptedKeyCheckTag))
                        {
                            deviceResponse.EncryptedKeyCheck = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                    }
                }
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                if (tags.Count > 0)
                {
                    DeviceSecurityConfiguration?.TrySetResult((deviceResponse, responseCode));
                }
            }
            else
            {
                DeviceSecurityConfiguration?.TrySetResult((null, responseCode));
            }
        }

        public void GetKernelInformationResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled || tags == null)
            {
                DeviceKernelConfiguration?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new KernelConfigurationObject();

            foreach (var tag in tags)
            {
                // note: we just need the first instance
                if (tag.Tag.SequenceEqual(E0Template.E0TemplateTag))
                {
                    var kernelApplicationTag = tag.InnerTags.Where(x => x.Tag.SequenceEqual(E0Template.ApplicationAIDTag)).FirstOrDefault();
                    deviceResponse.ApplicationIdentifierTerminal = BitConverter.ToString(kernelApplicationTag.Data).Replace("-", "");
                    var kernelChecksumTag = tag.InnerTags.Where(x => x.Tag.SequenceEqual(E0Template.KernelConfigurationTag)).FirstOrDefault();
                    deviceResponse.ApplicationKernelInformation = ConversionHelper.ByteArrayToAsciiString(kernelChecksumTag.Data).Replace("\0", string.Empty);
                    break;
                }
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                if (tags.Count > 0)
                {
                    DeviceKernelConfiguration?.TrySetResult((deviceResponse, responseCode));
                }
            }
            else
            {
                DeviceKernelConfiguration?.TrySetResult((null, responseCode));
            }
        }

        public void GetGeneratedHMACResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            var MACTag = new byte[] { 0xDF, 0xEC, 0x7B };

            if (cancelled || tags == null)
            {
                DeviceSecurityConfiguration?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new SecurityConfigurationObject();

            if (tags.FirstOrDefault().Tag.SequenceEqual(MACTag))
            {
                deviceResponse.GeneratedHMAC = BitConverter.ToString(tags.FirstOrDefault().Data).Replace("-", "");
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                if (tags.Count == 1)
                {
                    DeviceSecurityConfiguration?.TrySetResult((deviceResponse, responseCode));
                }
            }
            else
            {
                DeviceSecurityConfiguration?.TrySetResult((null, responseCode));
            }
        }

        public void GetBinaryStatusResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled || tags == null)
            {
                DeviceBinaryStatusInformation?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new BinaryStatusObject();

            foreach (var tag in tags)
            {
                if (tag.Tag.SequenceEqual(_6FTemplate._6fTemplateTag))
                {
                    var _tags = TLVImpl.Decode(tag.Data, 0, tag.Data.Length);

                    foreach (var dataTag in _tags)
                    {
                        if (dataTag.Tag.SequenceEqual(_6FTemplate.FileSizeTag))
                        {
                            deviceResponse.FileSize = BCDConversion.BCDToInt(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(_6FTemplate.FileCheckSumTag))
                        {
                            deviceResponse.FileCheckSum = BitConverter.ToString(dataTag.Data, 0).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(_6FTemplate.SecurityStatusTag))
                        {
                            deviceResponse.SecurityStatus = BCDConversion.BCDToInt(dataTag.Data);
                        }
                    }

                    break;
                }
            }

            if (responseCode == (int)VipaSW1SW2Codes.Success)
            {
                // command could return just a response without tags
                DeviceBinaryStatusInformation?.TrySetResult((deviceResponse, responseCode));
            }
            else
            {
                deviceResponse.FileNotFound = true;
                DeviceBinaryStatusInformation?.TrySetResult((deviceResponse, responseCode));
            }
        }

        public void GetBinaryDataResponseHandler(byte[] data, int responseCode, bool cancelled = false)
        {
            if (cancelled)
            {
                DeviceBinaryStatusInformation?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new BinaryStatusObject();

            if (responseCode == (int)VipaSW1SW2Codes.Success && data?.Length > 0)
            {
                deviceResponse.ReadResponseBytes = ArrayPool<byte>.Shared.Rent(data.Length);
                Array.Copy(data, 0, deviceResponse.ReadResponseBytes, 0, data.Length);
            }
            else
            {
                deviceResponse.FileNotFound = true;
            }

            DeviceBinaryStatusInformation?.TrySetResult((deviceResponse, responseCode));
        }

        public void GetDeviceInteractionKeyboardResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            bool returnResponse = false;

            if ((cancelled || tags == null) && (responseCode != (int)VipaSW1SW2Codes.CommandCancelled) &&
                (responseCode != (int)VipaSW1SW2Codes.UserEntryCancelled))
            {
                DeviceInteractionInformation?.TrySetResult((new LinkDALRequestIPA5Object(), responseCode));
                return;
            }

            var cardResponse = new LinkDALRequestIPA5Object();

            foreach (TLVImpl tag in tags)
            {
                if (tag.Tag.SequenceEqual(E0Template.E0TemplateTag))
                {
                    foreach (TLVImpl dataTag in tag.InnerTags)
                    {
                        //if (dataTag.Tag.SequenceEqual(E0Template.CardStatus) && ((dataTag.Data?[0] & 0x01) == 0x01))
                        //{
                        //    cardResponse.DALResponseData = new LinkDALActionResponse
                        //    {
                        //        Status = "CardPresented",
                        //        CardPresented = true
                        //    };

                        //    returnResponse = true;
                        //}
                        //else 
                        if (dataTag.Tag.SequenceEqual(E0Template.KeyPress))
                        {
                            cardResponse.DALResponseData = new LinkDALActionResponse
                            {
                                Status = "UserKeyPressed",
                                Value = BCDConversion.StringFromByteData(dataTag.Data)
                            };
                            returnResponse = true;
                            break;
                        }
                    }

                    break;
                }
                else if (tag.Tag.SequenceEqual(TLVImpl.SplitUIntToByteArray(E0Template.HTMLKeyPress)))
                {
                    cardResponse.DALResponseData = new LinkDALActionResponse
                    {
                        Status = "UserKeyPressed",
                        Value = tag.Data[3] switch
                        {
                            // button actions as reported from HTML page
                            0x00 => "KEY_2",
                            0x1B => "KEY_RED",
                            0x01 => "KEY_1",
                            0x0D => "KEY_GREEEN",
                            _ => "KEY_NONE"
                        }
                    };
                    returnResponse = true;
                    break;
                }
            }

            if (returnResponse)
            {
                DeviceInteractionInformation?.TrySetResult((cardResponse, responseCode));
            }
        }

        public void Get24HourRebootResponseHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false)
        {
            if (cancelled || tags == null)
            {
                Reboot24HourInformation?.TrySetResult((null, responseCode));
                return;
            }

            string deviceResponse = string.Empty;

            foreach (var tag in tags)
            {
                if (tag.Tag.SequenceEqual(E0Template.Reboot24HourTag))
                {
                    deviceResponse = Encoding.UTF8.GetString(tag.Data);
                    break;
                }
            }

            // command must always be processed
            Reboot24HourInformation?.TrySetResult((deviceResponse, responseCode));
        }

        public void GetTerminalDateTimeResponseHandler(byte[] data, int responseCode, bool cancelled = false)
        {
            if (cancelled)
            {
                TerminalDateTimeInformation?.TrySetResult((null, responseCode));
                return;
            }

            string deviceResponse = string.Empty;

            if (responseCode == (int)VipaSW1SW2Codes.Success && data?.Length > 0)
            {
                deviceResponse = Encoding.UTF8.GetString(data);
            }

            // command must always be processed
            TerminalDateTimeInformation?.TrySetResult((deviceResponse, responseCode));
        }

        #endregion --- response handlers ---

        public void LoadDeviceSectionConfig(DeviceSection config)
        {
            //preSwipeCardStorage.SetConfig(config?.Verifone?.PreSwipeTimeout ?? 0);
            DeviceInformation.ConfigurationHostId = config?.Verifone?.ConfigurationHostId ?? VerifoneSettingsSecurityConfiguration.ConfigurationHostId;
            DeviceInformation.OnlinePinKeySetId = config?.Verifone?.OnlinePinKeySetId ?? VerifoneSettingsSecurityConfiguration.OnlinePinKeySetId;
            DeviceInformation.ADEKeySetId = config?.Verifone?.ADEKeySetId ?? VerifoneSettingsSecurityConfiguration.ADEKeySetId;
        }
    }
}
