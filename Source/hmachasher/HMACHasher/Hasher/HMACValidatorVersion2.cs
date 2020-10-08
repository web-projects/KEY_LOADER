﻿namespace HMACHasher.Hasher
{
    static public class HMACValidatorVersion2
    {
        // MACPrimaryPANSalt: ENCRYPT PAN-SECRET with MAC-SECONDARY-HASH
        public static readonly byte[] MACPrimaryPANSalt    = new byte[] { 0x34, 0x66, 0x31, 0x77, 0x37, 0x70, 0x35, 0x73, 0x63, 0x77, 0x35, 0x2f, 0x59, 0x74, 0x6e, 0x41, 0x4c, 0x6f, 0x6d, 0x45, 0x46, 0x6e, 0x39, 0x69, 0x32, 0x63, 0x41, 0x75, 0x69, 0x59, 0x51, 0x57, 0x66, 0x32, 0x4c, 0x5a, 0x77, 0x43, 0x36, 0x4a, 0x68, 0x42, 0x59, 0x6b, 0x46, 0x64, 0x75, 0x43, 0x47, 0x54, 0x71, 0x75, 0x48, 0x77, 0x3d, 0x3d };

        // MACPrimaryHASH: RANDOM VALUE THAT GIVES PROPER HASH?
        public static readonly byte[] MACPrimaryKeyHASH = new byte[] { 0x79, 0x32, 0x6d, 0x61, 0x6b, 0x33, 0x4a, 0x43, 0x37, 0x41, 0x63, 0x54, 0x68, 0x4b, 0x6f, 0x79, 0x70, 0x63, 0x44, 0x62, 0x41, 0x6a, 0x33, 0x4b, 0x64, 0x54, 0x32, 0x36, 0x52, 0x6e, 0x32, 0x71, 0x63, 0x52, 0x50, 0x36, 0x61, 0x30, 0x65, 0x4e, 0x43, 0x48, 0x58, 0x6c, 0x46, 0x71, 0x6e, 0x52, 0x71, 0x39, 0x48, 0x42, 0x4f, 0x63, 0x6e, 0x69, 0x31, 0x53, 0x61, 0x79, 0x4b, 0x69, 0x76, 0x6e, 0x68, 0x50, 0x4b, 0x4f, 0x42, 0x31, 0x69, 0x52, 0x44, 0x4e, 0x52, 0x49, 0x71, 0x49, 0x36, 0x45, 0x48, 0x78, 0x64, 0x35, 0x66, 0x49, 0x68, 0x44, 0x59, 0x63, 0x72, 0x34, 0x35, 0x54, 0x69, 0x62 };

        // MACPrimaryHASH: VSSKey-06-HMAC in BYTE ARRAY
        //public static readonly byte[] MACPrimaryKeyHASH    = new byte[] { 0xC4, 0x64, 0x08, 0x40, 0x95, 0xAE, 0x8D, 0x1F, 0x16, 0xB5, 0x76, 0x02, 0x72, 0x49, 0x55, 0x65, 0x1D, 0x45, 0xB4, 0xB6, 0x08, 0x3E, 0x4A, 0x5E, 0x41, 0xC4, 0x83, 0x70, 0x81, 0xF4, 0x60, 0xA6 };

        // MACSecondaryHASH: VSSKey-07-HMAC in BYTE ARRAY
        public static readonly byte[] MACSecondaryKeyHASH  = new byte[] { 0xED, 0xA1, 0x00, 0xE8, 0xF3, 0x5D, 0xCE, 0x4B, 0xD9, 0xFD, 0xA2, 0xEF, 0x74, 0x56, 0xA1, 0xE4, 0x03, 0xE0, 0x9F, 0xEB, 0x2A, 0x95, 0xFB, 0x3D, 0x97, 0xF8, 0x87, 0x84, 0xB5, 0x48, 0xBF, 0x4D };
        // MACPrimaryHASHSalt: ENCRYPT VSSKey-06 with MAC-SECONDARY-HASH
        public static readonly byte[] MACPrimaryHASHSalt   = new byte[] { 0x77, 0x4f, 0x58, 0x30, 0x39, 0x64, 0x57, 0x74, 0x50, 0x50, 0x32, 0x31, 0x55, 0x55, 0x44, 0x74, 0x33, 0x50, 0x63, 0x69, 0x6b, 0x30, 0x6e, 0x68, 0x76, 0x72, 0x71, 0x33, 0x6e, 0x66, 0x69, 0x5a, 0x7a, 0x59, 0x5a, 0x47, 0x6c, 0x64, 0x6f, 0x4a, 0x54, 0x4a, 0x47, 0x42, 0x39, 0x66, 0x67, 0x42, 0x55, 0x58, 0x75, 0x4f, 0x4c, 0x45, 0x75, 0x6b, 0x58, 0x64, 0x68, 0x75, 0x4e, 0x52, 0x47, 0x2b, 0x41, 0x46, 0x6b, 0x37, 0x67, 0x33, 0x6f, 0x7a, 0x73, 0x34, 0x2f, 0x7a, 0x4e, 0x63, 0x6f, 0x6b, 0x77, 0x68, 0x2b, 0x76, 0x79, 0x79, 0x51, 0x56, 0x32, 0x34, 0x49, 0x5a, 0x4f, 0x71, 0x34, 0x66 };

        // MACSecondaryHASHSalt: ENCRYPT VSSKey-07 with MAC-PRIMARY-HASH
        //public static readonly byte[] MACSecondaryHASHSalt = new byte[] { 0x36, 0x38, 0x68, 0x45, 0x32, 0x6b, 0x4b, 0x54, 0x71, 0x31, 0x53, 0x30, 0x49, 0x6f, 0x41, 0x44, 0x65, 0x4f, 0x4a, 0x57, 0x61, 0x64, 0x6a, 0x59, 0x39, 0x54, 0x6f, 0x51, 0x64, 0x61, 0x38, 0x73, 0x67, 0x4a, 0x2b, 0x63, 0x38, 0x72, 0x6c, 0x54, 0x7a, 0x5a, 0x50, 0x41, 0x62, 0x32, 0x47, 0x61, 0x45, 0x67, 0x75, 0x47, 0x35, 0x38, 0x68, 0x45, 0x6f, 0x38, 0x6a, 0x65, 0x53, 0x56, 0x68, 0x37, 0x61, 0x50, 0x72, 0x48, 0x43, 0x2f, 0x72, 0x48, 0x62, 0x7a, 0x45, 0x5a, 0x33, 0x5a, 0x42, 0x54, 0x6e, 0x7a, 0x44, 0x69, 0x74, 0x53, 0x42, 0x53, 0x77, 0x4f, 0x62, 0x6c, 0x62, 0x32, 0x58, 0x53 };
        // MACSecondaryHASHSalt: ENCRYPT VSSKey-07 with MAC-PRIMARY-HASH
        public static readonly byte[] MACSecondaryHASHSalt = new byte[] { 0x70, 0x57, 0x64, 0x65, 0x47, 0x41, 0x54, 0x42, 0x4d, 0x50, 0x42, 0x76, 0x4e, 0x72, 0x38, 0x53, 0x34, 0x2f, 0x63, 0x6a, 0x54, 0x52, 0x35, 0x72, 0x32, 0x74, 0x49, 0x71, 0x53, 0x48, 0x49, 0x31, 0x2b, 0x34, 0x49, 0x31, 0x4a, 0x65, 0x63, 0x68, 0x31, 0x65, 0x32, 0x32, 0x39, 0x4c, 0x45, 0x46, 0x6e, 0x36, 0x59, 0x36, 0x59, 0x45, 0x52, 0x32, 0x46, 0x69, 0x68, 0x59, 0x62, 0x74, 0x77, 0x5a, 0x44, 0x50, 0x59, 0x65, 0x7a, 0x47, 0x34, 0x6d, 0x59, 0x61, 0x79, 0x49, 0x49, 0x39, 0x6b, 0x6d, 0x54, 0x65, 0x69, 0x67, 0x47, 0x47, 0x2f, 0x4c, 0x6b, 0x41, 0x4a, 0x4e, 0x4f, 0x47, 0x56, 0x73 };

        // KEY INJECTION
        public static readonly byte[] HMACKEY06 = new byte[] { 0x19, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x29, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x39, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x49, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67 };
        public static readonly byte[] HMACKEY07 = new byte[] { 0x15, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x25, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x35, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23 };
    }
}
