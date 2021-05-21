using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Config;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Devices.Verifone.Connection;
using Devices.Verifone.Helpers;
using Devices.Verifone.VIPA;
using Ninject;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using XO.Private;
using XO.Requests;
using XO.Responses;

namespace Devices.Verifone
{
    [Export(typeof(ICardDevice))]
    [Export("Verifone-M400", typeof(ICardDevice))]
    [Export("Verifone-P200", typeof(ICardDevice))]
    [Export("Verifone-P400", typeof(ICardDevice))]
    [Export("Verifone-UX300", typeof(ICardDevice))]
    internal class VerifoneDevice : IDisposable, ICardDevice
    {
        public string Name => StringValueAttribute.GetStringValue(DeviceType.Verifone);

        //public event PublishEvent PublishEvent;
        public event DeviceEventHandler DeviceEventOccured;

        private SerialConnection SerialConnection { get; set; }

        private bool IsConnected { get; set; }

        DeviceConfig deviceConfiguration;
        DeviceSection deviceSectionConfig;

        [Inject]
        internal IVIPADevice vipaDevice { get; set; } = new VIPAImpl();

        public DeviceInformation DeviceInformation { get; private set; }

        public string ManufacturerConfigID => DeviceType.Verifone.ToString();

        public int SortOrder { get; set; } = -1;

        int ConfigurationHostId { get => deviceSectionConfig?.Verifone?.ConfigurationHostId ?? VerifoneSettingsSecurityConfiguration.ConfigurationHostId; }

        int OnlinePinKeySetId { get => deviceSectionConfig?.Verifone?.OnlinePinKeySetId ?? VerifoneSettingsSecurityConfiguration.OnlinePinKeySetId; }

        int ADEKeySetId { get => deviceSectionConfig?.Verifone?.ADEKeySetId ?? VerifoneSettingsSecurityConfiguration.ADEKeySetId; }

        string ConfigurationPackageActive { get => deviceSectionConfig?.Verifone?.ConfigurationPackageActive; }

        string SigningMethodActive { get => deviceSectionConfig?.Verifone?.SigningMethodActive; }

        string ActiveCustomerId { get => deviceSectionConfig?.Verifone?.ActiveCustomerId; }

        public VerifoneDevice()
        {

        }

        public object Clone()
        {
            VerifoneDevice clonedObj = new VerifoneDevice();
            return clonedObj;
        }

        public void Dispose()
        {
            vipaDevice?.Dispose();
            IsConnected = false;
        }

        public void Disconnect()
        {
            SerialConnection?.Disconnect();
            IsConnected = false;
        }

        bool ICardDevice.IsConnected(object request)
        {
            return IsConnected;
        }

        public void SetDeviceSectionConfig(DeviceSection config)
        {
            deviceSectionConfig = config;
            if (vipaDevice != null)
            {
                Console.WriteLine($"\r\nACTIVE CONFIGURATION _: {deviceSectionConfig.Verifone?.ConfigurationPackageActive}");
                string onlinePINSource = deviceSectionConfig.Verifone?.ConfigurationHostId == VerifoneSettingsSecurityConfiguration.ConfigurationHostId ? "VSS" : "IPP";
                Console.WriteLine($"ONLINE DEBIT PIN STORE: {onlinePINSource}");
                vipaDevice.LoadDeviceSectionConfig(deviceSectionConfig);
            }
        }

        public List<LinkErrorValue> Probe(DeviceConfig config, DeviceInformation deviceInfo, out bool active)
        {
            DeviceInformation = deviceInfo;
            DeviceInformation.Manufacturer = ManufacturerConfigID;
            DeviceInformation.ComPort = deviceInfo.ComPort;

            SerialConnection = new SerialConnection(DeviceInformation);
            active = IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);

            if (active)
            {
                (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                {
                    // check for power on notification: reissue reset command to obtain device information
                    if (deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification != null)
                    {
                        Console.WriteLine($"\nDEVICE EVENT: Terminal ID={deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification?.TerminalID}," +
                            $" EVENT='{deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification?.TransactionStatusMessage}'");

                        deviceIdentifier = vipaDevice.DeviceCommandReset();

                        if (deviceIdentifier.VipaResponse != (int)VipaSW1SW2Codes.Success)
                        {
                            return null;
                        }
                    }

                    if (DeviceInformation != null)
                    {
                        DeviceInformation.Manufacturer = ManufacturerConfigID;
                        DeviceInformation.Model = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model;
                        DeviceInformation.SerialNumber = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.SerialNumber;
                        DeviceInformation.FirmwareVersion = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.FirmwareVersion;
                    }
                    vipaDevice = vipaDevice;
                    deviceConfiguration = config;
                    active = true;

                    //Console.WriteLine($"\nDEVICE PROBE SUCCESS ON {DeviceInformation?.ComPort}, FOR SN: {DeviceInformation?.SerialNumber}");
                }
                else
                {
                    //vipaDevice.CancelResponseHandlers();
                    //Console.WriteLine($"\nDEVICE PROBE FAILED ON {DeviceInformation?.ComPort}\n");
                }
            }
            return null;
        }

        public List<DeviceInformation> DiscoverDevices()
        {
            List<DeviceInformation> deviceInformation = new List<DeviceInformation>();
            Connection.DeviceDiscovery deviceDiscovery = new Connection.DeviceDiscovery();
            if (deviceDiscovery.FindVerifoneDevices())
            {
                foreach (var device in deviceDiscovery.deviceInfo)
                {
                    if (string.IsNullOrEmpty(device.ProductID) || string.IsNullOrEmpty(device.SerialNumber))
                        throw new Exception("The connected device's PID or SerialNumber did not match with the expected values!");

                    deviceInformation.Add(new DeviceInformation()
                    {
                        ComPort = device.ComPort,
                        ProductIdentification = device.ProductID,
                        SerialNumber = device.SerialNumber,
                        VendorIdentifier = Connection.DeviceDiscovery.VID
                    });

                    System.Diagnostics.Debug.WriteLine($"device: ON PORT={device.ComPort} - VERIFONE MODEL={deviceInformation[deviceInformation.Count - 1].ProductIdentification}, " +
                        $"SN=[{deviceInformation[deviceInformation.Count - 1].SerialNumber}], PORT={deviceInformation[deviceInformation.Count - 1].ComPort}");
                }
            }

            // validate COMM Port
            if (!deviceDiscovery.deviceInfo.Any() || deviceDiscovery.deviceInfo[0].ComPort == null || !deviceDiscovery.deviceInfo[0].ComPort.Any())
            {
                return null;
            }

            return deviceInformation;
        }

        public void DeviceSetIdle()
        {
            //Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: SET TO IDLE.");
            if (vipaDevice != null)
            {
                vipaDevice.DisplayMessage(VIPAImpl.VIPADisplayMessageValue.Idle);
            }
        }

        public bool DeviceRecovery()
        {
            Console.WriteLine($"DEVICE: ON PORT={DeviceInformation.ComPort} - DEVICE-RECOVERY");
            return false;
        }

        public List<LinkRequest> GetDeviceResponse(LinkRequest deviceInfo)
        {
            throw new NotImplementedException();
        }

        public string AmountToDollar(string amount)
        {
            if (amount == null)
            {
                return null;
            }

            string dollarAmount = string.Format("{0:#0.00}", Convert.ToDecimal(amount) / 100);

            return dollarAmount;
        }

        // ------------------------------------------------------------------------
        // Methods that are mapped for usage in their respective sub-workflows.
        // ------------------------------------------------------------------------
        #region --- subworkflow mapping
        public LinkRequest GetStatus(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GET STATUS for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest GetActiveKeySlot(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GET ACTIVE SLOT for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        (int VipaResult, int VipaResponse) response = vipaDevice.GetActiveKeySlot();
                        if (response.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: VIPA ACTIVE ADE KEY SLOT={response.VipaResult}\n");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED GET ACTIVE SLOT REQUEST WITH ERROR=0x{0:X4}\n", response.VipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest GetEMVKernelChecksum(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GET KERNEL CHECKSUM for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        (KernelConfigurationObject kernelConfigurationObject, int VipaResponse) response = vipaDevice.GetEMVKernelChecksum();
                        if (response.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            string[] kernelInformation = response.kernelConfigurationObject.ApplicationKernelInformation.SplitByLength(8).ToArray();

                            if (kernelInformation.Length == 4)
                            {
                                Console.WriteLine(string.Format("VIPA KERNEL CHECKSUM={0}-{1}-{2}-{3}",
                                   kernelInformation[0], kernelInformation[1], kernelInformation[2], kernelInformation[3]));
                            }
                            else
                            {
                                Console.WriteLine(string.Format("VIPA KERNEL CHECKSUM={0}",
                                    response.kernelConfigurationObject.ApplicationKernelInformation));
                            }

                            bool IsEngageDevice = BinaryStatusObject.ENGAGE_DEVICES.Any(x => x.Contains(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model.Substring(0, 4)));

                            if (response.kernelConfigurationObject.ApplicationKernelInformation.Substring(BinaryStatusObject.EMV_KERNEL_CHECKSUM_OFFSET).Equals(IsEngageDevice ? BinaryStatusObject.ENGAGE_EMV_KERNEL_CHECKSUM : BinaryStatusObject.UX301_EMV_KERNEL_CHECKSUM,
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                Console.WriteLine("VIPA EMV KERNEL VALIDATED");
                            }
                            else
                            {
                                Console.WriteLine("VIPA EMV KERNEL IS INVALID");
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED GET KERNEL CHECKSUM REQUEST WITH ERROR=0x{0:X4}\n", response.VipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest GetSecurityConfiguration(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GET SECURITY CONFIGURATION for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        bool activeSigningMethodIsSphere = SigningMethodActive.Equals("SPHERE");
                        bool activeSigningMethodIsVerifone = SigningMethodActive.Equals("VERIFONE");

                        (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) config = (new SecurityConfigurationObject(), (int)VipaSW1SW2Codes.Failure);
                        config = vipaDevice.GetSecurityConfiguration(deviceSectionConfig.Verifone.ConfigurationHostId, DeviceInformation.ADEKeySetId);
                        if (config.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: FIRMARE VERSION___={deviceIdentifier.deviceInfoObject.LinkDeviceResponse.FirmwareVersion}");
                            Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber ?? "??"} KEY KSN____={config.securityConfigurationObject.SRedCardKSN ?? "[ *** NOT FOUND *** ]"}");
                            if (config.securityConfigurationObject.SRedCardKSN != null)
                            {
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber} BDK KEY_ID_={config.securityConfigurationObject.SRedCardKSN?.Substring(4, 6)}");
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber} BDK TRSM ID={config.securityConfigurationObject.SRedCardKSN?.Substring(10, 5)}");
                            }

                            config = vipaDevice.GetSecurityConfiguration(deviceSectionConfig.Verifone.ConfigurationHostId, config.securityConfigurationObject.ADETestSlot);
                            if (config.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber ?? "??"} KEY KSN____={config.securityConfigurationObject.SRedCardKSN ?? "[ *** NOT FOUND *** ]"}");
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber} BDK KEY_ID_={config.securityConfigurationObject.SRedCardKSN?.Substring(4, 6)}");
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber} BDK TRSM ID={config.securityConfigurationObject.SRedCardKSN?.Substring(10, 5)}");
                            }
                            Console.WriteLine($"DEVICE: ADE SLOT NUMBER___=0x0{deviceSectionConfig.Verifone.ADEKeySetId}");
                            config = vipaDevice.GetSecurityConfiguration(deviceSectionConfig.Verifone.ConfigurationHostId, deviceSectionConfig.Verifone.OnlinePinKeySetId);
                            if (config.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine($"DEVICE: ONLINE PIN STORE__={(deviceSectionConfig.Verifone?.ConfigurationHostId == VerifoneSettingsSecurityConfiguration.ConfigurationHostId ? "IPP" : "VSS")}");
                                Console.WriteLine($"DEVICE: ONLINE PIN KSN____={config.securityConfigurationObject.OnlinePinKSN ?? "[ *** NOT FOUND *** ]"}");
                            }

                            // validate configuration
                            int vipaResponse = vipaDevice.ValidateConfiguration(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, activeSigningMethodIsSphere);
                            if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine($"DEVICE: CONFIGURATION IS VALID\n");
                            }
                            else
                            {
                                Console.WriteLine(string.Format("DEVICE: CONFIGURATION VALIDATION FAILED WITH ERROR=0x{0:X4}\n", vipaResponse));
                            }
                            Console.WriteLine("");
                        }
                        else
                        {
                            config = vipaDevice.GetSecurityConfiguration(config.securityConfigurationObject.ADETestSlot, config.securityConfigurationObject.VSSHostId);
                            if (config.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine($"DEVICE: FIRMARE VERSION  ={deviceIdentifier.deviceInfoObject.LinkDeviceResponse.FirmwareVersion}");
                                Console.WriteLine($"DEVICE: ADE-{config.securityConfigurationObject.KeySlotNumber} KEY KSN   ={config.securityConfigurationObject.SRedCardKSN}");
                                Console.WriteLine($"DEVICE: VSS SLOT NUMBER  ={config.securityConfigurationObject.VSSHostId - 0x01}");
                                Console.WriteLine($"DEVICE: ONLINE PIN KSN   ={config.securityConfigurationObject.OnlinePinKSN}");
                                // validate configuration
                                int vipaResponse = vipaDevice.ValidateConfiguration(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, activeSigningMethodIsSphere);
                                if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                                {
                                    Console.WriteLine($"DEVICE: CONFIGURATION IS VALID\n");
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("DEVICE: CONFIGURATION VALIDATION FAILED WITH ERROR=0x{0:X4}\n", vipaResponse));
                                }
                                Console.WriteLine("");
                            }
                        }
                        DeviceSetIdle();
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest Configuration(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: CONFIGURATION for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        int vipaResponse = (int)VipaSW1SW2Codes.Failure;

                        bool activePackageIsEpic = ConfigurationPackageActive.Equals("EPIC");
                        bool activePackageIsNJT = ConfigurationPackageActive.Equals("NJT");
                        bool activeSigningMethodIsSphere = SigningMethodActive.Equals("SPHERE");
                        bool activeSigningMethodIsVerifone = SigningMethodActive.Equals("VERIFONE");

                        if (activePackageIsEpic)
                        {
                            //vipaResponse = vipaDevice.ConfigurationFiles(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model);
                            vipaResponse = vipaDevice.EmvConfigurationPackage(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, activeSigningMethodIsSphere);
                        }
                        else if (activePackageIsNJT)
                        {
                            vipaResponse = vipaDevice.ConfigurationPackage(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, activeSigningMethodIsSphere);
                        }
                        else
                        {
                            Console.WriteLine($"DEVICE: INVALID CONFIGURATION {ConfigurationPackageActive}\n");
                        }

                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: CONFIGURATION UPDATED SUCCESSFULLY\n");

                            (DevicePTID devicePTID, int VipaResponse) response = (null, (int)VipaSW1SW2Codes.Success);

                            // TGZ files require reboot
                            //if (activePackageIsEpic)
                            //{
                            //    Console.Write("DEVICE: RELOADING CONFIGURATION...");
                            //    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceExtendedReset();

                            //    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            //    {
                            //        Console.WriteLine("SUCCESS!");
                            //    }
                            //    else
                            //    {
                            //        Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                            //    }
                            //}
                            //else
                            {
                                Console.Write("DEVICE: REQUESTING DEVICE REBOOT...");
                                (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceCommandReset();

                                if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                                {
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                                }
                                response = vipaDevice.DeviceReboot();
                            }

                            if (response.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                if (activePackageIsNJT)
                                {
                                    Console.WriteLine($"DEVICE: REBOOT REQUEST SUCCESSFUL for ID={response.devicePTID.PTID}, SN={response.devicePTID.SerialNumber}\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine(string.Format("DEVICE: FAILED REBOOT REQUEST WITH ERROR=0x{0:X4}\n", response.VipaResponse));
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED CONFIGURATION REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest FeatureEnablementToken(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: FEATURE ENABLEMENT TOKEN for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        int vipaResponse = vipaDevice.FeatureEnablementToken();
                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: FET UPDATED SUCCESSFULLY\n");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED FET REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest LockDeviceConfiguration0(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: LOCK DEVICE CONFIGURATION 0 for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        bool activePackageIsEpic = ConfigurationPackageActive.Equals("EPIC");
                        bool activePackageIsNJT = ConfigurationPackageActive.Equals("NJT");
                        bool activeSigningMethodIsSphere = SigningMethodActive.Equals("SPHERE");
                        bool activeSigningMethodIsVerifone = SigningMethodActive.Equals("VERIFONE");

                        int vipaResponse = vipaDevice.LockDeviceConfiguration0(activePackageIsEpic, activeSigningMethodIsSphere);

                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: CONFIGURATION LOCKED SUCCESSFULLY\n");

                            Console.Write("DEVICE: REQUESTING DEVICE REBOOT...");
                            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceCommandReset();

                            if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine("SUCCESS!");
                            }
                            else
                            {
                                Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                            }
                            vipaDevice.DeviceReboot();
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED LOCK CONFIGURATION REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest LockDeviceConfiguration8(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: LOCK DEVICE CONFIGURATION 8 for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        bool activePackageIsEpic = ConfigurationPackageActive.Equals("EPIC");
                        bool activePackageIsNJT = ConfigurationPackageActive.Equals("NJT");
                        bool activeSigningMethodIsSphere = SigningMethodActive.Equals("SPHERE");
                        bool activeSigningMethodIsVerifone = SigningMethodActive.Equals("VERIFONE");

                        int vipaResponse = vipaDevice.LockDeviceConfiguration8(activePackageIsEpic, activeSigningMethodIsSphere);

                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: CONFIGURATION LOCKED SUCCESSFULLY\n");

                            Console.Write("DEVICE: REQUESTING DEVICE REBOOT...");
                            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceCommandReset();

                            if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine("SUCCESS!");
                            }
                            else
                            {
                                Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                            }
                            vipaDevice.DeviceReboot();
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED LOCK CONFIGURATION REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest UnlockDeviceConfiguration(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: UNLOCK DEVICE CONFIGURATION for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        int vipaResponse = vipaDevice.UnlockDeviceConfiguration();
                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: CONFIGURATION UNLOCKED SUCCESSFULLY\n");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED UNLOCK CONFIGURATION REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest AbortCommand(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE: ABORT COMMAND for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest ResetDevice(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE: RESET DEVICE for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest RebootDevice(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: REBOOT DEVICE with SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        (DevicePTID devicePTID, int VipaResponse) response = vipaDevice.DeviceReboot();
                        if (response.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            //Console.WriteLine($"DEVICE: REBOOT SUCCESSFULLY for ID={response.devicePTID.PTID}, SN={response.devicePTID.SerialNumber}\n");
                            Console.WriteLine($"DEVICE: REBOOT REQUEST RECEIVED SUCCESSFULLY");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED REBOOT REQUEST WITH ERROR=0x{0:X4}\n", response.VipaResponse));
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest UpdateHMACKeys(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: UPDATE HMAC KEYS for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        int vipaResponse = vipaDevice.UpdateHMACKeys();
                        if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: HMAC KEYS UPDATED SUCCESSFULLY\n");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED HMAC KEYS UPDATE WITH ERROR=0x{0:X4}\n", vipaResponse));
                        }
                        DeviceSetIdle();
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest GenerateHMAC(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GENERATE HMAC for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        (string HMAC, int VipaResponse) config = vipaDevice.GenerateHMAC();
                        if (config.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine($"DEVICE: HMAC={config.HMAC}\n");
                        }
                        DeviceSetIdle();
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest UpdateIdleScreen(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: UPDATE IDLE SCREEN for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        bool activeSigningMethodIsSphere = SigningMethodActive.Equals("SPHERE");
                        bool activeSigningMethodIsVerifone = SigningMethodActive.Equals("VERIFONE");

                        try
                        {
                            int vipaResponse = vipaDevice.UpdateIdleScreen(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, activeSigningMethodIsSphere, ActiveCustomerId);

                            if (vipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine($"DEVICE: IDLE SCREEN CONFIGURATION UPDATED SUCCESSFULLY\n");

                                // TGZ files require REBOOT
                                //Console.Write("DEVICE: RELOADING CONFIGURATION...");
                                //(DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceExtendedReset();

                                //if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                                //{
                                //    Console.WriteLine("SUCCESS!");
                                //}

                                Console.Write("DEVICE: REQUESTING DEVICE REBOOT...");
                                (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceCommandReset();

                                if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                                {
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                                }
                            }
                            else if (vipaResponse == (int)VipaSW1SW2Codes.DeviceNotSupported)
                            {
                                Console.WriteLine(string.Format("DEVICE: UNSUPPORTED DEVICE ERROR=0x{0:X4}\n", vipaResponse));
                            }
                            else
                            {
                                Console.WriteLine(string.Format("DEVICE: FAILED IDLE SCREEN CONFIGURATION REQUEST WITH ERROR=0x{0:X4}\n", vipaResponse));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"UPDATE IDLE SCREEN COMMAND ERROR=[{e.Message}]");
                        }
                    }
                }
            }

            return linkRequest;
        }

        public LinkRequest DisplayCustomScreen(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: DISPLAY CUSTOM SCREEN for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        long amount = 9999999;
                        string requestedAmount = amount.ToString();

                        // Total..... (amount)| VERIFY AMOUNT| YES| NO
                        string displayMessage = $"VERIFY AMOUNT|Total.....${AmountToDollar(requestedAmount)}|YES|NO";

                        (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) verifyAmountResponse = vipaDevice.DisplayCustomScreen(displayMessage);

                        if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine("DEVICE: CUSTOM SCREEN EXECUTED SUCCESSFULLY - RESPONSE={0}\n", verifyAmountResponse.LinkActionRequestIPA5Object.DALResponseData.Value.Equals("1", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO");
                        }
                        else if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.DeviceNotSupported)
                        {
                            Console.WriteLine(string.Format("DEVICE: UNSUPPORTED DEVICE ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED DISPLAY CUSTOM SCREEN REQUEST WITH ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }
                    }
                }
            }

            DeviceSetIdle();

            return linkRequest;
        }

        public LinkRequest Reboot24Hour(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: 24 HOUR REBOOT for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        string timestamp = linkRequest.Actions.First().DeviceActionRequest.Reboot24Hour;
                        (string Timestamp, int VipaResponse) deviceResponse = vipaDevice.Reboot24Hour(timestamp);
                        if (deviceResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            if (timestamp.Equals(deviceResponse.Timestamp))
                            {
                                //Console.Write("DEVICE: RELOADING CONFIGURATION...");
                                //(DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceExtendedReset();

                                //if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                                //{
                                //    Console.WriteLine("SUCCESS!");
                                //}
                                //else
                                //{
                                //    Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                                //}
                                Console.Write("DEVICE: REQUESTING DEVICE REBOOT...");
                                (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifierExteneded = vipaDevice.DeviceCommandReset();

                                if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                                {
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    Console.WriteLine("FAILURE - PLEASE REBOOT DEVICE!");
                                }
                                vipaDevice.DeviceReboot();
                            }
                            else
                            {
                                Console.WriteLine($"FAILURE - INCORRECT TIMESTAMP SET: [{deviceResponse.Timestamp}]");
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED 24 HOUR REBOOT REQUEST WITH ERROR=0x{0:X4}\n", deviceResponse.VipaResponse));
                        }
                    }
                }
            }

            DeviceSetIdle();

            return linkRequest;
        }

        public LinkRequest SetTerminalDateTime(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: 24 HOUR REBOOT for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (vipaDevice != null)
            {
                if (!IsConnected)
                {
                    vipaDevice.Dispose();
                    SerialConnection = new SerialConnection(DeviceInformation);
                    IsConnected = vipaDevice.Connect(SerialConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = vipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        string timestamp = linkRequest.Actions.First().DeviceActionRequest.TerminalDateTime;
                        (string Timestamp, int VipaResponse) deviceResponse = vipaDevice.SetTerminalDateTime(timestamp);
                        if (deviceResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            // ignore seconds in timestamp comparison
                            if (timestamp.Substring(0, 12).Equals(deviceResponse.Timestamp.Substring(0, 12)))
                            {
                                Console.WriteLine($"SUCCESS -TIMESTAMP SET: [{deviceResponse.Timestamp}]");
                            }
                            else
                            {
                                Console.WriteLine($"FAILURE - INCORRECT TIMESTAMP SET: [{deviceResponse.Timestamp}]");
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED 24 HOUR REBOOT REQUEST WITH ERROR=0x{0:X4}\n", deviceResponse.VipaResponse));
                        }
                    }
                }
            }

            DeviceSetIdle();

            return linkRequest;
        }

        #endregion --- subworkflow mapping
    }
}
