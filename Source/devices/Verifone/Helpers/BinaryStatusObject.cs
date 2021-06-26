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
        public const string DEVICE_M400 = "M400";

        public static readonly string[] ALL_DEVICES = { DEVICE_P200, DEVICE_P400, DEVICE_M400, DEVICE_UX100, DEVICE_UX300, DEVICE_UX301 };
        public static readonly string[] ENGAGE_DEVICES = { DEVICE_P200, DEVICE_P400, DEVICE_M400 };
        public static readonly string[] UX_DEVICES = { DEVICE_UX100, DEVICE_UX300, DEVICE_UX301 };

        public const string WHITELIST = "#whitelist.dat";
        public const string WHITELISTHASH = "F914E92759E021F2A38AEFFF4D342A9D";
        public const int WHITELISTFILESIZE = 0x128;
        public const string WHITELIST_FILE_HMAC_HASH = "33514E07FFD021D98B80DC3990A9B8F0";
        public const int WHITELIST_FILE_HMAC_SIZE = 0x0131;

        public const string FET_BUNDLE = "1a.dl.zADE-Enablement-VfiDev.tar";
        public const string FET_HASH = "1C261B56A83E30413786E809D5698579";
        public const int FET_SIZE = 0x2800;

        //public const string UNLOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_UpdKeyCmd_Enable.tar";
        //public const string UNLOCK_CONFIG_HASH = "F466919BDBCF22DBF9DAD61C1E173F61";
        //public const int UNLOCK_CONFIG_SIZE = 0x5000;
        public const string UNLOCK_CONFIG_BUNDLE = "dl.bundle.Sphere_Config_nokeysigning.tar";
        public const string UNLOCK_CONFIG_HASH = "457EB4980E801C1D7883608B0A8CB492";
        public const int UNLOCK_CONFIG_SIZE = 0x5000;

        // RAW CONFIGURATION FILES
        #region --- raw config files ---

        // AIDS
        public const string AID_00392_NAME = "a000000003.92";
        public const string AID_00392_HASH = "98B8C420A5E79C61D1B5EFD511A84244";
        public const int AID_00392_SIZE = 0x018F;
        public const string AID_00394_NAME = "a000000003.94";
        public const string AID_00394_HASH = "48A392798CA6494BC3CCCE289860877F";
        public const int AID_00394_SIZE = 0x021F;
        public const string AID_004EF_NAME = "a000000004.ef";
        public const string AID_004EF_HASH = "B01124948B68CEE0A5CF7BD1AD33689C";
        public const int AID_004EF_SIZE = 0x0221;
        public const string AID_004EF_HMAC_HASH = "0592DF7F94C39FF23AF15C6836967C73";
        public const int AID_004EF_HMAC_SIZE = 0x0221;
        public const string AID_004F1_NAME = "a000000004.f1";
        public const string AID_004F1_HASH = "F9D16DC8D08911C849E40898946DACC8";
        public const int AID_004F1_SIZE = 0x0191;
        public const string AID_004F1_HMAC_HASH = "994C1F789205834E22B7BC1CF461A1E7";
        public const int AID_004F1_HMAC_SIZE = 0x0191;
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
        public const string CONTLEMV_HASH_ENGAGE = "5A0808695BBE305117B41A5A3AF7F47F";
        public const int CONTLEMV_FILESIZE_ENGAGE = 0x3E6D;
        public const string CONTLEMV_HMACHASH_ENGAGE = "4FF41F1E18E4954CACCCB03174638767";
        public const int CONTLEMV_HMACFILESIZE_ENGAGE = 0x3E6D;

        // UNATTENDED TERMINAL
        public const string CONTLEMV_UX301 = "contlemv.ux301";
        public const string CONTLEMV_HASH_UX301 = "F7F0741C8CED4224DD5C536959471319";
        public const int CONTLEMV_FILESIZE_UX301 = 0x3B1A;
        public const string CONTLEMV_HMACHASH_UX301 = "C0325A1129F5380BB7D64C7F4F263066";
        public const int CONTLEMV_HMACFILESIZE_UX301 = 0x3E4A;

        // EMV CONFIG: ICCDATA.DAT
        public const string ICCDATA = "iccdata.dat";

        public const string ICCDATA_ENGAGE = "iccdata.engage";
        public const string ICCDATA_HASH_ENGAGE = "7592402F5EF0FB8CB7BC9F0AA86DE325";
        public const int ICCDATA_FILESIZE_ENGAGE = 0x024E;
        public const string ICCDATA_HMACHASH_ENGAGE = "6F6325B88CEE560797A58276ED5584CE";
        public const int ICCDATA_HMACFILESIZE_ENGAGE = 0x024E;

        public const string ICCDATA_UX301 = "iccdata.ux301";
        public const string ICCDATAHASH_UX301 = "9C97C469B47B8FF27CBE9B244DD5AC94";
        public const int ICCDATAFILESIZE_UX301 = 0x0218;
        public const string ICCDATA_HMACHASH_UX301 = "FFCC54F1479C3D0073E5A673B2E67026";
        public const int ICCDATA_HMACFILESIZE_UX301 = 0x024E;

        // EMV CONFIG: ICCKEYS.KEY
        public const string ICCKEYS = "icckeys.key";

        public const string ICCKEYS_ENGAGE = "icckeys.engage";
        public const string ICCKEYS_HASH_ENGAGE = "8DEA6526A0B568934F20A15E16438A61";
        public const int ICCKEYS_FILESIZE_ENGAGE = 0x2A0B;
        public const string ICCKEYS_HMACHASH_ENGAGE = "2980B283880151878EFB8864D29A44A5";
        public const int ICCKEYS_HMACFILESIZE_ENGAGE = 0x2A0B;

        public const string ICCKEYS_UX301 = "icckeys.ux301";
        public const string ICCKEYSHASH_UX301 = "3CFEF99AFC0DEF70494553A2D4DF699D";
        public const int ICCKEYSFILESIZE_UX301 = 0x328D;
        public const string ICCKEYS_HMACHASH_UX301 = "52562953A245B79938A1A7E57CF3831E";
        public const int ICCKEYS_HMACFILESIZE_UX301 = 0x2A0B;

        public static readonly string[] EMV_CONFIG_FILES = { CONTLEMV, ICCDATA, ICCKEYS };

        // EMV KERNEL CONFIGURATION
        public const int EMV_KERNEL_CHECKSUM_OFFSET = 24;

        // LOA: CONFIG=1C, TERMINAL=22 (ENGAGE DEVICES)
        public const string ENGAGE_EMV_KERNEL_CHECKSUM = "96369E1F";
        // LOA: CONFIG=8C, TERMINAL=25 (UX301 DEVICES)
        public const string UX301_EMV_KERNEL_CHECKSUM = "D196BA9D";

        // GENERIC
        public const string CARDAPPCFG = "cardapp.cfg";
        public const string CARDAPPCFG_HASH = "DF61BC7C938CD116D1EB5C8D751066DE";
        public const int CARDAPPCFG_FILESIZE = 0x149B;
        public const string CARDAPPCFG_HMACHASH = "92DFBBDB39B270890347C3F1ED85C16C";
        public const int CARDAPPCFG_HMACFILESIZE = 0x1703;

        public const string CICAPPCFG = "cicapp.cfg";
        public const string CICAPPCFG_HASH = "C7BF6C37F6681327468125660E85C98C";
        public const int CICAPPCFG_FILESIZE = 0x1854;
        public const string CICAPPCFG_HMACHASH = "92DFBBDB39B270890347C3F1ED85C16C";
        public const int CICAPPCFG_HMACFILESIZE = 0x1703;

        public const string MAPPCFG = "mapp.cfg";
        public const string MAPPCFG_HASH = "EB64A8AE5C4B21C80CA1AB613DDFB61D";
        public const int MAPPCFG_FILESIZE = 0x1332;
        public const string MAPPCFG_HMACHASH = "020B681C04C3822CBFB1649CBE9AE08B";
        public const int MAPPCFG_HMACFILESIZE = 0x1332;

        public const string TDOLCFG = "tdol.cfg";
        public const string TDOLCFG_HASH = "2A31EA88480256A5F1D8459FD53149D8";
        public const int TDOLCFG_FILESIZE = 0x00AD;
        public const string TDOLCFG_HMACHASH = "10501C44678FEA6272BB2BB911C7A797";
        public const int TDOLCFG_HMACFILESIZE = 0x00AD;

        public const string TRMDOLCFG = "trmdol.cfg";
        public const string TRMDOLCFG_HASH = "1F884C6C441E7F33EE4A47FBE8C92124";
        public const int TRMDOLCFG_FILESIZE = 0x0115;
        public const string TRMDOLCFG_HMACHASH = "115BE5F7FC12F2D1D3663E1E61194A60";
        public const int TRMDOLCFG_HMACFILESIZE = 0x004D;

        public static Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize, (string hash, int size) reBooted)> binaryStatus =
            new Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize, (string hash, int size) reBooted)>()
            {
                // THIS FILE IS NOT ADDED WHEN SphereConfig is applied to lock-down the device
                //[WHITELIST] = ("WHITELIST", BinaryStatusObject.ALL_DEVICES, WHITELIST, WHITELISTHASH, WHITELISTFILESIZE, (WHITELIST_FILE_AFTER_REBOOT_HASH, WHITELIST_FILE_AFTER_REBOOT_SIZE)),
                // AIDS
                [AID_00392_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_00392_NAME, AID_00392_HASH, AID_00392_SIZE, (string.Empty, 0)),
                [AID_00394_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_00394_NAME, AID_00394_HASH, AID_00394_SIZE, (string.Empty, 0)),
                [AID_004EF_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_004EF_NAME, AID_004EF_HASH, AID_004EF_SIZE, (AID_004EF_HMAC_HASH, AID_004EF_HMAC_SIZE)),
                [AID_004F1_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_004F1_NAME, AID_004F1_HASH, AID_004F1_SIZE, (AID_004F1_HMAC_HASH, AID_004F1_HMAC_SIZE)),
                [AID_025C8_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025C8_NAME, AID_025C8_HASH, AID_025C8_SIZE, (string.Empty, 0)),
                [AID_025C9_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025C9_NAME, AID_025C9_HASH, AID_025C9_SIZE, (string.Empty, 0)),
                [AID_025CA_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_025CA_NAME, AID_025CA_HASH, AID_025CA_SIZE, (string.Empty, 0)),
                [AID_06511_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_06511_NAME, AID_06511_HASH, AID_06511_SIZE, (string.Empty, 0)),
                [AID_06513_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_06513_NAME, AID_06513_HASH, AID_06513_SIZE, (string.Empty, 0)),
                [AID_1525C_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_1525C_NAME, AID_1525C_HASH, AID_1525C_SIZE, (string.Empty, 0)),
                [AID_1525D_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_1525D_NAME, AID_1525D_HASH, AID_1525D_SIZE, (string.Empty, 0)),
                [AID_384C1_NAME] = ("AID", BinaryStatusObject.ALL_DEVICES, AID_384C1_NAME, AID_384C1_HASH, AID_384C1_SIZE, (string.Empty, 0)),
                // CLESS CONFIG
                [CONTLEMV_ENGAGE] = ("CLESS", BinaryStatusObject.ENGAGE_DEVICES, CONTLEMV, CONTLEMV_HASH_ENGAGE, CONTLEMV_FILESIZE_ENGAGE, (CONTLEMV_HMACHASH_ENGAGE, CONTLEMV_HMACFILESIZE_ENGAGE)),
                [CONTLEMV_UX301] = ("CLESS", BinaryStatusObject.UX_DEVICES, CONTLEMV, CONTLEMV_HASH_UX301, CONTLEMV_FILESIZE_UX301, (CONTLEMV_HMACHASH_UX301, CONTLEMV_HMACFILESIZE_UX301)),
                // EMV CONFIG
                [ICCDATA_ENGAGE] = ("EMV", BinaryStatusObject.ENGAGE_DEVICES, ICCDATA, ICCDATA_HASH_ENGAGE, ICCDATA_FILESIZE_ENGAGE, (ICCDATA_HMACHASH_ENGAGE, ICCDATA_HMACFILESIZE_ENGAGE)),
                [ICCDATA_UX301] = ("EMV", BinaryStatusObject.UX_DEVICES, ICCDATA, ICCDATAHASH_UX301, ICCDATAFILESIZE_UX301, (ICCDATA_HMACHASH_UX301, ICCDATA_HMACFILESIZE_UX301)),
                [ICCKEYS_ENGAGE] = ("EMV", BinaryStatusObject.ENGAGE_DEVICES, ICCKEYS, ICCKEYS_HASH_ENGAGE, ICCKEYS_FILESIZE_ENGAGE, (ICCKEYS_HMACHASH_ENGAGE, ICCKEYS_HMACFILESIZE_ENGAGE)),
                [ICCKEYS_UX301] = ("EMV", BinaryStatusObject.UX_DEVICES, ICCKEYS, ICCKEYSHASH_UX301, ICCKEYSFILESIZE_UX301, (ICCKEYS_HMACHASH_UX301, ICCKEYS_HMACFILESIZE_UX301)),
                // GENERIC
                [CARDAPPCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, CARDAPPCFG, CARDAPPCFG_HASH, CARDAPPCFG_FILESIZE, (CARDAPPCFG_HMACHASH, CARDAPPCFG_HMACFILESIZE)),
                [CICAPPCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, CICAPPCFG, CICAPPCFG_HASH, CICAPPCFG_FILESIZE, (CICAPPCFG_HMACHASH, CICAPPCFG_HMACFILESIZE)),
                [MAPPCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, MAPPCFG, MAPPCFG_HASH, MAPPCFG_FILESIZE, (MAPPCFG_HMACHASH, MAPPCFG_HMACFILESIZE)),
                [TDOLCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, TDOLCFG, TDOLCFG_HASH, TDOLCFG_FILESIZE, (TDOLCFG_HMACHASH, TDOLCFG_HMACFILESIZE)),
                [TRMDOLCFG] = ("CFG", BinaryStatusObject.ALL_DEVICES, TRMDOLCFG, TRMDOLCFG_HASH, TRMDOLCFG_FILESIZE, (TRMDOLCFG_HMACHASH, TRMDOLCFG_HMACFILESIZE))
            };

        #endregion --- raw config files ---

        // PACKAGED CONFIGURATION
        #region --- packaged config files ---
        // ATTENDED TERMINAL
        public const string CONFIG_PKG_ENGAGE = "dl.VIPA_cfg_emv_att_prodcapk_sphere.tgz";
        public const string CONFIG_PKG_HASH_ENGAGE = "75f72274e6c40e623ebdf3605c6a31d1";
        public const int CONFIG_PKG_FILESIZE_ENGAGE = 0x7E16;

        // UNATTENDED TERMINAL
        public const string CONFIG_PKG_UX301 = "dl.VIPA_cfg_emv_unatt_prodcapk_sphere.tgz";
        public const string CONFIG_PKG_HASH_UX = "";
        public const int CONFIG_PKG_FILESIZE_UX = 0x00;

        public static Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)> configurationPackages =
             new Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)>()
             {
                 // CONFIGURATION PACKAGE
                 [CONTLEMV_ENGAGE] = ("EMV", BinaryStatusObject.ENGAGE_DEVICES, CONFIG_PKG_ENGAGE, CONFIG_PKG_HASH_ENGAGE, CONFIG_PKG_FILESIZE_ENGAGE),
                 [CONTLEMV_UX301] = ("EMV", BinaryStatusObject.UX_DEVICES, CONFIG_PKG_ENGAGE, CONFIG_PKG_HASH_UX, CONFIG_PKG_FILESIZE_UX),
             };

        #endregion --- packaged config files ---

        #region --- ADE SLOT CONFIGURATION ---

        // VIPA 6.8.2.11 CONFIGURATIONS
        public const string VIPA_BUNDLES_11 = "6.8.2.11";

        // --- SLOT LOCK SPHERE SIGNED BUNDLES ---
        // EPIC ADE SLOT-0 LOCK CONFIGURATIONS
        public const string SPHERE_EPIC_LOCK_CONFIG0_BUNDLE_11 = "sphere_VIPA_cfg_68211_Epic_slot0_210623.tgz";
        public const string SPHERE_EPIC_LOCK_CONFIG0_HASH_11 = "43260C4112D7938ECA1BB1E1909D9D90";
        public const int SPHERE_EPIC_LOCK_CONFIG0_SIZE_11 = 0x00007707;
        // EPIC ADE SLOT-8 LOCK CONFIGURATIONS
        public const string SPHERE_EPIC_LOCK_CONFIG8_BUNDLE_11 = "sphere_VIPA_cfg_68211_Epic_slot8_210623.tgz";
        public const string SPHERE_EPIC_LOCK_CONFIG8_HASH_11 = "9B824032F86DBC80147A9B80BA4E3D23";
        public const int SPHERE_EPIC_LOCK_CONFIG8_SIZE_11 = 0x0000770E;
        // NJT ADE SLOT-0 LOCK CONFIGURATIONS
        public const string SPHERE_NJT_LOCK_CONFIG0_BUNDLE_11 = "sphere_VIPA_cfg_68211_NJT_slot0_210623.tgz";
        public const string SPHERE_NJT_LOCK_CONFIG0_HASH_11 = "3FF143C9B46FD5F1D6D03AAC7B8FAA54";
        public const int SPHERE_NJT_LOCK_CONFIG0_SIZE_11 = 0x00007719;
        // NJT ADE SLOT-8 LOCK CONFIGURATIONS
        public const string SPHERE_NJT_LOCK_CONFIG8_BUNDLE_11 = "sphere_VIPA_cfg_68211_NJT_slot8_210623.tgz";
        public const string SPHERE_NJT_LOCK_CONFIG8_HASH_11 = "9423606FB43EA007DBB97EF5A0BABF04";
        public const int SPHERE_NJT_LOCK_CONFIG8_SIZE_11 = 0x00007711;

        // ---------- VERIFONE-DEV SIGNED
        // EPIC ADE SLOT-0 LOCK CONFIGURATIONS
        public const string VERIFONE_EPIC_LOCK_CONFIG0_BUNDLE_11 = "verifone_VIPA_cfg_68211_Epic_slot0_210519.tgz";
        public const string VERIFONE_EPIC_LOCK_CONFIG0_HASH_11 = "8F968901AA7ADC82D27F6F2C4152A5E6";
        public const int VERIFONE_EPIC_LOCK_CONFIG0_SIZE_11 = 0x0000752D;
        // EPIC ADE SLOT-8 LOCK CONFIGURATIONS
        public const string VERIFONE_EPIC_LOCK_CONFIG8_BUNDLE_11 = "verifone_VIPA_cfg_68211_Epic_slot8_210519.tgz";
        public const string VERIFONE_EPIC_LOCK_CONFIG8_HASH_11 = "0E9392A75908318B552F05234804DA9C";
        public const int VERIFONE_EPIC_LOCK_CONFIG8_SIZE_11 = 0x0000751D;
        // NJT ADE SLOT-0 LOCK CONFIGURATIONS
        public const string VERIFONE_NJT_LOCK_CONFIG0_BUNDLE_11 = "verifone_VIPA_cfg_68211_NJT_slot0_210519.tgz";
        public const string VERIFONE_NJT_LOCK_CONFIG0_HASH_11 = "4FECE8F62B02C46159EEEF5C4ECCB5E9";
        public const int VERIFONE_NJT_LOCK_CONFIG0_SIZE_11 = 0x0000752F;
        // NJT ADE SLOT-8 LOCK CONFIGURATIONS
        public const string VERIFONE_NJT_LOCK_CONFIG8_BUNDLE_11 = "verifone_VIPA_cfg_68211_NJT_slot8_210519.tgz";
        public const string VERIFONE_NJT_LOCK_CONFIG8_HASH_11 = "02C9FB6134F1962D2DC48E2C6B9CA261";
        public const int VERIFONE_NJT_LOCK_CONFIG8_SIZE_11 = 0x00007535;

        // VIPA 6.8.2.17 CONFIGURATIONS
        public const string VIPA_BUNDLES_17 = "6.8.2.17";

        // ---------- SPHERE SIGNED
        // EPIC ADE SLOT-0 LOCK CONFIGURATIONS
        public const string SPHERE_EPIC_LOCK_CONFIG0_BUNDLE_17 = "sphere_VIPA_cfg_68217_Epic_slot0_210623.tgz";
        public const string SPHERE_EPIC_LOCK_CONFIG0_HASH_17 = "AF9A2D63C7F2A4FAA6F7619FD74E37AD";
        public const int SPHERE_EPIC_LOCK_CONFIG0_SIZE_17 = 0x00007C24;
        // EPIC ADE SLOT-8 LOCK CONFIGURATIONS
        public const string SPHERE_EPIC_LOCK_CONFIG8_BUNDLE_17 = "sphere_VIPA_cfg_68217_Epic_slot8_210623.tgz";
        public const string SPHERE_EPIC_LOCK_CONFIG8_HASH_17 = "B4DED7C9214B44F7E1276EB58717ADB4";
        public const int SPHERE_EPIC_LOCK_CONFIG8_SIZE_17 = 0x00007C27;
        // NJT ADE SLOT-0 LOCK CONFIGURATIONS
        public const string SPHERE_NJT_LOCK_CONFIG0_BUNDLE_17 = "sphere_VIPA_cfg_68217_NJT_slot0_210623.tgz";
        public const string SPHERE_NJT_LOCK_CONFIG0_HASH_17 = "FD1727B54D9FFDC3490D433824ECEB2F";
        public const int SPHERE_NJT_LOCK_CONFIG0_SIZE_17 = 0x00007C28;
        // NJT ADE SLOT-8 LOCK CONFIGURATIONS
        public const string SPHERE_NJT_LOCK_CONFIG8_BUNDLE_17 = "sphere_VIPA_cfg_68217_NJT_slot8_210623.tgz";
        public const string SPHERE_NJT_LOCK_CONFIG8_HASH_17 = "2436D5815F99DA1CB5EC9A27BBC3E7D3";
        public const int SPHERE_NJT_LOCK_CONFIG8_SIZE_17 = 0x00007C26;

        // ---------- VERIFONE-DEV SIGNED
        // EPIC ADE SLOT-0 LOCK CONFIGURATIONS
        public const string VERIFONE_EPIC_LOCK_CONFIG0_BUNDLE_17 = "verifone_VIPA_cfg_68217_Epic_slot0_210519.tgz";
        public const string VERIFONE_EPIC_LOCK_CONFIG0_HASH_17 = "B386A5931832C32923CB5D6F5C524F70";
        public const int VERIFONE_EPIC_LOCK_CONFIG0_SIZE_17 = 0x00007A6F;
        // EPIC ADE SLOT-8 LOCK CONFIGURATIONS
        public const string VERIFONE_EPIC_LOCK_CONFIG8_BUNDLE_17 = "verifone_VIPA_cfg_68217_Epic_slot8_210519.tgz";
        public const string VERIFONE_EPIC_LOCK_CONFIG8_HASH_17 = "01A88EEC231CFF476B7E7B3716F7F1EA";
        public const int VERIFONE_EPIC_LOCK_CONFIG8_SIZE_17 = 0x000079AB;
        // NJT ADE SLOT-0 LOCK CONFIGURATIONS
        public const string VERIFONE_NJT_LOCK_CONFIG0_BUNDLE_17 = "verifone_VIPA_cfg_68217_NJT_slot0_210519.tgz";
        public const string VERIFONE_NJT_LOCK_CONFIG0_HASH_17 = "0CEAA2DEE759EA51EA621CA4498C2CB4";
        public const int VERIFONE_NJT_LOCK_CONFIG0_SIZE_17 = 0x00007A85;
        // NJT ADE SLOT-8 LOCK CONFIGURATIONS
        public const string VERIFONE_NJT_LOCK_CONFIG8_BUNDLE_17 = "verifone_VIPA_cfg_68217_NJT_slot8_210519.tgz";
        public const string VERIFONE_NJT_LOCK_CONFIG8_HASH_17 = "220BCFF54EC37434096837331925A4AB";
        public const int VERIFONE_NJT_LOCK_CONFIG8_SIZE_17 = 0x00007A74;

        public static Dictionary<string, (string configVersion, string[] deviceTypes, string fileName, string fileHash, int fileSize)> configBundlesSlot0 =
            new Dictionary<string, (string configVersion, string[] deviceTypes, string fileName, string fileHash, int fileSize)>()
        {
            // VIPA 6.8.2.11
            ["SPHERE_EPIC-11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, SPHERE_EPIC_LOCK_CONFIG0_BUNDLE_11, SPHERE_EPIC_LOCK_CONFIG0_HASH_11, SPHERE_EPIC_LOCK_CONFIG0_SIZE_11),
            ["SPHERE_NJT-11"]  = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, SPHERE_NJT_LOCK_CONFIG0_BUNDLE_11, SPHERE_NJT_LOCK_CONFIG0_HASH_11, SPHERE_NJT_LOCK_CONFIG0_SIZE_11),
            ["VERIFO_EPIC-11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, VERIFONE_EPIC_LOCK_CONFIG0_BUNDLE_11, VERIFONE_EPIC_LOCK_CONFIG0_HASH_11, VERIFONE_EPIC_LOCK_CONFIG0_SIZE_11),
            ["VERIFO_NJT-11"]  = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, VERIFONE_NJT_LOCK_CONFIG0_BUNDLE_11, VERIFONE_NJT_LOCK_CONFIG0_HASH_11, VERIFONE_NJT_LOCK_CONFIG0_SIZE_11),
            // VIPA 6.8.2.17
            ["SPHERE_EPIC-17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, SPHERE_EPIC_LOCK_CONFIG0_BUNDLE_17, SPHERE_EPIC_LOCK_CONFIG0_HASH_17, SPHERE_EPIC_LOCK_CONFIG0_SIZE_17),
            ["SPHERE_NJT-17"]  = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, SPHERE_NJT_LOCK_CONFIG0_BUNDLE_17, SPHERE_NJT_LOCK_CONFIG0_HASH_17, SPHERE_NJT_LOCK_CONFIG0_SIZE_17),
            ["VERIFO_EPIC-17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, VERIFONE_EPIC_LOCK_CONFIG0_BUNDLE_17, VERIFONE_EPIC_LOCK_CONFIG0_HASH_17, VERIFONE_EPIC_LOCK_CONFIG0_SIZE_17),
            ["VERIFO_NJT-17"]  = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, VERIFONE_NJT_LOCK_CONFIG0_BUNDLE_17, VERIFONE_NJT_LOCK_CONFIG0_HASH_17, VERIFONE_NJT_LOCK_CONFIG0_SIZE_17),
        };

        public static Dictionary<string, (string configVersion, string[] deviceTypes, string fileName, string fileHash, int fileSize)> configBundlesSlot8 =
            new Dictionary<string, (string configVersion, string[] deviceTypes, string fileName, string fileHash, int fileSize)>()
        {
            // VIPA 6.8.2.11
            ["SPHERE_EPIC-11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, SPHERE_EPIC_LOCK_CONFIG8_BUNDLE_11, SPHERE_EPIC_LOCK_CONFIG8_HASH_11, SPHERE_EPIC_LOCK_CONFIG8_SIZE_11),
            ["SPHERE_NJT-11"]  = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, SPHERE_NJT_LOCK_CONFIG8_BUNDLE_11, SPHERE_NJT_LOCK_CONFIG8_HASH_11, SPHERE_NJT_LOCK_CONFIG8_SIZE_11),
            ["VERIFO_EPIC-11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, VERIFONE_EPIC_LOCK_CONFIG8_BUNDLE_11, VERIFONE_EPIC_LOCK_CONFIG8_HASH_11, VERIFONE_EPIC_LOCK_CONFIG8_SIZE_11),
            ["VERIFO_NJT-11"]  = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, VERIFONE_NJT_LOCK_CONFIG8_BUNDLE_11, VERIFONE_NJT_LOCK_CONFIG8_HASH_11, VERIFONE_NJT_LOCK_CONFIG8_SIZE_11),
            // VIPA 6.8.2.17
            ["SPHERE_EPIC-17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, SPHERE_EPIC_LOCK_CONFIG8_BUNDLE_17, SPHERE_EPIC_LOCK_CONFIG8_HASH_17, SPHERE_EPIC_LOCK_CONFIG8_SIZE_17),
            ["SPHERE_NJT-17"]  = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, SPHERE_NJT_LOCK_CONFIG8_BUNDLE_17, SPHERE_NJT_LOCK_CONFIG8_HASH_17, SPHERE_NJT_LOCK_CONFIG8_SIZE_17),
            ["VERIFO_EPIC-17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, VERIFONE_EPIC_LOCK_CONFIG8_BUNDLE_17, VERIFONE_EPIC_LOCK_CONFIG8_HASH_17, VERIFONE_EPIC_LOCK_CONFIG8_SIZE_17),
            ["VERIFO_NJT-17"]  = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, VERIFONE_NJT_LOCK_CONFIG8_BUNDLE_17, VERIFONE_NJT_LOCK_CONFIG8_HASH_17, VERIFONE_NJT_LOCK_CONFIG8_SIZE_17),
        };
        #endregion --- ADE SLOT CONFIGURATION ---

        #region --- emv configuration packages ---
        // ATTENDED TERMINAL --------------------------------------------------------------------------------------------------------------
        public const string ATTENDED_VIPA_BUNDLES_11 = "6.8.2.11";
        // VIPA 6.8.2.11 CONFIGURATIONS
        // ---------- SPHERE SIGNED
        public const string SPHERE_ATTENDED_EMV_CONFIG_PKG_11 = "sphere_VIPA6.8.2.11_emv_FD_attended_210611.tgz";
        public const string SPHERE_ATTENDED_EMV_CONFIG_PKG_HASH_11 = "14B46E6CD6371F033CA833597A24575A";
        public const int SPHERE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_11 = 0x0000FEA1;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_ATTENDED_EMV_CONFIG_PKG_11 = "verifone_VIPA6.8.2.11_emv_FD_attended_210610.tgz";
        public const string VERIFONE_ATTENDED_EMV_CONFIG_PKG_HASH_11 = "C149BF8F50065EF299B70FFC7C416FD4";
        public const int VERIFONE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_11 = 0x000100EB;

        // VIPA 6.8.2.17 CONFIGURATIONS
        public const string ATTENDED_VIPA_BUNDLES_17 = "6.8.2.17";
        // ---------- SPHERE SIGNED
        public const string SPHERE_ATTENDED_EMV_CONFIG_PKG_17 = "sphere_VIPA6.8.2.17_emv_FD_attended_210611.tgz";
        public const string SPHERE_ATTENDED_EMV_CONFIG_PKG_HASH_17 = "0CBA2916E96C42C33DA46A4BF88F1350";
        public const int SPHERE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_17 = 0x0000FEEF;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_ATTENDED_EMV_CONFIG_PKG_17 = "verifone_VIPA6.8.2.17_emv_FD_attended_210610.tgz";
        public const string VERIFONE_ATTENDED_EMV_CONFIG_PKG_HASH_17 = "9058AEE6244EE5018180E56B20F1A3A9";
        public const int VERIFONE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_17 = 0x000100D8;

        // UNATTENDED TERMINAL ------------------------------------------------------------------------------------------------------------
        // VIPA 6.8.2.11 CONFIGURATIONS
        public const string UNATTENDED_VIPA_BUNDLES_11 = "6.8.2.11";
        // ---------- SPHERE SIGNED
        public const string SPHERE_UNATTENDED_EMV_CONFIG_PKG_11 = "sphere_VIPA6.8.2.11_emv_FD_unattended_210623.tgz";
        public const string SPHERE_UNATTENDED_EMV_CONFIG_PKG_HASH_11 = "4AA0976F4FAEBA5CCEF677C08B68F22B";
        public const int SPHERE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_11 = 0x0001004C;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_UNATTENDED_EMV_CONFIG_PKG_11 = "verifone_VIPA6.8.2.11_emv_FD_unattended_210519.tgz";
        public const string VERIFONE_UNATTENDED_EMV_CONFIG_PKG_HASH_11 = "BD04C7D9866C64FD7AA5D712FBF988F6";
        public const int VERIFONE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_11 = 0x0000FFA4;

        // VIPA 6.8.2.17 CONFIGURATIONS
        public const string UNATTENDED_VIPA_BUNDLES_17 = "6.8.2.17";
        // ---------- SPHERE SIGNED
        public const string SPHERE_UNATTENDED_EMV_CONFIG_PKG_17 = "sphere_VIPA6.8.2.17_emv_FD_unattended_210623.tgz";
        public const string SPHERE_UNATTENDED_EMV_CONFIG_PKG_HASH_17 = "7923E1D8ED1D12E40C1924B86FE7784C";
        public const int SPHERE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17 = 0x0000FFE9;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_UNATTENDED_VIPA_BUNDLES_17 = "6.8.2.17";
        public const string VERIFONE_UNATTENDED_EMV_CONFIG_PKG_17 = "verifone_VIPA6.8.2.17_emv_FD_unattended_210519.tgz";
        public const string VERIFONE_UNATTENDED_EMV_CONFIG_PKG_HASH_17 = "08299B59CA865D6D2AFB274FA1642CD0";
        public const int VERIFONE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17 = 0x0000FEDA;

        // EMV CONFIGURATION PACKAGE
        public static Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)> emvConfigurationPackages =
             new Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)>()
        {
            // VIPA 6.8.2.11
            ["SPHERE_ATT-11"] = (ATTENDED_VIPA_BUNDLES_11, BinaryStatusObject.ENGAGE_DEVICES, SPHERE_ATTENDED_EMV_CONFIG_PKG_11, SPHERE_ATTENDED_EMV_CONFIG_PKG_HASH_11, SPHERE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_11),
            ["SPHERE_UNA-11"] = (UNATTENDED_VIPA_BUNDLES_11, BinaryStatusObject.UX_DEVICES, SPHERE_UNATTENDED_EMV_CONFIG_PKG_11, SPHERE_UNATTENDED_EMV_CONFIG_PKG_HASH_11, SPHERE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
            ["VERIFO_ATT-11"] = (ATTENDED_VIPA_BUNDLES_11, BinaryStatusObject.ENGAGE_DEVICES, VERIFONE_ATTENDED_EMV_CONFIG_PKG_11, VERIFONE_ATTENDED_EMV_CONFIG_PKG_HASH_11, VERIFONE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_11),
            ["VERIFO_UNA-11"] = (UNATTENDED_VIPA_BUNDLES_11, BinaryStatusObject.UX_DEVICES, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_11, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_HASH_11, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
            // VIPA 6.8.2.17
            ["SPHERE_ATT-17"] = (ATTENDED_VIPA_BUNDLES_17, BinaryStatusObject.ENGAGE_DEVICES, SPHERE_ATTENDED_EMV_CONFIG_PKG_17, SPHERE_ATTENDED_EMV_CONFIG_PKG_HASH_17, SPHERE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
            ["SPHERE_UNA-17"] = (UNATTENDED_VIPA_BUNDLES_17, BinaryStatusObject.UX_DEVICES, SPHERE_UNATTENDED_EMV_CONFIG_PKG_17, SPHERE_UNATTENDED_EMV_CONFIG_PKG_HASH_17, SPHERE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
            ["VERIFO_ATT-17"] = (ATTENDED_VIPA_BUNDLES_17, BinaryStatusObject.ENGAGE_DEVICES, VERIFONE_ATTENDED_EMV_CONFIG_PKG_17, VERIFONE_ATTENDED_EMV_CONFIG_PKG_HASH_17, VERIFONE_ATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
            ["VERIFO_UNA-17"] = (UNATTENDED_VIPA_BUNDLES_17, BinaryStatusObject.UX_DEVICES, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_17, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_HASH_17, VERIFONE_UNATTENDED_EMV_CONFIG_PKG_FILESIZE_17),
        };

        #endregion --- emv configuration packages ---

        #region --- IDLE SCREEN PACKAGE ---
        /*public const string IDLE_MSG = "idle.msg";
        public const string IDLE_MSG_HASH = "22C6918ACCDB2DF73AE95F57DBEB6BC8";
        public const int IDLE_MSG_SIZE = 0x0000001D;

        public const string RAPTOR_IMAGE = "SphereIdle.png";
        public const string M400_IMAGE = "SphereIdleM400.png";
        public const string M400_IMAGE_HASH = "67C675266D01D2039F17F7F921903CC1";
        public const int M400_IMAGE_SIZE = 0x00083148;
        public const string P200_IMAGE = "SphereIdleP200.png";
        public const string P200_IMAGE_HASH = "D8BEC176F1CB634FECA4F94420B412B9";
        public const int P200_IMAGE_SIZE = 0x00033503;
        public const string P400_IMAGE = "SphereIdleP400.png";
        public const string P400_IMAGE_HASH = "08784D82B6587D18E023D77191FA7094";
        public const int P400_IMAGE_SIZE = 0x00016C87;

        public static Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)> RaptorIdleScreen =
            new Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)>()
        {
            [IDLE_MSG] = (BinaryStatusObject.ENGAGE_DEVICES, IDLE_MSG, IDLE_MSG, IDLE_MSG_HASH, IDLE_MSG_SIZE),
            [M400_IMAGE] = (new string[] { DEVICE_M400 }, M400_IMAGE, RAPTOR_IMAGE, M400_IMAGE_HASH, M400_IMAGE_SIZE),
            [P200_IMAGE] = (new string[] { DEVICE_P200 }, P200_IMAGE, RAPTOR_IMAGE, P200_IMAGE_HASH, P200_IMAGE_SIZE),
            [P400_IMAGE] = (new string[] { DEVICE_P400 }, P400_IMAGE, RAPTOR_IMAGE, P400_IMAGE_HASH, P400_IMAGE_SIZE)
        };*/

        // ---------- M400 SPHERE SIGNED
        public const string SPHERE_M400_IMAGE_TGZ_199 = "sphere_idlescreen_sphere_m400_199_210623.tgz";
        public const string SPHERE_M400_IMAGE_TGZ_199_HASH = "E26943FD86EF633D230B1ED807857231";
        public const int SPHERE_M400_IMAGE_TGZ_199_SIZE = 0x00084490;
        public const string SPHERE_M400_IMAGE_TGZ_250 = "sphere_idlescreen_sphere_m400_250_210623.tgz";
        public const string SPHERE_M400_IMAGE_TGZ_250_HASH = "9E8893E841529FAF0540E4386955A1D3";
        public const int SPHERE_M400_IMAGE_TGZ_250_SIZE = 0x0003851E;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_M400_IMAGE_TGZ_199 = "verifone_idlescreen_sphere_m400_199_210623.tgz";
        public const string VERIFONE_M400_IMAGE_TGZ_199_HASH = "724CE9548770966AB8D5DD387AA38776";
        public const int VERIFONE_M400_IMAGE_TGZ_199_SIZE = 0x000844D0;
        public const string VERIFONE_M400_IMAGE_TGZ_250 = "verifone_idlescreen_sphere_m400_250_210623.tgz";
        public const string VERIFONE_M400_IMAGE_TGZ_250_HASH = "5E962F7DE578C6766DB8C77D7E20BA72";
        public const int VERIFONE_M400_IMAGE_TGZ_250_SIZE = 0x00038562;

        // ---------- P200 SPHERE SIGNED
        public const string SPHERE_P200_IMAGE_TGZ_199 = "sphere_idlescreen_sphere_p200_199_210623.tgz";
        public const string SPHERE_P200_IMAGE_TGZ_199_HASH = "969B6296B022AF46781CE369C162BECA";
        public const int SPHERE_P200_IMAGE_TGZ_199_SIZE = 0x000347B0;
        public const string SPHERE_P200_IMAGE_TGZ_250 = "sphere_idlescreen_sphere_p200_250_210623.tgz";
        public const string SPHERE_P200_IMAGE_TGZ_250_HASH = "47C04B14820D25F8C4D0FC899FCE4671";
        public const int SPHERE_P200_IMAGE_TGZ_250_SIZE = 0x0000E292;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_P200_IMAGE_TGZ_199 = "verifone_idlescreen_sphere_p200_199_210623.tgz";
        public const string VERIFONE_P200_IMAGE_TGZ_199_HASH = "EC58191C8C2559DACA56EAB07C94C207";
        public const int VERIFONE_P200_IMAGE_TGZ_199_SIZE = 0x000347E4;
        public const string VERIFONE_P200_IMAGE_TGZ_250 = "verifone_idlescreen_sphere_p200_250_210623.tgz";
        public const string VERIFONE_P200_IMAGE_TGZ_250_HASH = "2BF3BCC65532415C21950FF6C33D12EA";
        public const int VERIFONE_P200_IMAGE_TGZ_250_SIZE = 0x0000E2C6;

        // ---------- P400 SPHERE SIGNED
        public const string SPHERE_P400_IMAGE_TGZ_199 = "sphere_idlescreen_sphere_p400_199_210623.tgz";
        public const string SPHERE_P400_IMAGE_TGZ_199_HASH = "2637CB166E2ABED3BE5EF25E79DC6B07";
        public const int SPHERE_P400_IMAGE_TGZ_199_SIZE = 0x00017C6D;
        public const string SPHERE_P400_IMAGE_TGZ_250 = "sphere_idlescreen_sphere_p400_250_210623.tgz";
        public const string SPHERE_P400_IMAGE_TGZ_250_HASH = "AC2A72CD83328D3A48EBCFA9B06297E2";
        public const int SPHERE_P400_IMAGE_TGZ_250_SIZE = 0x0003CEB4;
        // ---------- VERIFONE-DEV SIGNED
        public const string VERIFONE_P400_IMAGE_TGZ_199 = "verifone_idlescreen_sphere_p400_199_210623.tgz";
        public const string VERIFONE_P400_IMAGE_TGZ_199_HASH = "5C00EB2144EF171BDD663751092F0FA1";
        public const int VERIFONE_P400_IMAGE_TGZ_199_SIZE = 0x00017C9C;
        public const string VERIFONE_P400_IMAGE_TGZ_250 = "verifone_idlescreen_sphere_p400_250_210623.tgz";
        public const string VERIFONE_P400_IMAGE_TGZ_250_HASH = "08BC71F3D21D1AE032FD5715ED5C07E8";
        public const int VERIFONE_P400_IMAGE_TGZ_250_SIZE = 0x0003CEF9;

        public static Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)> RaptorIdleScreenTGZ_199 =
            new Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)>()
        {
            // M400
            ["SPHERE_M400"] = (new string[] { DEVICE_M400 }, SPHERE_M400_IMAGE_TGZ_199, SPHERE_M400_IMAGE_TGZ_199, SPHERE_M400_IMAGE_TGZ_199_HASH, SPHERE_M400_IMAGE_TGZ_199_SIZE),
            ["VERIFONE_M400"] = (new string[] { DEVICE_M400 }, VERIFONE_M400_IMAGE_TGZ_199, VERIFONE_M400_IMAGE_TGZ_199, VERIFONE_M400_IMAGE_TGZ_199_HASH, VERIFONE_M400_IMAGE_TGZ_199_SIZE),
            // P200
            ["SPHERE_P200"] = (new string[] { DEVICE_P200 }, SPHERE_P200_IMAGE_TGZ_199, SPHERE_P200_IMAGE_TGZ_199, SPHERE_P200_IMAGE_TGZ_199_HASH, SPHERE_P200_IMAGE_TGZ_199_SIZE),
            ["VERIFONE_P200"] = (new string[] { DEVICE_P200 }, VERIFONE_P200_IMAGE_TGZ_199, VERIFONE_P200_IMAGE_TGZ_199, VERIFONE_P200_IMAGE_TGZ_199_HASH, VERIFONE_P200_IMAGE_TGZ_199_SIZE),
            // P400
            ["SPHERE_P400"] = (new string[] { DEVICE_P400 }, SPHERE_P400_IMAGE_TGZ_199, SPHERE_P400_IMAGE_TGZ_199, SPHERE_P400_IMAGE_TGZ_199_HASH, SPHERE_P400_IMAGE_TGZ_199_SIZE),
            ["VERIFONE_P400"] = (new string[] { DEVICE_P400 }, VERIFONE_P400_IMAGE_TGZ_199, VERIFONE_P400_IMAGE_TGZ_199, VERIFONE_P400_IMAGE_TGZ_199_HASH, VERIFONE_P400_IMAGE_TGZ_199_SIZE),
        };

        public static Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)> RaptorIdleScreenTGZ_250 =
            new Dictionary<string, (string[] deviceTypes, string fileName, string fileTargetName, string fileHash, int fileSize)>()
        {
            // M400
            ["SPHERE_M400"] = (new string[] { DEVICE_M400 }, SPHERE_M400_IMAGE_TGZ_250, SPHERE_M400_IMAGE_TGZ_250, SPHERE_M400_IMAGE_TGZ_250_HASH, SPHERE_M400_IMAGE_TGZ_250_SIZE),
            ["VERIFONE_M400"] = (new string[] { DEVICE_M400 }, VERIFONE_M400_IMAGE_TGZ_250, VERIFONE_M400_IMAGE_TGZ_250, VERIFONE_M400_IMAGE_TGZ_250_HASH, VERIFONE_M400_IMAGE_TGZ_250_SIZE),
            // P200
            ["SPHERE_P200"] = (new string[] { DEVICE_P200 }, SPHERE_P200_IMAGE_TGZ_250, SPHERE_P200_IMAGE_TGZ_250, SPHERE_P200_IMAGE_TGZ_250_HASH, SPHERE_P200_IMAGE_TGZ_250_SIZE),
            ["VERIFONE_P200"] = (new string[] { DEVICE_P200 }, VERIFONE_P200_IMAGE_TGZ_250, VERIFONE_P200_IMAGE_TGZ_250, VERIFONE_P200_IMAGE_TGZ_250_HASH, VERIFONE_P200_IMAGE_TGZ_250_SIZE),
            // P400
            ["SPHERE_P400"] = (new string[] { DEVICE_P400 }, SPHERE_P400_IMAGE_TGZ_250, SPHERE_P400_IMAGE_TGZ_250, SPHERE_P400_IMAGE_TGZ_250_HASH, SPHERE_P400_IMAGE_TGZ_250_SIZE),
            ["VERIFONE_P400"] = (new string[] { DEVICE_P400 }, VERIFONE_P400_IMAGE_TGZ_250, VERIFONE_P400_IMAGE_TGZ_250, VERIFONE_P400_IMAGE_TGZ_250_HASH, VERIFONE_P400_IMAGE_TGZ_250_SIZE),
        };

        #endregion --- IDLE SCREEN PACKAGE ---

        #region --- BASE_BUNDLE packages ---
        // VIPA bundle signatures
        public const string VIPA_VER_FW = "www/vipa_ver.txt";
        public const string VIPA_VER_FW_HASH_11 = "7FCB0038F795ED9930F7536832BD3DD4";
        public const int VIPA_VER_FW_FILESIZE_11 = 0x00000018;
        public const string VIPA_VER_FW_HASH_17 = "7FCB0038F795ED9930F7536832BD3DD4";
        public const int VIPA_VER_FW_FILESIZE_17 = 0x00000018;

        public const string VIPA_VER_EMV = "emv_ver.txt";
        public const string VIPA_VER_EMV_HASH_EPIC_0_11 = "8D73517F3D7505298573E2C825F46649";
        public const int VIPA_VER_EMV_FILESIZE_EPIC_0_11 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_EPIC_8_11 = "8D73517F3D7505298573E2C825F46649";
        public const int VIPA_VER_EMV_FILESIZE_EPIC_8_11 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_NJT_0_11 = "64491F3FA90BDDF0313A84E940BC193C";
        public const int VIPA_VER_EMV_FILESIZE_NJT_0_11 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_NJT_8_11 = "64491F3FA90BDDF0313A84E940BC193C";
        public const int VIPA_VER_EMV_FILESIZE_NJT_8_11 = 0x0000001B;

        public const string VIPA_VER_EMV_HASH_EPIC_0_17 = "BC68563F6128976CB885EEC6689E07B0";
        public const int VIPA_VER_EMV_FILESIZE_EPIC_0_17 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_EPIC_8_17 = "8D73517F3D7505298573E2C825F46649";
        public const int VIPA_VER_EMV_FILESIZE_EPIC_8_17 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_NJT_0_17 = "64491F3FA90BDDF0313A84E940BC193C";
        public const int VIPA_VER_EMV_FILESIZE_NJT_0_17 = 0x0000001B;
        public const string VIPA_VER_EMV_HASH_NJT_8_17 = "64491F3FA90BDDF0313A84E940BC193C";
        public const int VIPA_VER_EMV_FILESIZE_NJT_8_17 = 0x0000001B;

        public const string VIPA_VER_IDLE = "idle_ver.txt";
        public const string VIPA_VER_IDLE_HASH_M400_17 = "2D02DA6A938D12BDA602AEDF929D0EB1";
        public const int VIPA_VER_IDLE_FILESIZE_M400_17 = 0x00000014;
        public const string VIPA_VER_IDLE_HASH_P200_17 = "2D02DA6A938D12BDA602AEDF929D0EB1";
        public const int VIPA_VER_IDLE_FILESIZE_P200_17 = 0x00000014;
        public const string VIPA_VER_IDLE_HASH_P400_17 = "7FCB0038F795ED9930F7536832BD3DD4";
        public const int VIPA_VER_IDLE_FILESIZE_P400_17 = 0x00000014;

        public const string VIPA_BUNDLES_19 = "6.8.2.19";
        public const string VIPA_VER_FW_HASH_19 = "TODO";
        public const int VIPA_VER_FW_FILESIZE_19 = 0x00000000;
        public const string VIPA_VER_EMV_HASH_EPIC_0_19 = "TODO";
        public const int VIPA_VER_EMV_FILESIZE_EPIC_0_19 = 0x00000000;
        public const string VIPA_VER_IDLE_HASH_19 = "TODO";
        public const int VIPA_VER_IDLE_FILESIZE_19 = 0x00000000;

        public static Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)> vipaVersions =
            new Dictionary<string, (string configType, string[] deviceTypes, string fileName, string fileHash, int fileSize)>()
        {
            // VIPA 6.8.2.11
            ["SPHERE_BASE_11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ALL_DEVICES, VIPA_VER_FW, VIPA_VER_FW_HASH_11, VIPA_VER_FW_FILESIZE_11),
            ["SPHERE_AES0_11"] = (VIPA_BUNDLES_11, BinaryStatusObject.ENGAGE_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_11, VIPA_VER_EMV_FILESIZE_EPIC_0_11),
            ["SPHERE_UES0_11"] = (VIPA_BUNDLES_11, BinaryStatusObject.UX_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_11, VIPA_VER_EMV_FILESIZE_EPIC_0_11),
            ["SPHERE_ILM4_11"] = (VIPA_BUNDLES_11, new string[] { BinaryStatusObject.DEVICE_M400 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_M400_17, VIPA_VER_IDLE_FILESIZE_M400_17),
            ["SPHERE_ILP2_11"] = (VIPA_BUNDLES_11, new string[] { BinaryStatusObject.DEVICE_P200 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_P200_17, VIPA_VER_IDLE_FILESIZE_P200_17),
            ["SPHERE_ILP4_11"] = (VIPA_BUNDLES_11, new string[] { BinaryStatusObject.DEVICE_P400 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_P400_17, VIPA_VER_IDLE_FILESIZE_P400_17),
            // VIPA 6.8.2.17
            ["SPHERE_BASE_17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ALL_DEVICES, VIPA_VER_FW, VIPA_VER_FW_HASH_17, VIPA_VER_FW_FILESIZE_17),
            ["SPHERE_AES0_17"] = (VIPA_BUNDLES_17, BinaryStatusObject.ENGAGE_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_17, VIPA_VER_EMV_FILESIZE_EPIC_0_17),
            ["SPHERE_UES0_17"] = (VIPA_BUNDLES_17, BinaryStatusObject.UX_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_17, VIPA_VER_EMV_FILESIZE_EPIC_0_17),
            ["SPHERE_ILM4_17"] = (VIPA_BUNDLES_17, new string[] { BinaryStatusObject.DEVICE_M400 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_M400_17, VIPA_VER_IDLE_FILESIZE_M400_17),
            ["SPHERE_ILP2_17"] = (VIPA_BUNDLES_17, new string[] { BinaryStatusObject.DEVICE_P200 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_P200_17, VIPA_VER_IDLE_FILESIZE_P200_17),
            ["SPHERE_ILP4_17"] = (VIPA_BUNDLES_17, new string[] { BinaryStatusObject.DEVICE_P400 }, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_P400_17, VIPA_VER_IDLE_FILESIZE_P400_17),
            // VIPA 6.8.2.19
            ["SPHERE_BASE_19"] = (VIPA_BUNDLES_19, BinaryStatusObject.ALL_DEVICES, VIPA_VER_FW, VIPA_VER_FW_HASH_19, VIPA_VER_FW_FILESIZE_19),
            ["SPHERE_AES0_19"] = (VIPA_BUNDLES_19, BinaryStatusObject.ENGAGE_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_19, VIPA_VER_EMV_FILESIZE_EPIC_0_19),
            ["SPHERE_UES0_19"] = (VIPA_BUNDLES_19, BinaryStatusObject.UX_DEVICES, VIPA_VER_EMV, VIPA_VER_EMV_HASH_EPIC_0_19, VIPA_VER_EMV_FILESIZE_EPIC_0_19),
            ["SPHERE_IDLE_19"] = (VIPA_BUNDLES_19, BinaryStatusObject.ENGAGE_DEVICES, VIPA_VER_IDLE, VIPA_VER_IDLE_HASH_19, VIPA_VER_IDLE_FILESIZE_19),
        };

        #endregion --- BASE_BUNDLE packages ---

        public const string MAPP_SRED_CONFIG = "mapp_vsd_sred.cfg";

        public bool FileNotFound { get; set; }
        public int FileSize { get; set; }
        public string FileCheckSum { get; set; }
        public int SecurityStatus { get; set; }
        public byte[] ReadResponseBytes { get; set; }
    }
}
