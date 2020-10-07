using HMACHasher.HasherVersion2;
using System;
using System.Diagnostics;
using System.Text;

namespace HMACHasher
{
    /// <summary>
    /// 
    /// HOW TO FILL IN VALUES FOR HMACValidator
    ///
    /// string PANSecret            : "4111111111111111" EXPANDED TO EBCDIC => "34313131313131313131313131313131"
    /// string expectedVSSKey06     : TC_4111_GENERATE_HMAC_ASCII.py "Generated HMAC HOSTID-06"
    /// string expectedVSSKey06     : TC_4111_GENERATE_HMAC_ASCII.py "Generated HMAC HOSTID-07"
    /// 
    /// byte[] MACSecondaryKeyHASH  : expectedVSSKey07 to byte[]
    /// byte[] MACPrimaryPANSalt    : Hasher.HMACHasher.EncryptHMAC(PANSecret, HMACValidator.MACSecondaryKeyHASH)
    /// 
    /// byte[] MACPrimaryHASHSalt   : Hasher.HMACHasher.EncryptHMAC(expectedVSSKey06, HMACValidator.MACSecondaryKeyHASH)
    /// byte[] MACPrimaryKeyHASH    :
    /// 
    /// byte[] MACSecondaryHASHSalt : Hasher.HMACHasher..EncryptHMAC(expectedVSSKey07, HMACValidator.MACPrimaryKeyHASH) 
    ///
    /// USAGE:
    /// 
    /// STEP1  : VSS-6 => HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryPANSalt), HMACValidator.MACSecondaryKeyHASH)
    /// HMAC   : 4111111111111111
    /// RESULT : C464084095AE8D1F16B57602724955651D45B4B6083E4A5E41C4837081F460A6
    /// COMPARE: HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH)
    /// 
    /// STEP2  : VSS-7 => HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH)
    /// HMAC   : C464084095AE8D1F16B57602724955651D45B4B6083E4A5E41C4837081F460A6
    /// RESULT : EDA100E8F35DCE4BD9FDA2EF7456A1E403E09FEB2A95FB3D97F88784B548BF4D
    /// COMPARE: HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACSecondaryHASHSalt), HMACValidator.MACPrimaryKeyHASH)
    /// 
    /// </summary>
    public static class HasherVersion2Impl
    {
        // OBTAIN THE FOLLOWING FROM PYTHON SCRIPT: TC_4111_GENERATE_HMAC_ASCII.py
        // "Generated HMAC HOSTID-06:"
        static string expectedVSSKey06 = "C464084095AE8D1F16B57602724955651D45B4B6083E4A5E41C4837081F460A6";
        static byte[] vssKey06Hash = new byte[] { 0xC4, 0x64, 0x08, 0x40, 0x95, 0xAE, 0x8D, 0x1F, 0x16, 0xB5, 0x76, 0x02, 0x72, 0x49, 0x55, 0x65, 0x1D, 0x45, 0xB4, 0xB6, 0x08, 0x3E, 0x4A, 0x5E, 0x41, 0xC4, 0x83, 0x70, 0x81, 0xF4, 0x60, 0xA6 };

        // "Generated HMAC HOSTID-07:"
        static string expectedVSSKey07 = "EDA100E8F35DCE4BD9FDA2EF7456A1E403E09FEB2A95FB3D97F88784B548BF4D";
        static byte[] vssKey07Hash = new byte[] { 0xED, 0xA1, 0x00, 0xE8, 0xF3, 0x5D, 0xCE, 0x4B, 0xD9, 0xFD, 0xA2, 0xEF, 0x74, 0x56, 0xA1, 0xE4, 0x03, 0xE0, 0x9F, 0xEB, 0x2A, 0x95, 0xFB, 0x3D, 0x97, 0xF8, 0x87, 0x84, 0xB5, 0x48, 0xBF, 0x4D };

        // VSS-6-HMAC      : C464084095AE8D1F16B57602724955651D45B4B6083E4A5E41C4837081F460A6
        // MACPRIMARYHASH  : 43343634303834303935414538443146313642353736303237323439353536353144343542344236303833453441354534314334383337303831463436304136
        // HASH-PRODUCED   : 1E19BFDF2D8EC6BCC74FBE047678F13D

        // VSS-7-HMAC      : EDA100E8F35DCE4BD9FDA2EF7456A1E403E09FEB2A95FB3D97F88784B548BF4D
        // MACSECONDARYHASH: 45444131303045384633354443453442443946444132454637343536413145343033453039464542324139354642334439374638383738344235343842463444
        // HASH-PRODUCED   : B7F2BC7070B46E63842E787272C6E95C

        // PAN
        //static string PANShortSecret = "4111111111111111";
        static string PANSecret = "34313131313131313131313131313131";
        //static byte[] PANGeneration = new byte[] { 0x41, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 };

        public static void TestHasher()
        {
            // THIS IS THE EXPECTED HOSTID-07 KEY
            string actualVSSKey07 = BitConverter.ToString(HMACValidator.MACSecondaryKeyHASH).Replace("-", "");
            Debug.WriteLine($"SECONDARY HASH=[{actualVSSKey07}]");
            bool isVSSKey07Match = expectedVSSKey07.Equals(actualVSSKey07);
            Debug.WriteLine($"SECONDARY HASH IS A MATCH? {isVSSKey07Match}");

            // *** HOW TO CONSTRUCT MACPrimaryPANSalt ***
            // how to create MACPrimaryPANSalt ???
            // USAGE: decrypts HostId-Key-06
            Generate_MACPrimaryPANSalt();

            // TEST MACPrimaryPANSalt
            string decriptedPrimaryPANSalt = HasherVersion2.HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryPANSalt), HMACValidator.MACSecondaryKeyHASH);
            Debug.WriteLine($"PAN DECRYPTED HASH={decriptedPrimaryPANSalt}");
            Console.WriteLine($"PAN DECRYPTED HASH__={decriptedPrimaryPANSalt}");
            bool isPANMatch = PANSecret.Equals(decriptedPrimaryPANSalt);
            Debug.WriteLine($"SECONDARY HASH IS A MATCH? {isPANMatch}");

            // *** HOW TO CONSTRUCT MACPrimaryHASHSalt ***
            // USAGE   : decrypts HostId-Key-06
            // REQUIRES: expectedVSSKey06, HMACValidator.MACSecondaryKeyHASH
            Generate_MACPrimaryHASHSalt();

            // KEY HOST-ID 06 VALIDATOR
            string decriptedVSS06Hash = HasherVersion2.HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH);
            Debug.WriteLine($"DECRYPTED VSS-6 HASH={decriptedVSS06Hash}");
            Console.WriteLine($"DECRYPTED VSS-6 HASH={decriptedVSS06Hash}");
            bool isVSS06HMACMatch = expectedVSSKey06.Equals(decriptedVSS06Hash);
            Debug.WriteLine($"SECONDARY HASH IS A MATCH? {isVSS06HMACMatch}");

            // *** HOW TO CONSTRUCT MACPrimaryKeyHASH ***
            // USAGE   : 
            // REQUIRES: 
            //Generate_MACPrimaryKeyHASH();

            // *** HOW TO CONSTRUCT MACSecondaryHASHSalt ***
            // USAGE   : decrypts HostId-Key-06
            // REQUIRES: expectedVSSKey06, HMACValidator.MACPrimaryKeyHASH
            Generate_MACSecondaryHASHSalt();

            // KEY HOST-ID 07 VALIDATOR
            string decriptedVSS07Hash = HasherVersion2.HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACSecondaryHASHSalt), HMACValidator.MACPrimaryKeyHASH);
            Debug.WriteLine($"DECRYPTED VSS-7 HASH={decriptedVSS07Hash}");
            Console.WriteLine($"DECRYPTED VSS-7 HASH={decriptedVSS07Hash}");
            bool isVSS07HMACMatch = expectedVSSKey07.Equals(decriptedVSS07Hash);
            Debug.WriteLine($"SECONDARY HASH IS A MATCH? {isVSS07HMACMatch}");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: PAN SECRET
        ///          34313131313131313131313131313131
        /// 2) KEY : MAC SECONDARY KEY HASH
        ///          44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833
        ///          HASH: 9A40969CEA07981C36906CDB56108C1D
        /// OUTPUT : 7658386C36534F6D53766D705752693641446E5373716C5A474C6F414F644B7971566B597567413530724B414E774E41354D2B6E78413D3D          
        /// </summary>
        static void Generate_MACPrimaryPANSalt()
        {
            // *** HOW TO CONSTRUCT MACPrimaryPANSalt ***
            string encryptedHashText = HasherVersion2.HMACHasher.EncryptHMAC(PANSecret, HMACValidator.MACSecondaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            //Debug.WriteLine($"ENCRYPTED HASH={encryptedHashText}");
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryPANSalt = new byte[] { };
            Debug.WriteLine($"ENCRYPTED HASH=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-06
        ///          C464084095AE8D1F16B57602724955651D45B4B6083E4A5E41C4837081F460A6
        /// 2) KEY : MAC SECONDARY KEY HASH
        ///          44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833
        ///          HASH: 9A40969CEA07981C36906CDB56108C1D
        /// OUTPUT : 5A2B50664D4C5576366476464F7772314D753650637658306845676C5771386E6E57745A58332B6C45795938775837677A6C4956516953765865765766564B786C6A45494E495242302F4379695150624D556230365941334130446B7A366645         
        /// </summary>
        static void Generate_MACPrimaryHASHSalt()
        {
            // ENCRYPT: expectedVSSKey06 with MACSecondaryKeyHASH == MACPrimaryHASHSalt
            // DECRYPT: MACPrimaryPANSalt with MACSecondaryKeyHASH == HostIdKey-06

            // *** HOW TO CONSTRUCT MACPrimaryHASHSalt ***
            string encryptedHashText = HasherVersion2.HMACHasher.EncryptHMAC(expectedVSSKey06, HMACValidator.MACSecondaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryHASHSalt = new byte[] { };
            Debug.WriteLine($"ENCRYPTED HASH=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-07
        ///          EDA100E8F35DCE4BD9FDA2EF7456A1E403E09FEB2A95FB3D97F88784B548BF4D
        /// 2) KEY : ?
        ///          HASH: 
        /// OUTPUT : 79326D616B334A4337416354684B6F7970634462416A334B64543236526E3271635250366130654E4348586C46716E52713948424F636E69315361794B69766E68504B4F42316952444E52497149364548786435664968445963723435546962
        /// </summary>
        static void Generate_MACPrimaryKeyHASH()
        {
            string encryptedHashText = HasherVersion2.HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidator.MACPrimaryKeyHASH);

            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryKeyHASH = new byte[] { };
            Debug.WriteLine($"ENCRYPTED HASH=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-07
        ///          EDA100E8F35DCE4BD9FDA2EF7456A1E403E09FEB2A95FB3D97F88784B548BF4D
        /// 2) KEY : MAC PRIMARY KEY HASH
        ///          79326D616B334A4337416354684B6F7970634462416A334B64543236526E3271635250366130654E4348586C46716E52713948424F636E69315361794B69766E68504B4F42316952444E52497149364548786435664968445963723435546962
        ///MD5 HASH: 0B121C1ECDCC79A846B0E05761FC6CC4
        /// OUTPUT : 6976707A7151594F3859676F514C7935363867737251533439626F48655270753744723779586A4C326D7146456E3432563744412F4C4E6342706D4E4D58486D6B43326B6C6C6A33327A4B725469324C4C7254384F572F4C6B414A4E4F475673
        ///MD5 HASH: 2687AB9EE5622135137CF7E7E1C80CD7 
        /// </summary>
        static void Generate_MACSecondaryHASHSalt()
        {
            // ENCRYPT: expectedVSSKey07 with MACPrimaryKeyHASH
            // DECRYPT: MACSecondaryHASHSalt with MACSecondaryKeyHASH == VSSKey07

            // *** HOW TO CONSTRUCT MACSecondaryHASHSalt ***
            string encryptedHashText = HasherVersion2.HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidator.MACPrimaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACSecondaryHASHSalt = new byte[] { };
            Debug.WriteLine($"ENCRYPTED SECONDARY HASH SALT=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
        }

        private static string ReverseString(string message)
        {
            char[] charArray = message.ToCharArray();
            Array.Reverse(charArray);
            return String.Concat(charArray);
        }
    }
}
