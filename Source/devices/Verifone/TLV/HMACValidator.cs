﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Devices.Verifone.TLV
{
    static public class HMACValidator
    {
        public static readonly byte[] MACPrimaryPANSalt = new byte[] { 0x76, 0x58, 0x38, 0x6c, 0x36, 0x53, 0x4f, 0x6d, 0x53, 0x76, 0x6d, 0x70, 0x57, 0x52, 0x69, 0x36, 0x41, 0x44, 0x6e, 0x53, 0x73, 0x71, 0x6c, 0x5a, 0x47, 0x4c, 0x6f, 0x41, 0x4f, 0x64, 0x4b, 0x79, 0x71, 0x56, 0x6b, 0x59, 0x75, 0x67, 0x41, 0x35, 0x30, 0x72, 0x4b, 0x41, 0x4e, 0x77, 0x4e, 0x41, 0x35, 0x4d, 0x2b, 0x6e, 0x78, 0x41, 0x3d, 0x3d };
        public static readonly byte[] MACPrimaryHASH = new byte[] { 0x79, 0x32, 0x6d, 0x61, 0x6b, 0x33, 0x4a, 0x43, 0x37, 0x41, 0x63, 0x54, 0x68, 0x4b, 0x6f, 0x79, 0x70, 0x63, 0x44, 0x62, 0x41, 0x6a, 0x33, 0x4b, 0x64, 0x54, 0x32, 0x36, 0x52, 0x6e, 0x32, 0x71, 0x63, 0x52, 0x50, 0x36, 0x61, 0x30, 0x65, 0x4e, 0x43, 0x48, 0x58, 0x6c, 0x46, 0x71, 0x6e, 0x52, 0x71, 0x39, 0x48, 0x42, 0x4f, 0x63, 0x6e, 0x69, 0x31, 0x53, 0x61, 0x79, 0x4b, 0x69, 0x76, 0x6e, 0x68, 0x50, 0x4b, 0x4f, 0x42, 0x31, 0x69, 0x52, 0x44, 0x4e, 0x52, 0x49, 0x71, 0x49, 0x36, 0x45, 0x48, 0x78, 0x64, 0x35, 0x66, 0x49, 0x68, 0x44, 0x59, 0x63, 0x72, 0x34, 0x35, 0x54, 0x69, 0x62 };
        public static readonly byte[] MACSecondaryHASH = new byte[] { 0x44, 0x31, 0x46, 0x38, 0x38, 0x32, 0x37, 0x44, 0x44, 0x39, 0x32, 0x37, 0x36, 0x46, 0x39, 0x46, 0x38, 0x30, 0x46, 0x38, 0x38, 0x39, 0x30, 0x44, 0x33, 0x45, 0x36, 0x30, 0x37, 0x41, 0x43, 0x30, 0x33, 0x43, 0x41, 0x30, 0x32, 0x32, 0x42, 0x41, 0x39, 0x31, 0x42, 0x38, 0x30, 0x32, 0x34, 0x33, 0x35, 0x36, 0x44, 0x43, 0x44, 0x46, 0x35, 0x34, 0x41, 0x44, 0x34, 0x33, 0x34, 0x46, 0x38, 0x33 };
        public static readonly byte[] MACPrimaryHASHSalt = new byte[] { 0x5a, 0x2b, 0x50, 0x66, 0x4d, 0x4c, 0x55, 0x76, 0x36, 0x64, 0x76, 0x46, 0x4f, 0x77, 0x72, 0x31, 0x4d, 0x75, 0x36, 0x50, 0x63, 0x76, 0x58, 0x30, 0x68, 0x45, 0x67, 0x6c, 0x57, 0x71, 0x38, 0x6e, 0x6e, 0x57, 0x74, 0x5a, 0x58, 0x33, 0x2b, 0x6c, 0x45, 0x79, 0x59, 0x38, 0x77, 0x58, 0x37, 0x67, 0x7a, 0x6c, 0x49, 0x56, 0x51, 0x69, 0x53, 0x76, 0x58, 0x65, 0x76, 0x57, 0x66, 0x56, 0x4b, 0x78, 0x6c, 0x6a, 0x45, 0x49, 0x4e, 0x49, 0x52, 0x42, 0x30, 0x2f, 0x43, 0x79, 0x69, 0x51, 0x50, 0x62, 0x4d, 0x55, 0x62, 0x30, 0x36, 0x59, 0x41, 0x33, 0x41, 0x30, 0x44, 0x6b, 0x7a, 0x36, 0x66, 0x45 };
        public static readonly byte[] MACSecondaryHASHSalt = new byte[] { 0x69, 0x76, 0x70, 0x7a, 0x71, 0x51, 0x59, 0x4f, 0x38, 0x59, 0x67, 0x6f, 0x51, 0x4c, 0x79, 0x35, 0x36, 0x38, 0x67, 0x73, 0x72, 0x51, 0x53, 0x34, 0x39, 0x62, 0x6f, 0x48, 0x65, 0x52, 0x70, 0x75, 0x37, 0x44, 0x72, 0x37, 0x79, 0x58, 0x6a, 0x4c, 0x32, 0x6d, 0x71, 0x46, 0x45, 0x6e, 0x34, 0x32, 0x56, 0x37, 0x44, 0x41, 0x2f, 0x4c, 0x4e, 0x63, 0x42, 0x70, 0x6d, 0x4e, 0x4d, 0x58, 0x48, 0x6d, 0x6b, 0x43, 0x32, 0x6b, 0x6c, 0x6c, 0x6a, 0x33, 0x32, 0x7a, 0x4b, 0x72, 0x54, 0x69, 0x32, 0x4c, 0x4c, 0x72, 0x54, 0x38, 0x4f, 0x57, 0x2f, 0x4c, 0x6b, 0x41, 0x4a, 0x4e, 0x4f, 0x47, 0x56, 0x73 };
    }
}
