namespace Devices.Verifone.Helpers
{
    public class BinaryStatusObject
    {
        public const string MAPP_SRED_CONFIG = "mapp_vsd_sred.cfg";

        public bool FileNotFound { get; set; }
        public int FileSize { get; set; }
        public string FileCheckSum { get; set; }
        public int SecurityStatus { get; set; }
        public byte[] ReadResponseBytes { get; set; }
    }
}
