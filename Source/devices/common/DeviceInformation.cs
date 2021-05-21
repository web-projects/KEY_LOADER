using Devices.Common.Config;

namespace Devices.Common
{
    public class DeviceInformation
    {
        public string ComPort { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string ProductIdentification { get; set; }
        public string VendorIdentifier { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public byte ConfigurationHostId { get; set; } = VerifoneSettingsOnlinePin.ConfigurationHostId;
        public byte OnlinePinKeySetId { get; set; } = VerifoneSettingsOnlinePin.OnlinePinKeySetId;
        public string ConfigurationPackageActive { get; set; }
        public string SigningMethodActive { get; set; }
        public string ActiveCustomerId { get; set; }
    }
}
