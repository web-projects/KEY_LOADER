//using HMACHasher.HasherVersion1;

using System;

namespace HMACHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            HasherVersion2Impl.TestHasher();
            _ = Console.ReadKey(true).Key;
        }
    }
}
