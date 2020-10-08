using HMACHasher.Hasher.Manager;
using System;

namespace HMACHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            //HasherVersion1Impl.TestHasher();
            HasherVersion2Impl.TestHasher();
            _ = Console.ReadKey(true).Key;
        }
    }
}
