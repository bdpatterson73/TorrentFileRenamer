using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.IO.Hashing;


namespace TorrentFileRenamer
{
    public static class HashHelper
    {


        public static string CRC32File(string fileNamePath)
        {
            if (!File.Exists(fileNamePath))
                return "";
            string retVal = "";
            // Create a new instance of the CRC32 class.
            CRC32 crc32 = new CRC32();

            // Open the file and get a stream that represents it.
            using (FileStream fileStream = File.Open(fileNamePath, FileMode.Open))
            {
               byte[] hash =  crc32.ComputeHash(fileStream);
                // Calculate the hash for the file.
                //byte[] hash = crc32.GetHashAndReset(fileStream);

                // Print the hash value.
                retVal = BitConverter.ToString(hash).Replace("-","");
            }

            return retVal;
        }
    }
}
