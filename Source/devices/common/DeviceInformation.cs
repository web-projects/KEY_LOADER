using Devices.Common.Config;

namespace Devices.Common
{
    public class DeviceInformation
    {
        public string ComPort { get; set; }
        public string SerialNumber { get; set; }
        public string ProductIdentification { get; set; }
        public string VendorIdentifier { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public byte OnlinePinHostId { get; set; } = VerifoneSettingsOnlinePin.OnlinePinHostId;
        public byte OnlinePinKeySetId { get; set; } = VerifoneSettingsOnlinePin.OnlinePinKeySetId;
        public string ConfigurationPackageActive { get; set; }
    }
}
