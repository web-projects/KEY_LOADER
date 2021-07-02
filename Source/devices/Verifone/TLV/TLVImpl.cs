using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devices.Verifone.TLV
{
    public class TLVImpl
    {
        public byte[] Tag { get; set; }
        public byte[] Data { get; set; }     // Value should be null if innerTags is populated

        public List<TLVImpl> InnerTags { get; set; } // Value should be null if data is populated

        public TLVImpl()            //This will be used for more complex ones (with inner tags)
        {
        }

        public TLVImpl(byte[] tag, byte[] data) //For simple ones (no inner tags)
        {
            this.Tag = tag;
            this.Data = data;
        }

        public TLVImpl(uint tag, byte[] data) //For simple ones (no inner tags)
        {
            this.Tag = SplitUIntToByteArray(tag);
            this.Data = data;
        }

        public static List<TLVImpl> Decode(byte[] data, int startOffset = 0, int dataLength = -1, List<byte[]> tagofTagsList = null)
        {
            if (data == null)
            {
                return null;
            }

            List<TLVImpl> allTags = new List<TLVImpl>();
            int dataOffset = startOffset;

            if (dataLength == -1)
            {
                dataLength = data.Length;
            }

            if (tagofTagsList == null)
            {
                tagofTagsList = new List<byte[]>();
            }

            while (dataOffset < dataLength)
            {
                int tagLength = 1;
                int tagStartOffset = dataOffset;
                byte tagByte0 = data[dataOffset];

                if ((tagByte0 & 0x1F) == 0x1F)
                {
                    // Long form tag
                    dataOffset++;       // Skip first tag byte

                    while ((data[dataOffset] & 0x80) == 0x80)
                    {
                        tagLength++;   // More bit set, so add middle byte to tagLength
                        dataOffset++;
                    }

                    tagLength++;       // Include final byte (where more=0)
                    dataOffset++;
                }
                else
                {
                    // Short form (single byte) tag
                    dataOffset++;       // Simply increment past single byte tag; tagLength is already 1
                }

                // protect from buffer overrun
                if (dataOffset >= data.Length)
                {
                    return null;
                }

                byte lengthByte0 = data[dataOffset];

                // TAG 9F0D is an internal error status
                if (lengthByte0 > data.Length)
                {
                    ByteArrayComparer byteArrayComparer = new ByteArrayComparer();
                    if (byteArrayComparer.Equals(new byte[] { 0x9F, 0x0D }, new byte[] { tagByte0, data[dataOffset - 1] }))
                    {
                        System.Diagnostics.Debug.WriteLine($"VIPA-READ [TAG 9F0D - internal error]");
                        Console.WriteLine($"\nVIPA-READ [TAG 9F0D - internal error]");
                        continue;
                    }
                }

                int tagDataLength = 0;

                if ((lengthByte0 & 0x80) == 0x80)
                {
                    // Long form length
                    int tagDataLengthLength = lengthByte0 & 0x7F;
                    int tagDataLengthIndex = 0;
                    while (tagDataLengthIndex < tagDataLengthLength)
                    {
                        if (dataOffset + tagDataLength > data.Length)
                        {
                            tagDataLength = data.Length - dataOffset;
                        }
                        else
                        {
                            tagDataLength <<= 8;
                        }

                        tagDataLength += data[dataOffset + tagDataLengthIndex + 1];

                        tagDataLengthIndex++;
                    }

                    dataOffset += 1 + tagDataLengthLength;  // Skip long form byte, plus all length bytes
                }
                else
                {
                    // Short form (single byte) length
                    tagDataLength = lengthByte0;
                    dataOffset++;
                }

                TLVImpl tag = new TLVImpl
                {
                    Tag = new byte[tagLength]
                };

                Array.Copy(data, tagStartOffset, tag.Tag, 0, tagLength);

                bool foundTagOfTags = false;
                foreach (var tagOftags in tagofTagsList)
                {
                    if (tagOftags.SequenceEqual(tag.Tag))
                    {
                        foundTagOfTags = true;
                    }
                }

                if (foundTagOfTags)
                {
                    if (dataOffset + tagDataLength > data.Length)
                    {
                        tagDataLength = data.Length - dataOffset;
                    }

                    tag.InnerTags = Decode(data, dataOffset, dataOffset + tagDataLength, tagofTagsList);
                }
                else
                {
                    // special handling of POS cancellation: "ABORTED" is in the data field without a length
                    if (tagDataLength > data.Length)
                    {
                        tagDataLength = data.Length - dataOffset;
                    }
                    else if (tagDataLength + dataOffset > data.Length)
                    {
                        // TAG 9F0D is an internal error status: datalen mismatch
                        // i.e. DDFDF12-08-398EF6C2AD1A
                        tagDataLength = data.Length - dataOffset;
                        System.Diagnostics.Debug.WriteLine($"VIPA-READ [LENGTH MISMATCH FOR TAG {BitConverter.ToString(tag.Tag).Replace("-", "")}]");
                        Console.WriteLine($"VIPA-READ [LENGTH MISMATCH FOR TAG {BitConverter.ToString(tag.Tag).Replace("-", "")}]\n");
                    }
                    tag.Data = new byte[tagDataLength];
                    Array.Copy(data, dataOffset, tag.Data, 0, tagDataLength);
                }

                allTags.Add(tag);

                dataOffset += tagDataLength;
            }

            return /*(allTags.Count == 0) ? null :*/ allTags;
        }

        //When you want to automatically decode more than 1 layer of innertags
        public static List<TLVImpl> DeepDecode(byte[] data, int count = 0)
        {
            if (count < 0)      //don't go deeper than needed
            {
                return null;
            }

            List<TLVImpl> firstLayer = Decode(data);
            if (firstLayer == null || firstLayer.Count == 0)      //If this is no longer decodable, don't bother going deeper
            {
                return null;
            }

            foreach (TLVImpl nextLayer in firstLayer)
            {
                nextLayer.InnerTags = DeepDecode(nextLayer.Data, count - 1);
            }

            return firstLayer;
        }

        public static byte[] Encode(TLVImpl tags)
        {
            return Encode(new List<TLVImpl> { tags });
        }

        public static byte[] Encode(List<TLVImpl> tags)
        {
            List<byte[]> allTagBytes = new List<byte[]>();
            int allTagBytesLength = 0;

            foreach (TLVImpl tag in tags)
            {
                int len = tag.Tag.Length;
                byte[] data = tag.Data;

                if (tag.InnerTags != null)
                {
                    data = Encode(tag.InnerTags);
                }

                if (data == null)
                {
                    data = Array.Empty<byte>();
                }

                if (data.Length > 65535)
                {
                    throw new Exception($"TLV data too long for Encode: length {data.Length}");
                }

                if (data.Length > 255)
                {
                    len += 3 + data.Length;
                }
                else if (data.Length > 127)
                {
                    len += 2 + data.Length;
                }
                else
                {
                    len += 1 + data.Length;
                }

                byte[] tagData = new byte[len];
                int tagDataOffset = 0;

                Array.Copy(tag.Tag, 0, tagData, tagDataOffset, tag.Tag.Length);
                tagDataOffset += tag.Tag.Length;

                if (data.Length > 255)
                {
                    tagData[tagDataOffset + 0] = 0x80 + 2;

                    tagData[tagDataOffset + 1] = (byte)(data.Length / 256);

                    tagData[tagDataOffset + 2] = (byte)(data.Length % 256);

                    tagDataOffset += 3;
                }
                else if (data.Length > 127)
                {
                    tagData[tagDataOffset + 0] = 0x80 + 1;

                    tagData[tagDataOffset + 1] = (byte)data.Length;

                    tagDataOffset += 2;
                }
                else
                {
                    tagData[tagDataOffset] = (byte)data.Length;
                    tagDataOffset += 1;
                }

                Array.Copy(data, 0, tagData, tagDataOffset, data.Length);
                tagDataOffset += data.Length;

                allTagBytes.Add(tagData);
                allTagBytesLength += tagDataOffset;
            }

            byte[] allBytes = new byte[allTagBytesLength];
            int allBytesOffset = 0;

            foreach (byte[] tagBytes in allTagBytes)
            {
                Array.Copy(tagBytes, 0, allBytes, allBytesOffset, tagBytes.Length);
                allBytesOffset += tagBytes.Length;
            }

            return allBytes;
        }

        public static byte[] SplitUIntToByteArray(uint value)
        {
            //This is a lot faster than doing a loop (3s for 10M ops vs 5s for 10M ops using loops)
            if (value <= 0xFF)
                return new byte[] { (byte)value };
            else if (value <= 0xFFFF)
                return new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
            else if (value <= 0xFFFFFF)
                return new byte[] { (byte)(value >> 16), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF) };
            else
                return new byte[] { (byte)(value >> 24), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF) };
        }

        private static uint CombineByteArray(ReadOnlySpan<byte> span)
        {
            uint result = 0;
            for (int i = 0; i < span.Length; i++)
            {
                result += (uint)(span[i] << ((span.Length - i - 1) * 8));
            }

            return result;
        }
    }
}
