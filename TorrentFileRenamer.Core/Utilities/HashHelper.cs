namespace TorrentFileRenamer.Core.Utilities
{
    public static class HashHelper
    {
        public static string CRC32File(string fileNamePath)
        {
            if (!File.Exists(fileNamePath))
                return "";

            string retVal = "";
            CRC32 crc32 = new CRC32();

            using (FileStream fileStream = File.Open(fileNamePath, FileMode.Open))
            {
                byte[] hash = crc32.ComputeHash(fileStream);
                retVal = BitConverter.ToString(hash).Replace("-", "");
            }

            return retVal;
        }
    }
}