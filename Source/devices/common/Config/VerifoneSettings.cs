﻿using System;
using System.Collections.Generic;

namespace Devices.Common.Config
{
    [Serializable]
    public class VerifoneSettings
    {
        public int SortOrder { get; set; } = -1;
        public List<string> SupportedDevices { get; internal set; } = new List<string>();
        public byte OnlinePinHostId { get; set; } = VerifoneSettingsOnlinePin.OnlinePinHostId;
        public byte OnlinePinKeySetId { get; set; } = VerifoneSettingsOnlinePin.OnlinePinKeySetId;
        public List<string> ConfigurationPackages { get; internal set; } = new List<string>();
        public string ConfigurationPackageActive { get; set; } = VerifoneSettingsConfigurationPackages.Epic;
    }

    /// <summary>
    /// VSS ONLINE PIN SETUP: hostId = 0x02, KeySetId = 0x00
    /// IPP ONLINE PIN SETUP: hostId = 0x05, KeySetId = 0x01
    /// </summary>
    public static class VerifoneSettingsOnlinePin
    {
        public const byte OnlinePinHostId = 0x02;
        public const byte OnlinePinKeySetId = 0x00;
    }

    /// <summary>
    /// Verifone Configuration Support
    /// NJT : non-EMV
    /// EPIC: EMV
    /// </summary>
    public static class VerifoneSettingsConfigurationPackages
    {
        public static string Epic = "EPIC";
        public static string NJT = "NJT";
    }
}
