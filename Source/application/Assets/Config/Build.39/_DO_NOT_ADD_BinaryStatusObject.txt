using System.Collections.Generic;

namespace Devices.Verifone.Helpers
{
    public class BinaryStatusObject
    {
        public const string DEVICE_UX100 = "UX100";
        public const string DEVICE_UX300 = "UX300";
        public const string DEVICE_UX301 = "UX301";
        public const string DEVICE_P200 = "P200";
        public const string DEVICE_P400 = "P400";

        public static readonly string[] ALL_DEVICES = { DEVICE_P200, DEVICE_P400, DEVICE_UX100, DEVICE_UX300, DEVICE_UX301 };
        public static readonly string[] ENGAGE_DEVICES = { DEVICE_P200, DEVICE_P400 };
        public static readonly string[] UX_DEVICES = { DEVICE_UX100, DEVICE_UX300, DEVICE_UX301 };

        public const string WHITELIST = "#whitelist.dat";
        public const string WHITELISTHASH = "247F373C9DEA79C9EEEDA69615F02E42";
        public const int WHITELISTFILESIZE = 0x129;

        public const string FET_BUNDLE = "1a.dl.zADE-Enablement-VfiDev.tar";
        public const string FET_HASH = "1C261B56A83E30413786E809D5698579";
        public const int FET_SIZE = 0x2800;

        public const string LOCK_CONFIG0_BUNDLE = "dl.bundle.Sphere_Config4-s0.tar";
        public const string LOCK_CONFIG0_HASH = "1DC66D8F45A843BB2B1ADECF7917FDAF";
        public const int LOCK_CONFIG0_SIZE = 0x5000;

        public const string LOCK_CONFIG8_BUNDLE = "dl.bundle.Sphere_Config4-s8.tar";
        public const string LOCK_CONFIG8_HASH = "B6977AEA39B9BC4423099D46F0A480E9";
        public const int LOCK_CONFIG8_SIZE = 0x5000;

        public const string UNLOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_UpdKeyCmd_Enable.tar";
        public const string UNLOCK_CONFIG_HASH = "F466919BDBCF22DBF9DAD61C1E173F61";
        public const int UNLOCK_CONFIG_SIZE = 0x5000;

        public const string CONFIG_SLOT_0_BUNDLE = "dl.bundle.Sphere_Config4-s0.tar";
        public const string CONFIG_SLOT_0_HASH = "1DC66D8F45A843BB2B1ADECF7917FDAF";
        public const int CONFIG_SLOT_0_SIZE = 0x5000;

         // AIDS
        public const string AID_00392_NAME = "a000000003.92";
        public const string AID_00392_HASH = "98B8C420A5E79C61D1B5EFD511A84244";
        public const int AID_00392_SIZE = 0x018F;
        public const string AID_00394_NAME = "a000000003.94";
        public const string AID_00394_HASH = "48A392798CA6494BC3CCCE289860877F";
        public const int AID_00394_SIZE = 0x021F;
        public const string AID_004EF_NAME = "a000000004.ef";
        public const string AID_004EF_HASH = "B01124948B68CEE0A5CF7BD1AD33689C";
        public const int AID_004EF_SIZE = 0x0220;
        public const string AID_004F1_NAME = "a000000004.f1";
        public const string AID_004F1_HASH = "F9D16DC8D08911C849E40898946DACC8";
        public const int AID_004F1_SIZE = 0x0190;
        public const string AID_025C8_NAME = "a000000025.c8";
        public const string AID_025C8_HASH = "848365828CD61B5E9CB1196E3DB9BD1C";
        public const int AID_025C8_SIZE = 0x014F;
        public const string AID_025C9_NAME = "a000000025.c9";
        public const string AID_025C9_HASH = "265FB15DE345985DBC0A84C5323A456D";
        public const int AID_025C9_SIZE = 0x018F;
        public const string AID_025CA_NAME = "a000000025.ca";
        public const string AID_025CA_HASH = "1FBEE953AD0033862BA215675844B241";
        public const int AID_025CA_SIZE = 0x021F;
        public const string AID_06511_NAME = "a000000065.11";
        public const string AID_06511_HASH = "84D71F9CB8CA709C6D2B3E0F6B8571E8";
        public const int AID_06511_SIZE = 0x018F;
        public const string AID_06513_NAME = "a000000065.13";
        public const string AID_06513_HASH = "DE947D3B5C01CEBF8E1092D3C8977975";
        public const int AID_06513_SIZE = 0x021F;
        public const string AID_1525C_NAME = "a000000152.5c";
        public const string AID_1525C_HASH = "0C5BB3A95266C46B844F605615FFF3E5";
        public const int AID_1525C_SIZE = 0x018F;
        public const string AID_1525D_NAME = "a000000152.5d";
        public const string AID_1525D_HASH = "14AF5206F20AE2E4BCCE3A6A0078D3E8";
        public const int AID_1525D_SIZE = 0x021F;
        public const string AID_384C1_NAME = "a000000384.c1";
        public const string AID_384C1_HASH = "98D37F4911E8A0F8A542A9B7494ECF9E";
        public const int AID_384C1_SIZE = 0x14F;

        // CLESS CONFIG
        public const string CONTLEMV = "contlemv.cfg";

        // ATTENDED TERMINAL
        public const string CONTLEMV_ENGAGE = "contlemv.engage";
        public const string CONTLEMVHASH_ENGAGE = "762071D3CED73923E2C59ADFD3DB1808";
        public const int CONTLEMVFILESIZE_ENGAGE = 0x3C51;

        // UNATTENDED TERMINAL
        public const string CONTLEMV_UX301 = "contlemv.ux301";
        public const string CONTLEMVHASH_UX301 = "40A5C513AFA2A03B2417B1FD8579EF40";
        public const int CONTLEMVFILESIZE_UX301 = 0x3C2E;

        // EMV CONFIG: ICCDATA.DAT
        public const string ICCDATA = "iccdata.dat";

        public const string ICCDATA_ENGAGE = "iccdata.engage";
        public const string ICCDATAHASH_ENGAGE = "AD5DC51CDA4CB9C4E5070ED9FE81ACE6";
        public const int ICCDATAFILESIZE_ENGAGE = 0x0218;

        public const string ICCDATA_UX301 = "iccdata.ux301";
        public const string ICCDATAHASH_UX301 = "1F5FF4EF05363A144FAA57EFD9DB845E";
        public const int ICCDATAFILESIZE_UX301 = 0x0218;

        // EMV CONFIG: ICCKEYS.KEY
        public const string ICCKEYS = "icckeys.key";

        public const string ICCKEYS_ENGAGE = "icckeys.engage";
        public const string ICCKEYSHASH_ENGAGE = "7828BD708DBB72265907522A426E7351";
        public const int ICCKEYSFILESIZE_ENGAGE = 0x27ED;

        public const string ICCKEYS_UX301 = "icckeys.ux301";
        public const string ICCKEYSHASH_UX301 = "1996325248070752DF6651542DD74BE5";
        public const int ICCKEYSFILESIZE_UX301 = 0x27ED;

        public static readonly string[] EMV_CONFIG_FILES = { CONTLEMV, ICCDATA, ICCKEYS };

        // EMV KERNEL CONFIGURATION
        public const int EMV_KERNEL_CHECKSUM_OFFSET = 24;

        // LOA: CONFIG=1C, TERMINAL=22 (ENGAGE DEVICES)
        public const string ENGAGE_EMV_KERNEL_CHECKSUM = "96369E1F";
        // LOA: CONFIG=8C, TERMINAL=25 (UX301 DEVICES)
        public const string UX301_EMV_KERNEL_CHECKSUM = "D196BA9D";

        // GENERIC
        public const string MAPPCFG = "mapp.cfg";
        public const string MAPPCFG_HASH = "73A38658B39019CE679D2D29B2FA7318";
        public const int MAPPCFG_FILESIZE = 0x0D23;
        public const string MAPPCFG_FILE_AFTER_REBOOT_HASH = "583B0AF5CF1DC80A5D29D4A636E2AFF5";
        public const int MAPPCFG_FILE_AFTER_REBOOT_SIZE = 0x0D0B;

        public const string CICAPPCFG = "cicapp.cfg";
        public const string CICAPPCFG_HASH = "C7BF6C37F6681327468125660E85C98C";
        public const int CICAPPCFG_FILESIZE = 0x1854;

        public const string TDOLCFG = "tdol.cfg";
        public const string TDOLCFG_HASH = "2A31EA88480256A5F1D8459FD53149D8";
        public const int TDOLCFG_FILESIZE = 0x008B;

        public static Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize, (string hash, int size) reBooted)> binaryStatus =
            new Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize, (string hash, int size) reBooted)>()
            {
                [WHITELIST] = ("WHITELIST", BinaryStatusObject.ALL_DEVICES, WHITELIST, WHITELISTHASH, WHITELISTFILESIZE, (string.Empty, 0)),
                // AIDS
                [AID_00392_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_00392_NAME, AID_00392_HASH, AID_00392_SIZE, (string.Empty, 0)),
                [AID_00394_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_00394_NAME, AID_00394_HASH, AID_00394_SIZE, (string.Empty, 0)),
                [AID_004EF_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_004EF_NAME, AID_004EF_HASH, AID_004EF_SIZE, (string.Empty, 0)),
                [AID_004F1_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_004F1_NAME, AID_004F1_HASH, AID_004F1_SIZE, (string.Empty, 0)),
                [AID_025C8_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025C8_NAME, AID_025C8_HASH, AID_025C8_SIZE, (string.Empty, 0)),
                [AID_025C9_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025C9_NAME, AID_025C9_HASH, AID_025C9_SIZE, (string.Empty, 0)),
                [AID_025CA_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025CA_NAME, AID_025CA_HASH, AID_025CA_SIZE, (string.Empty, 0)),
                [AID_06511_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_06511_NAME, AID_06511_HASH, AID_06511_SIZE, (string.Empty, 0)),
                [AID_06513_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_06513_NAME, AID_06513_HASH, AID_06513_SIZE, (string.Empty, 0)),
                [AID_1525C_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_1525C_NAME, AID_1525C_HASH, AID_1525C_SIZE, (string.Empty, 0)),
                [AID_1525D_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_1525D_NAME, AID_1525D_HASH, AID_1525D_SIZE, (string.Empty, 0)),
                [AID_384C1_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_384C1_NAME, AID_384C1_HASH, AID_384C1_SIZE, (string.Empty, 0)),
                // CLESS CONFIG
                [CONTLEMV_ENGAGE] = ("CLESS", BinaryStatusObject.ENGAGE_DEVICES, CONTLEMV, CONTLEMVHASH_ENGAGE, CONTLEMVFILESIZE_ENGAGE, (string.Empty, 0)),
                [CONTLEMV_UX301] = ("CLESS", BinaryStatusObject.UX_DEVICES, CONTLEMV, CONTLEMVHASH_UX301, CONTLEMVFILESIZE_UX301, (string.Empty, 0)),
                // EMV CONFIG
                [ICCDATA_ENGAGE] = ("EMV", BinaryStatusObject.ENGAGE_DEVICES, ICCDATA, ICCDATAHASH_ENGAGE, ICCDATAFILESIZE_ENGAGE, (string.Empty, 0)),
                [ICCDATA_UX301] = ("EMV", BinaryStatusObject.UX_DEVICES, ICCDATA, ICCDATAHASH_UX301, ICCDATAFILESIZE_UX301, (string.Empty, 0)),
                [ICCKEYS_ENGAGE] = ("EMV", BinaryStatusObject.ENGAGE_DEVICES, ICCKEYS, ICCKEYSHASH_ENGAGE, ICCKEYSFILESIZE_ENGAGE, (string.Empty, 0)),
                [ICCKEYS_UX301] = ("EMV", BinaryStatusObject.UX_DEVICES, ICCKEYS, ICCKEYSHASH_UX301, ICCKEYSFILESIZE_UX301, (string.Empty, 0)),
                // GENERIC
                [MAPPCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, MAPPCFG, MAPPCFG_HASH, MAPPCFG_FILESIZE, (MAPPCFG_FILE_AFTER_REBOOT_HASH, MAPPCFG_FILE_AFTER_REBOOT_SIZE)),
                [CICAPPCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, CICAPPCFG, CICAPPCFG_HASH, CICAPPCFG_FILESIZE, (string.Empty, 0)),
                [TDOLCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, TDOLCFG, TDOLCFG_HASH, TDOLCFG_FILESIZE, (string.Empty, 0))
            };

        public const string MAPP_SRED_CONFIG = "mapp_vsd_sred.cfg";

        public bool FileNotFound { get; set; }
        public int FileSize { get; set; }
        public string FileCheckSum { get; set; }
        public int SecurityStatus { get; set; }
        public byte[] ReadResponseBytes { get; set; }
    }
}
