using System;
using System.Diagnostics;
using System.Text;

namespace HMACHasher.Hasher.Manager
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
    /// byte[] MACPrimaryPANSalt    : HMACHasher.EncryptHMAC(PANSecret, HMACValidator.MACSecondaryKeyHASH)
    /// 
    /// byte[] MACPrimaryHASHSalt   : HMACHasher.EncryptHMAC(expectedVSSKey06, HMACValidator.MACSecondaryKeyHASH)
    /// byte[] MACPrimaryKeyHASH    :
    /// 
    /// byte[] MACSecondaryHASHSalt : HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidator.MACPrimaryKeyHASH) 
    ///
    /// USAGE:
    /// 
    /// STEP1  : VSS-6 => HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryPANSalt), HMACValidator.MACSecondaryKeyHASH)
    /// HMAC   : 4111111111111111
    /// RESULT : 98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0
    /// COMPARE: HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH)
    /// 
    /// STEP2  : VSS-7 => HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACPrimaryHASHSalt), HMACValidator.MACSecondaryKeyHASH)
    /// HMAC   : 98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0
    /// RESULT : D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83
    /// COMPARE: HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidator.MACSecondaryHASHSalt), HMACValidator.MACPrimaryKeyHASH)
    /// 
    /// </summary>
    public static class HasherVersion1Impl
    {
        // OBTAIN THE FOLLOWING FROM PYTHON SCRIPT: TC_4111_GENERATE_HMAC_ASCII.py
        // "Generated HMAC HOSTID-06:"
        static string expectedVSSKey06 = "98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0";
        static byte[] vssKey06Hash = new byte[] { 0x98, 0xA8, 0xAA, 0xED, 0x5A, 0x2B, 0xA9, 0xE2, 0x28, 0xB1, 0x38, 0x27, 0x4F, 0xDF, 0x54, 0x6D, 0x66, 0x88, 0xD2, 0xAB, 0x8D, 0x9A, 0x36, 0xE0, 0xA5, 0x0A, 0x5B, 0xF3, 0xB1, 0x42, 0xAF, 0xB0 };
        //static byte[] vssKey06KeyHash = new byte[] { 0x39, 0x38, 0x41, 0x38, 0x41, 0x41, 0x45, 0x44, 0x35, 0x41, 0x32, 0x42, 0x41, 0x39, 0x45, 0x32, 0x32, 0x38, 0x42, 0x31, 0x33, 0x38, 0x32, 0x37, 0x34, 0x46, 0x44, 0x46, 0x35, 0x34, 0x36, 0x44, 0x36, 0x36, 0x38, 0x38, 0x44, 0x32, 0x41, 0x42, 0x38, 0x44, 0x39, 0x41, 0x33, 0x36, 0x45, 0x30, 0x41, 0x35, 0x30, 0x41, 0x35, 0x42, 0x46, 0x33, 0x42, 0x31, 0x34, 0x32, 0x41, 0x46, 0x42, 0x30 };
        // 98a8aaed5a2ba9e228b138274fdf546d6688d2ab8d9a36e0a50a5bf3b142afb0
        static byte[] vssKey06KeyHash = new byte[] { 0x39, 0x38, 0x61, 0x38, 0x61, 0x61, 0x65, 0x64, 0x35, 0x61, 0x32, 0x62, 0x61, 0x39, 0x65, 0x32, 0x32, 0x38, 0x62, 0x31, 0x33, 0x38, 0x32, 0x37, 0x34, 0x66, 0x64, 0x66, 0x35, 0x34, 0x36, 0x64, 0x36, 0x36, 0x38, 0x38, 0x64, 0x32, 0x61, 0x62, 0x38, 0x64, 0x39, 0x61, 0x33, 0x36, 0x65, 0x30, 0x61, 0x35, 0x30, 0x61, 0x35, 0x62, 0x66, 0x33, 0x62, 0x31, 0x34, 0x32, 0x61, 0x66, 0x62, 0x30 };
        // "Generated HMAC HOSTID-07:"
        static string expectedVSSKey07 = "D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83";
        static byte[] vssKey07Hash = new byte[] { 0xD1, 0xF8, 0x82, 0x7D, 0xD9, 0x27, 0x6F, 0x9F, 0x80, 0xF8, 0x89, 0x0D, 0x3E, 0x60, 0x7A, 0xC0, 0x3C, 0xA0, 0x22, 0xBA, 0x91, 0xB8, 0x02, 0x43, 0x56, 0xDC, 0xDF, 0x54, 0xAD, 0x43, 0x4F, 0x83 };

        // VERSION 1
        // MACPRIMARYHASH  : 79326d616b334a4337416354684b6f7970634462416a334b64543236526e3271635250366130654e4348586c46716e52713948424f636e69315361794b69766e68504b4f42316952444e52497149364548786435664968445963723435546962
        // MD5-HASH        : 0B121C1ECDCC79A846B0E05761FC6CC4

        // VERSION 2
        // MACPRIMARYHASH  : 39394138414145443541324241394532323842313338323734464446353436443636383844324142384439413336453041353041354246334231343241464230
        // MD5-HASH        : D700C6ADA0159742C38CBE4D9D03890B

        // VSS-7-HMAC      : D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83
        // MACSECONDARYHASH: 44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833
        // MD5-HASH        : 9A40969CEA07981C36906CDB56108C1D

        // VSS-6-HMAC      : 98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0
        // MACPRIMARYHASH  : 39384138414145443541324241394532323842313338323734464446353436443636383844324142384439413336453041353041354246334231343241464230
        // MD5-HASH        : 8EED5B9CE698138BF71A2512CFF52DBD

        // PAN
        //static string PANShortSecret = "4111111111111111";
        static string PANSecret = "34313131313131313131313131313131";
        //static byte[] PANGeneration = new byte[] { 0x41, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 };

        public static void TestHasher()
        {
            // THIS IS THE EXPECTED HOSTID-07 KEY
            Debug.WriteLine($"SECONDARY HASH=[{Encoding.UTF8.GetString(HMACValidatorVersion1.MACSecondaryKeyHASH)}]");
            bool isMatch = expectedVSSKey07.Equals(Encoding.UTF8.GetString(HMACValidatorVersion1.MACSecondaryKeyHASH));
            Debug.WriteLine($"SECONDARY HASH IS A MATCH? {isMatch}");

            // *** HOW TO CONSTRUCT MACPrimaryPANSalt ***
            // how to create MACPrimaryPANSalt ???
            // USAGE: decrypts HostId-Key-06
            Generate_MACPrimaryPANSalt();

            // TEST MACPrimaryPANSalt
            string decriptedPrimaryPANSalt = HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidatorVersion1.MACPrimaryPANSalt), HMACValidatorVersion1.MACSecondaryKeyHASH);
            Debug.WriteLine($"PAN DECRYPTED HASH={decriptedPrimaryPANSalt}");
            Console.WriteLine($"PAN DECRYPTED HASH={decriptedPrimaryPANSalt}");

            // *** HOW TO CONSTRUCT MACPrimaryHASHSalt ***
            // USAGE   : decrypts HostId-Key-06
            // REQUIRES: expectedVSSKey06, HMACValidator.MACSecondaryKeyHASH
            Generate_MACPrimaryHASHSalt();

            // KEY HOST-ID 06 VALIDATOR
            string decriptedVSS06Hash = HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidatorVersion1.MACPrimaryHASHSalt), HMACValidatorVersion1.MACSecondaryKeyHASH);
            Debug.WriteLine($"DECRYPTED VSS-6 HASH={decriptedVSS06Hash}");
            Console.WriteLine($"DECRYPTED VSS-6 HASH={decriptedVSS06Hash}");

            // *** HOW TO CONSTRUCT MACPrimaryKeyHASH ***
            // USAGE   : 
            // REQUIRES: 
            Generate_MACPrimaryKeyHASH();

            // *** HOW TO CONSTRUCT MACSecondaryHASHSalt ***
            // USAGE   : decrypts HostId-Key-06
            // REQUIRES: expectedVSSKey06, HMACValidator.MACPrimaryKeyHASH
            Generate_MACSecondaryHASHSalt();

            // KEY HOST-ID 07 VALIDATOR
            string decriptedVSS07Hash = HMACHasher.DecryptHMAC(Encoding.ASCII.GetString(HMACValidatorVersion1.MACSecondaryHASHSalt), HMACValidatorVersion1.MACPrimaryKeyHASH);
            Debug.WriteLine($"DECRYPTED VSS-7 HASH={decriptedVSS07Hash}");
            Console.WriteLine($"DECRYPTED VSS-7 HASH={decriptedVSS07Hash}");
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
            string encryptedHashText = HMACHasher.EncryptHMAC(PANSecret, HMACValidatorVersion1.MACSecondaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            //Debug.WriteLine($"ENCRYPTED HASH={encryptedHashText}");
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryPANSalt = new byte[] { };
            Debug.WriteLine($"MACPrimaryPANSalt=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
            Debug.WriteLine($"HASH LEN={encryptedHashBytes.Length}");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-06
        ///          98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0
        /// 2) KEY : MAC SECONDARY KEY HASH
        ///          44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833
        ///          HASH: 9A40969CEA07981C36906CDB56108C1D
        /// OUTPUT : 5A2B50664D4C5576366476464F7772314D753650637658306845676C5771386E6E57745A58332B6C45795938775837677A6C4956516953765865765766564B786C6A45494E495242302F4379695150624D556230365941334130446B7A366645         
        /// </summary>
        static void Generate_MACPrimaryHASHSalt()
        {
            // ENCRYPT: expectedVSSKey06 with MACSecondaryKeyHASH == MACPrimaryHASHSalt
            // DECRYPT: MACPrimaryPANSalt with MACSecondaryKeyHASH == VSSKey-06

            // *** HOW TO CONSTRUCT MACPrimaryHASHSalt ***
            string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey06, HMACValidatorVersion1.MACSecondaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryHASHSalt = new byte[] { };
            Debug.WriteLine($"MACPrimaryHASHSalt=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
            Debug.WriteLine($"HASH LEN={encryptedHashBytes.Length}");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-07
        ///          D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83
        /// 2) KEY : ?
        ///          HASH: 
        /// OUTPUT : 79326D616B334A4337416354684B6F7970634462416A334B64543236526E3271635250366130654E4348586C46716E52713948424F636E69315361794B69766E68504B4F42316952444E52497149364548786435664968445963723435546962
        /// </summary>
        static void Generate_MACPrimaryKeyHASH()
        {
            // ENCRYPT: expectedVSSKey07 with ???
            // USAGE: 
            // CREATE MACSecondaryHASHSalt
            // DECRYPT: VSS-07 == Decrypt(MACSecondaryHASHSalt, HMACValidator.MACPrimaryKeyHASH)

            // PAN SECRET      : 34313131313131313131313131313131

            // VSS-6-HMAC      : 98A8AAED5A2BA9E228B138274FDF546D6688D2AB8D9A36E0A50A5BF3B142AFB0
            // MACPRIMARYHASH  : 39384138414145443541324241394532323842313338323734464446353436443636383844324142384439413336453041353041354246334231343241464230
            // MD5-HASH        : 8EED5B9CE698138BF71A2512CFF52DBD
            // MACPRIMARYHASHL : 39386138616165643561326261396532323862313338323734666466353436643636383864326162386439613336653061353061356266336231343261666230

            // VSS-07 HMAC     : D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83
            // MD5-HASH        : 9A40969CEA07981C36906CDB56108C1D
            // MACSECONDARYHASH: 44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833
            string vss07BitesU = "44314638383237444439323736463946383046383839304433453630374143303343413032324241393142383032343335364443444635344144343334463833";
            string vss07BitesL = "64316638383237646439323736663966383066383839306433653630376163303363613032326261393162383032343335366463646635346164343334663833";

            // MACPrimaryHASHSalt
            // 5A2B50664D4C5576366476464F7772314D753650637658306845676C5771386E6E57745A58332B6C45795938775837677A6C4956516953765865765766564B786C6A45494E495242302F4379695150624D556230365941334130446B7A366645

            //
            // DATA            : 79326d616b334a4337416354684b6f7970634462416a334b64543236526e3271635250366130654e4348586c46716e52713948424f636e69315361794b69766e68504b4f42316952444e52497149364548786435664968445963723435546962
            // MD5 HASH        : 0B121C1ECDCC79A846B0E05761FC6CC4

            // expectedVSSKey07
            //         : D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83

            // MACSecondaryHASHSalt
            // 6976707A7151594F3859676F514C7935363867737251533439626F48655270753744723779586A4C326D7146456E3432563744412F4C4E6342706D4E4D58486D6B43326B6C6C6A33327A4B725469324C4C7254384F572F4C6B414A4E4F475673

            // *** HOW TO CONSTRUCT MACPrimaryKeyHASH ***
            // DECRYPT: MACSecondaryHASHSalt with MACPrimaryKeyHASH == HostIdKey07
            // ENCRYPT: ?
            // ENCRYPT: expectedVSSKey06 with MACSecondaryHASHSalt == MACPrimaryKeyHASH

            string expectedVSSKey07Lower = expectedVSSKey07.ToLower();
            // vssKey06Hash: VSSKey06-HMAC in BYTE ARRAY
            //string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07, vssKey06Hash);
            string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07, vssKey06KeyHash);
            //string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07Lower, vssKey06KeyHash);
            //string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidatorVersion1.MACSecondaryHASHSalt);
            //string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidatorVersion1.MACPrimaryHASHSalt);
            //string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07Lower, HMACValidatorVersion1.MACPrimaryPANSalt);

            //string encryptedHashText = HMACHasher.EncryptHMAC(vss07BitesL, HMACValidatorVersion1.MACSecondaryHASHSalt);
            //string encryptedHashText = HMACHasher.EncryptHMAC(vss07BitesL, HMACValidatorVersion1.MACPrimaryHASHSalt);
            //string encryptedHashText = HMACHasher.EncryptHMAC(vss07BitesL, HMACValidatorVersion1.MACPrimaryPANSalt);
            //string encryptedHashText = HMACHasher.EncryptHMAC(vss07BitesL, vssKey06Hash);

            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACPrimaryKeyHASH = new byte[] { };
            Debug.WriteLine($"MACPrimaryKeyHASH=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
            Debug.WriteLine($"HASH LEN={encryptedHashBytes.Length}");
        }

        /// <summary>
        /// REQUIRES:
        /// 1) DATA: HOSTID-07
        ///          D1F8827DD9276F9F80F8890D3E607AC03CA022BA91B8024356DCDF54AD434F83
        /// 2) KEY : MAC PRIMARY KEY HASH
        ///          79326D616B334A4337416354684B6F7970634462416A334B64543236526E3271635250366130654E4348586C46716E52713948424F636E69315361794B69766E68504B4F42316952444E52497149364548786435664968445963723435546962
        ///MD5 HASH: 0B121C1ECDCC79A846B0E05761FC6CC4
        /// OUTPUT : 6976707A7151594F3859676F514C7935363867737251533439626F48655270753744723779586A4C326D7146456E3432563744412F4C4E6342706D4E4D58486D6B43326B6C6C6A33327A4B725469324C4C7254384F572F4C6B414A4E4F475673
        ///MD5 HASH: 2687AB9EE5622135137CF7E7E1C80CD7 
        /// </summary>
        static void Generate_MACSecondaryHASHSalt()
        {
            // HARD DEPENDENCY: MACPrimaryKeyHASH
            //
            // ENCRYPT: expectedVSSKey07 with MACPrimaryKeyHASH
            // DECRYPT: MACSecondaryHASHSalt with MACSecondaryKeyHASH == VSSKey07

            // *** HOW TO CONSTRUCT MACSecondaryHASHSalt ***
            string encryptedHashText = HMACHasher.EncryptHMAC(expectedVSSKey07, HMACValidatorVersion1.MACPrimaryKeyHASH);
            byte[] encryptedHashBytes = Encoding.ASCII.GetBytes(encryptedHashText);
            // TAKE THE VALUE IN THE DEBUGGER AND REPLACE IT IN HMACValidator: 
            // public static readonly byte[] MACSecondaryHASHSalt = new byte[] { };
            Debug.WriteLine($"MACSecondaryHASHSalt=[0x{BitConverter.ToString(encryptedHashBytes).Replace("-", ", 0x").ToLower()}]");
            Debug.WriteLine($"HASH LEN={encryptedHashBytes.Length}");
        }

        private static string ReverseString(string message)
        {
            char[] charArray = message.ToCharArray();
            Array.Reverse(charArray);
            return String.Concat(charArray);
        }
    }
}
