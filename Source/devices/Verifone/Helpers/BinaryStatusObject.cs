namespace Devices.Verifone.Helpers
{
    public class BinaryStatusObject
    {
        public const string MAPP_SRED_CONFIG = "mapp_vsd_sred.cfg";

        public const string FET_BUNDLE = "1a.dl.zADE-Enablement-VfiDev.tar";
        public const string FET_HASH = "1C261B56A83E30413786E809D5698579";
        public const int FET_SIZE = 0x2800;

        public const string LOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_Config3-s8.tar";
        public const string LOCK_CONFIG_HASH = "BB1613802E6FF387FC453D76E17A829D";
        public const int LOCK_CONFIG_SIZE = 0x5000;

        public const string UNLOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_UpdKeyCmd_Enable.tar";
        public const string UNLOCK_CONFIG_HASH = "F466919BDBCF22DBF9DAD61C1E173F61";
        public const int UNLOCK_CONFIG_SIZE = 0x5000;

        public const string CONFIG_SLOT_0_BUNDLE = "dl.bundle.Sphere_Config3-s0.tar";
        public const string CONFIG_SLOT_0_HASH = "685E453581F85336C3AA012292E6B08A";
        public const int CONFIG_SLOT_0_SIZE = 0x5000;

        public bool FileNotFound { get; set; }
        public int FileSize { get; set; }
        public string FileCheckSum { get; set; }
        public int SecurityStatus { get; set; }
        public byte[] ReadResponseBytes { get; set; }
    }
}
