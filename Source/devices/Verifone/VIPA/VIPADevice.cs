using Devices.Common.Helpers;
using Devices.Verifone.Connection;
using Devices.Verifone.Helpers;
using Devices.Verifone.TLV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XO.Private;
using XO.Responses;
using static Devices.Verifone.Helpers.Messages;

namespace Devices.Verifone.VIPA
{
    public class VIPADevice : IVIPADevice, IDisposable
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
        #endregion --- enumerations ---

        #region --- attributes ---
        private enum ResetDeviceCfg
        {
            ReturnSerialNumber = 1 << 0,
            ReturnAfterCardRemoval = 1 << 1,
            LeaveScreenDisplayUnchanged = 1 << 2,
            SlideShowStartsNormalTiming = 1 << 3,
            NoBeepDuringReset = 1 << 4,
            ResetImmediately = 1 << 5,
            ReturnPinpadConfiguration = 1 << 6,
            AddVOSComponentsInformation = 1 << 7
        }

        private int ResponseTagsHandlerSubscribed = 0;

        public TaskCompletionSource<int> ResponseCodeResult = null;

        public delegate void ResponseTagsHandlerDelegate(List<TLV.TLV> tags, int responseCode, bool cancelled = false);
        internal ResponseTagsHandlerDelegate ResponseTagsHandler = null;

        public delegate void ResponseTaglessHandlerDelegate(byte[] data, int responseCode, bool cancelled = false);
        internal ResponseTaglessHandlerDelegate ResponseTaglessHandler = null;

        public delegate void ResponseCLessHandlerDelegate(List<TLV.TLV> tags, int responseCode, int pcb, bool cancelled = false);
        internal ResponseCLessHandlerDelegate ResponseCLessHandler = null;

        public TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)> DeviceIdentifier = null;
        public TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)> DeviceSecurityConfiguration = null;

        public TaskCompletionSource<(string HMAC, int VipaResponse)> DeviceGenerateHMAC = null;

        #endregion --- attributes ---

        #region --- connection ---
        private SerialConnection serialConnection { get; set; }

        public bool Connect(string comPort, SerialConnection connection)
        {
            serialConnection = connection;
            return serialConnection.Connect(comPort);
        }

        public void Dispose()
        {
            serialConnection?.Dispose();
        }

        #endregion --- connection ---

        private void WriteSingleCmd(VIPACommand command)
        {
            serialConnection?.WriteSingleCmd(new VIPAResponseHandlers
            {
                responsetagshandler = ResponseTagsHandler,
                responsetaglesshandler = ResponseTaglessHandler,
                responsecontactlesshandler = ResponseCLessHandler
            }, command);
        }

        #region --- VIPA commands ---
        public bool DisplayMessage(VIPADisplayMessageValue displayMessageValue = VIPADisplayMessageValue.Idle, bool enableBacklight = false, string customMessage = "")
        {
            ResponseCodeResult = new TaskCompletionSource<int>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += ResponseCodeHandler;

            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xD2, ins = 0x01, p1 = (byte)displayMessageValue, p2 = (byte)(enableBacklight ? 0x01 : 0x00), data = Encoding.ASCII.GetBytes(customMessage) };
            WriteSingleCmd(command);   // Display [D2, 01]

            var displayCommandResponseCode = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return displayCommandResponseCode == (int)VipaSW1SW2Codes.Success;
        }

        internal (int VipaData, int VipaResponse) DeviceCommandAbort()
        {
            (int VipaData, int VipaResponse) deviceResponse = (-1, (int)VipaSW1SW2Codes.Failure);

            ResponseCodeResult = new TaskCompletionSource<int>();

            try
            {
                DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);
                ResponseTagsHandlerSubscribed++;
                ResponseTagsHandler += ResponseCodeHandler;

                Debug.WriteLine(ConsoleMessages.AbortCommand.GetStringValue());
                VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xD0, ins = 0xFF, p1 = 0x00, p2 = 0x00 };
                WriteSingleCmd(command);

                deviceResponse = ((int)VipaSW1SW2Codes.Success, ResponseCodeResult.Task.Result);

                ResponseTagsHandler -= ResponseCodeHandler;
                ResponseTagsHandlerSubscribed--;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("\r\n=========================== RESETDEVICE ERROR ===========================");
                Console.WriteLine($"{DateTime.Now.ToString("yyyyMMdd:HHmmss")}: {e.Message}");
                Console.WriteLine("===============================================================================\r\n");
            }
            catch (OperationCanceledException op)
            {
                Console.WriteLine("{0}: (1) DeviceManager::ResetDevice - EXCEPTION=[{1}]", DateTime.Now.ToString("yyyyMMdd:HHmmss"), op.Message);
            }

            return deviceResponse;
        }

        public (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceCommandReset()
        {
            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceResponse = (null, (int)VipaSW1SW2Codes.Failure);

            DeviceIdentifier = new TaskCompletionSource<(DeviceInfoObject deviceInfoObject, int VipaResponse)>(TaskCreationOptions.RunContinuationsAsynchronously);

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetDeviceInfoResponseHandler;

            System.Diagnostics.Debug.WriteLine(ConsoleMessages.DeviceReset.GetStringValue());
            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xD0, ins = 0xFF, p1 = 0x00, p2 = 0x00 };
            WriteSingleCmd(command);

            command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xD0, ins = 0x00, p1 = 0x00, p2 = (byte)(ResetDeviceCfg.ReturnSerialNumber | ResetDeviceCfg.ReturnAfterCardRemoval | ResetDeviceCfg.ReturnPinpadConfiguration) };
            WriteSingleCmd(command);   // Device Info [D0, 00]

            deviceResponse = DeviceIdentifier.Task.Result;

            ResponseTagsHandler -= GetDeviceInfoResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceResponse;
        }

        public (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetSecurityConfiguration(byte vssSlot)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetSecurityInformationResponseHandler;

            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            System.Diagnostics.Debug.WriteLine(ConsoleMessages.GetDeviceHealth.GetStringValue());
            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xC4, ins = 0x11, p1 = vssSlot, p2 = 0x00 };
            WriteSingleCmd(command);

            var deviceSecurityConfigurationInfo = DeviceSecurityConfiguration.Task.Result;

            ResponseTagsHandler -= GetSecurityInformationResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceSecurityConfigurationInfo;
        }

        public (string HMAC, int VipaResponse) GenerateHMAC()
        {
            CancelResponseHandlers();

            (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) securityConfig = (new SecurityConfigurationObject(), 0);

            securityConfig = GetGeneratedHMAC(securityConfig.securityConfigurationObject.PrimarySlot,
                            HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryPANSalt), HMACValidator.MACSecondaryHASH));

            if (securityConfig.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                if (securityConfig.securityConfigurationObject.GeneratedHMAC.Equals(HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryHASH),
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    securityConfig = GetGeneratedHMAC(securityConfig.securityConfigurationObject.SecondarySlot, securityConfig.securityConfigurationObject.GeneratedHMAC);
                    if (securityConfig.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        if (securityConfig.securityConfigurationObject.GeneratedHMAC.Equals(HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACSecondaryHASHSalt), HMACValidator.MACPrimaryHASH),
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
            }

            return (securityConfig.securityConfigurationObject.GeneratedHMAC, securityConfig.VipaResponse);
        }

        private (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetGeneratedHMAC(int hostID, string MAC)
        {
            CancelResponseHandlers();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetGeneratedHMACResponseHandler;

            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            var dataForHMAC = new List<TLV.TLV>
            {
                new TLV.TLV
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLV.TLV>
                    {
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x0E },
                            Data = ConversionHelper.HexToByteArray(MAC)
                        },
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x23 },
                            Data = new byte[] { Convert.ToByte(hostID) }
                        }
                    }
                }
            };
            TLV.TLV tlv = new TLV.TLV();
            var dataForHMACData = tlv.Encode(dataForHMAC);

            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xC4, ins = 0x22, p1 = 0x00, p2 = 0x00, data = dataForHMACData };
            WriteSingleCmd(command);

            var deviceSecurityConfigurationInfo = DeviceSecurityConfiguration.Task.Result;

            ResponseTagsHandler -= GetGeneratedHMACResponseHandler;
            ResponseTagsHandlerSubscribed--;

            return deviceSecurityConfigurationInfo;
        }

        public int LoadHMACKeys()
        {
            string generatedHMAC = GetCurrentKSNHMAC();

            // KEY 06 Generation
            byte[] hmac_generated_key = ConversionHelper.HexToByteArray(generatedHMAC);

            // Signature = HMAC_old(old XOR new) - array1 is smaller or equal in size as array2
            byte[] hmac_signature_06 = ConversionHelper.XORArrays(hmac_generated_key, HMACValidator.HMACKEY06);

            var dataKey06HMAC = FormatE0Tag(hmac_signature_06);
            TLV.TLV tlv = new TLV.TLV();
            byte[] dataForHMACData = tlv.Encode(dataKey06HMAC);

            // key slot 06
            int vipaResponse = UpdateHMACKey(0x06, dataForHMACData);

            if (vipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                // KEY 07 Generation
                byte[] hmac_signature_07 = ConversionHelper.XORArrays(hmac_generated_key, HMACValidator.HMACKEY07);

                var dataKey07HMAC = FormatE0Tag(hmac_signature_07);
                tlv = new TLV.TLV();
                dataForHMACData = tlv.Encode(dataKey07HMAC);

                // key slot 07
                vipaResponse = UpdateHMACKey(0x07, dataForHMACData);
            }

            return vipaResponse;
        }

        private List<TLV.TLV> FormatE0Tag(byte[] hmackey)
        {
            return new List<TLV.TLV>
            {
                new TLV.TLV
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLV.TLV>
                    {
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x46 },
                            Data = new byte[] { 0x03 }
                        },
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x2E },
                            Data = hmackey
                        },
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xED, 0x15 },
                            Data = hmackey
                        }
                    }
                }
            };
        }

        private string GetCurrentKSNHMAC()
        {
            DeviceSecurityConfiguration = new TaskCompletionSource<(SecurityConfigurationObject securityConfigurationObject, int VipaResponse)>();

            ResponseTagsHandlerSubscribed++;
            ResponseTagsHandler += GetGeneratedHMACResponseHandler;

            var dataForHMAC = new List<TLV.TLV>
            {
                new TLV.TLV
                {
                    Tag = new byte[] { 0xE0 },
                    InnerTags = new List<TLV.TLV>
                    {
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x0E },
                            Data = new byte[] { 0x00 }
                        },
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x23 },
                            Data = new byte[] { 0x06 }
                        },
                        new TLV.TLV
                        {
                            Tag = new byte[] { 0xDF, 0xEC, 0x23 },
                            Data = new byte[] { 0x07 }
                        }
                    }
                }
            };
            TLV.TLV tlv = new TLV.TLV();
            byte[] dataForHMACData = tlv.Encode(dataForHMAC);

            Debug.WriteLine(ConsoleMessages.LoadHMACKeys.GetStringValue());
            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xC4, ins = 0x22, p1 = 0x00, p2 = 0x00, data = dataForHMACData };
            WriteSingleCmd(command);

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

            Debug.WriteLine(ConsoleMessages.LoadHMACKeys.GetStringValue());
            VIPACommand command = new VIPACommand { nad = 0x01, pcb = 0x00, cla = 0xC4, ins = 0x0A, p1 = keyId, p2 = 0x01, data = dataForHMACData };
            WriteSingleCmd(command);

            int vipaResponse = ResponseCodeResult.Task.Result;

            ResponseTagsHandler -= ResponseCodeHandler;
            ResponseTagsHandlerSubscribed--;

            return vipaResponse;
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

        public void ResponseCodeHandler(List<TLV.TLV> tags, int responseCode, bool cancelled = false)
        {
            ResponseCodeResult?.TrySetResult(cancelled ? -1 : responseCode);
        }

        private void GetDeviceInfoResponseHandler(List<TLV.TLV> tags, int responseCode, bool cancelled = false)
        {
            var eeTemplateTag = new byte[] { 0xEE };                // EE Template tag
            var terminalNameTag = new byte[] { 0xDF, 0x0D };        // Terminal Name tag
            var terminalIdTag = new byte[] { 0x9F, 0x1C };          // Terminal ID tag
            var serialNumberTag = new byte[] { 0x9F, 0x1E };        // Serial Number tag
            var tamperStatus = new byte[] { 0xDF, 0x81, 0x01 };     // Tamper Status tag
            var arsStatus = new byte[] { 0xDF, 0x81, 0x02 };        // ARS Status tag

            var efTemplateTag = new byte[] { 0xEF };                // EF Template tag
            var whiteListHash = new byte[] { 0xDF, 0xDB, 0x09 };    // Whitelist tag

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
                if (tag.Tag.SequenceEqual(eeTemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(terminalNameTag))
                        {
                            deviceResponse.Model = Encoding.UTF8.GetString(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(serialNumberTag))
                        {
                            deviceResponse.SerialNumber = Encoding.UTF8.GetString(dataTag.Data);
                            //deviceInformation.SerialNumber = deviceResponse.SerialNumber ?? string.Empty;
                        }
                        else if (dataTag.Tag.SequenceEqual(tamperStatus))
                        {
                            //DF8101 = 00 no tamper detected
                            //DF8101 = 01 tamper detected
                            //cardInfo.TamperStatus = Encoding.UTF8.GetString(dataTag.Data);
                        }
                        else if (dataTag.Tag.SequenceEqual(arsStatus))
                        {
                            //DF8102 = 00 ARS not active
                            //DF8102 = 01 ARS active
                            //cardInfo.ArsStatus = Encoding.UTF8.GetString(dataTag.Data);
                        }
                    }

                    break;
                }
                else if (tag.Tag.SequenceEqual(terminalIdTag))
                {
                    //deviceResponse.TerminalId = Encoding.UTF8.GetString(tag.Data);
                }
                else if (tag.Tag.SequenceEqual(efTemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(whiteListHash))
                        {
                            //cardInfo.WhiteListHash = BitConverter.ToString(dataTag.Data).Replace("-", "");
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
                        linkDeviceResponse = deviceResponse,
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

        public void GetSecurityInformationResponseHandler(List<TLV.TLV> tags, int responseCode, bool cancelled = false)
        {
            var E0TemplateTag = new byte[] { 0xe0 };                    // E0 Template tag
            var onlinePINKSNTag = new byte[] { 0xDF, 0xED, 0x03 };
            var initVectorTag = new byte[] { 0xDF, 0xDF, 0x12 };
            var sRedCardKSNTag = new byte[] { 0xDF, 0xDF, 0x11 };
            var encryptedKeyCheckTag = new byte[] { 0xDF, 0xDF, 0x10 };
            var keySlotNumberTag = new byte[] { 0xDF, 0xEC, 0x2E };

            if (cancelled || tags == null)
            {
                DeviceSecurityConfiguration?.TrySetResult((null, responseCode));
                return;
            }

            var deviceResponse = new SecurityConfigurationObject();

            foreach (var tag in tags)
            {
                if (tag.Tag.SequenceEqual(E0TemplateTag))
                {
                    foreach (var dataTag in tag.InnerTags)
                    {
                        if (dataTag.Tag.SequenceEqual(onlinePINKSNTag))
                        {
                            deviceResponse.OnlinePinKSN = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        if (dataTag.Tag.SequenceEqual(keySlotNumberTag))
                        {
                            deviceResponse.KeySlotNumber = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(sRedCardKSNTag))
                        {
                            deviceResponse.SRedCardKSN = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(initVectorTag))
                        {
                            deviceResponse.InitVector = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                        else if (dataTag.Tag.SequenceEqual(encryptedKeyCheckTag))
                        {
                            deviceResponse.EncryptedKeyCheck = BitConverter.ToString(dataTag.Data).Replace("-", "");
                        }
                    }

                    break;
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

        public void GetGeneratedHMACResponseHandler(List<TLV.TLV> tags, int responseCode, bool cancelled = false)
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

        #endregion --- response handlers ---
    }
}
