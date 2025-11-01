using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace TorrentFileRenamer.Core.Utilities
{
    public class CRC32 : HashAlgorithm
    {
        private static readonly Hashtable _crc32TablesCache;
        private static readonly CRC32 _defaultCRC;
        private static uint _allOnes = 0xffffffff;
      
        [CLSCompliant(false)] 
        public static readonly uint DefaultPolynomial = 0xEDB88320;

    private readonly uint[] _crc32Table;
        private uint _crc;

      static CRC32()
     {
   _crc32TablesCache = Hashtable.Synchronized(new Hashtable());
     _defaultCRC = new CRC32();
   }

   public CRC32() : this(DefaultPolynomial)
        {
        }

        [CLSCompliant(false)]
        public CRC32(uint polynomial)
        {
            HashSizeValue = 32;
            _crc32Table = (uint[])_crc32TablesCache[polynomial]!;
     if (_crc32Table == null)
        {
   _crc32Table = _buildCRC32Table(polynomial);
  _crc32TablesCache.Add(polynomial, _crc32Table);
            }
    Initialize();
        }

        public override void Initialize()
    {
            _crc = _allOnes;
        }

        protected override void HashCore(byte[] buffer, int offset, int count)
        {
    for (int i = offset; i < count; i++)
    {
       ulong ptr = (_crc & 0xFF) ^ buffer[i];
          _crc >>= 8;
      _crc ^= _crc32Table[ptr];
}
        }

        protected override byte[] HashFinal()
        {
        var finalHash = new byte[4];
            ulong finalCRC = _crc ^ _allOnes;

  finalHash[0] = (byte)((finalCRC >> 0) & 0xFF);
            finalHash[1] = (byte)((finalCRC >> 8) & 0xFF);
            finalHash[2] = (byte)((finalCRC >> 16) & 0xFF);
            finalHash[3] = (byte)((finalCRC >> 24) & 0xFF);

            return finalHash;
        }

      public static int Compute(string asciiString)
      {
            _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(asciiString));
        }

        public static int Compute(Stream inputStream)
        {
   _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(inputStream));
        }

        public static int Compute(byte[] buffer)
        {
 _defaultCRC.Initialize();
    return ToInt32(_defaultCRC.ComputeHash(buffer));
     }

        public static int Compute(byte[] buffer, int offset, int count)
{
          _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(buffer, offset, count));
        }

        public byte[] ComputeHash(string asciiString)
   {
        byte[] rawBytes = Encoding.ASCII.GetBytes(asciiString);
            return ComputeHash(rawBytes);
        }

        public new byte[] ComputeHash(Stream inputStream)
        {
   var buffer = new byte[4096];
   int bytesRead;
      while ((bytesRead = inputStream.Read(buffer, 0, 4096)) > 0)
            {
     HashCore(buffer, 0, bytesRead);
   }
            return HashFinal();
  }

        public new byte[] ComputeHash(byte[] buffer)
   {
   return ComputeHash(buffer, 0, buffer.Length);
        }

      public new byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
   HashCore(buffer, offset, count);
    return HashFinal();
        }

        private static uint[] _buildCRC32Table(uint polynomial)
        {
            uint crc;
  var table = new uint[256];

     for (int i = 0; i < 256; i++)
  {
      crc = (uint)i;
                for (int j = 8; j > 0; j--)
           {
        if ((crc & 1) == 1)
        crc = (crc >> 1) ^ polynomial;
    else
    crc >>= 1;
}
         table[i] = crc;
}

       return table;
        }

        private static int ToInt32(byte[] buffer)
        {
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
