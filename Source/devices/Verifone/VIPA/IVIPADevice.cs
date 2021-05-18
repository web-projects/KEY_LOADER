using Devices.Verifone.Helpers;
using Devices.Verifone.TLV;
using Devices.Verifone.Connection;
using System.Collections.Generic;
using static Devices.Verifone.VIPA.VIPAImpl;
using Config;
using Devices.Common.AppConfig;
using Devices.Common;
using System;
using XO.Private;

namespace Devices.Verifone.VIPA
{
    public interface IVIPADevice
    {
        bool Connect(SerialConnection connection, DeviceInformation deviceInformation);

        void Dispose();
        
        void ResponseCodeHandler(List<TLVImpl> tags, int responseCode, bool cancelled = false);

        bool DisplayMessage(VIPADisplayMessageValue displayMessageValue = VIPADisplayMessageValue.Idle, bool enableBacklight = false, string customMessage = "");

        (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceCommandReset();

        (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceExtendedReset();

        (DevicePTID devicePTID, int VipaResponse) DeviceReboot();

        (int VipaResult, int VipaResponse) GetActiveKeySlot();

        (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetSecurityConfiguration(byte hostID, byte vssSlot);

        (KernelConfigurationObject kernelConfigurationObject, int VipaResponse) GetEMVKernelChecksum();

        //[Obsolete]
        int ConfigurationFiles(string deviceModel);

        int ConfigurationPackage(string deviceModel);

        int EmvConfigurationPackage(string deviceModel);

        int ValidateConfiguration(string deviceModel);

        int FeatureEnablementToken();

        int LockDeviceConfiguration0(bool activeConfigurationIsEpic);

        int LockDeviceConfiguration8(bool activeConfigurationIsEpic);

        int UnlockDeviceConfiguration();

        (string HMAC, int VipaResponse) GenerateHMAC();

        int UpdateHMACKeys();
        
        void LoadDeviceSectionConfig(DeviceSection deviceSectionConfig);

        int UpdateIdleScreen(string deviceModel);

        (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) DisplayCustomScreen(string displayMessage);

        (string Timestamp, int VipaResponse) Reboot24Hour(string timestamp);
    }
}