namespace Devices.Verifone.Helpers
{
    public class BinaryStatusObject
    {
        public const string MAPP_SRED_CONFIG = "mapp_vsd_sred.cfg";

        public const string LOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_UpdKeyCmd_Disable.tar";
        public const string UNLOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_UpdKeyCmd_Enable.tar";
        public const string CONFIG_SLOT_0_BUNDLE = "dl.bundle.Sphere_Config3-s0.tar";
        public const string CONFIG_SLOT_8_BUNDLE = "dl.bundle.Sphere_Config3-s8.tar";

        public bool FileNotFound { get; set; }
        public int FileSize { get; set; }
        public string FileCheckSum { get; set; }
        public int SecurityStatus { get; set; }
        public byte[] ReadResponseBytes { get; set; }
    }
}
