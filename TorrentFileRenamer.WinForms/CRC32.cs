using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Class CRC32
    /// </summary>
    public class CRC32 : HashAlgorithm
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Initializes static members of the <see cref="CRC32" /> class.
        /// </summary>
        static CRC32()
        {
            _crc32TablesCache = Hashtable.Synchronized(new Hashtable());
            _defaultCRC = new CRC32();
        }

        /// <summary>
        /// Creates a CRC32 object using the <see cref="DefaultPolynomial" />.
        /// </summary>
        public CRC32()
            : this(DefaultPolynomial)
        {
        }

        /// <summary>
        /// Creates a CRC32 object using the specified polynomial.
        /// </summary>
        /// <param name="polynomial">The polynomial.</param>
        /// <remarks>The polynomical should be supplied in its bit-reflected form. <see cref="DefaultPolynomial" />.</remarks>
        [CLSCompliant(false)]
        public CRC32(uint polynomial)
        {
            HashSizeValue = 32;
            _crc32Table = (uint[])_crc32TablesCache[polynomial];
            if (_crc32Table == null)
            {
                _crc32Table = _buildCRC32Table(polynomial);
                _crc32TablesCache.Add(polynomial, _crc32Table);
            }

            Initialize();
        }

        // static constructor

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the default polynomial (used in WinZip, Ethernet, etc.)
        /// </summary>
        [CLSCompliant(false)] public static readonly uint DefaultPolynomial = 0xEDB88320;
        // Bitwise reflection of 0x04C11DB7;

        #endregion

        #region METHODS

        /// <summary>
        /// Initializes an implementation of HashAlgorithm.
        /// </summary>
        public override void Initialize()
        {
            _crc = _allOnes;
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        protected override void HashCore(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < count; i++)
            {
                ulong ptr = (_crc & 0xFF) ^ buffer[i];
                _crc >>= 8;
                _crc ^= _crc32Table[ptr];
            }
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
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

        /// <summary>
        /// Computes the CRC32 value for the given ASCII string using the <see cref="DefaultPolynomial" />.
        /// </summary>
        /// <param name="asciiString">The ASCII string.</param>
        /// <returns>System.Int32.</returns>
        public static int Compute(string asciiString)
        {
            _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(asciiString));
        }

        /// <summary>
        /// Computes the CRC32 value for the given input stream using the <see cref="DefaultPolynomial" />.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <returns>System.Int32.</returns>
        public static int Compute(Stream inputStream)
        {
            _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(inputStream));
        }

        /// <summary>
        /// Computes the CRC32 value for the input data using the <see cref="DefaultPolynomial" />.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>System.Int32.</returns>
        public static int Compute(byte[] buffer)
        {
            _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(buffer));
        }

        /// <summary>
        /// Computes the hash value for the input data using the <see cref="DefaultPolynomial" />.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>System.Int32.</returns>
        public static int Compute(byte[] buffer, int offset, int count)
        {
            _defaultCRC.Initialize();
            return ToInt32(_defaultCRC.ComputeHash(buffer, offset, count));
        }

        /// <summary>
        /// Computes the hash value for the given ASCII string.
        /// </summary>
        /// <param name="asciiString">The ASCII string.</param>
        /// <returns>System.Byte[][].</returns>
        /// <remarks>The computation preserves the internal state between the calls, so it can be used for computation of a stream data.</remarks>
        public byte[] ComputeHash(string asciiString)
        {
            byte[] rawBytes = Encoding.ASCII.GetBytes(asciiString);
            return ComputeHash(rawBytes);
        }

        /// <summary>
        /// Computes the hash value for the given input stream.
        /// </summary>
        /// <param name="inputStream">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        /// <remarks>The computation preserves the internal state between the calls, so it can be used for computation of a stream data.</remarks>
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

        /// <summary>
        /// Computes the hash value for the input data.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        /// <remarks>The computation preserves the internal state between the calls, so it can be used for computation of a stream data.</remarks>
        public new byte[] ComputeHash(byte[] buffer)
        {
            return ComputeHash(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Computes the hash value for the input data.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash code.</returns>
        /// <remarks>The computation preserves the internal state between the calls, so it can be used for computation of a stream data.</remarks>
        public new byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            HashCore(buffer, offset, count);
            return HashFinal();
        }

        #endregion

        #region PRIVATE SECTION

        /// <summary>
        /// The _all ones
        /// </summary>
        private static uint _allOnes = 0xffffffff;

        /// <summary>
        /// The _default CRC
        /// </summary>
        private static readonly CRC32 _defaultCRC;

        /// <summary>
        /// The _CRC32 tables cache
        /// </summary>
        private static readonly Hashtable _crc32TablesCache;

        /// <summary>
        /// The _CRC32 table
        /// </summary>
        private readonly uint[] _crc32Table;

        /// <summary>
        /// The _CRC
        /// </summary>
        private uint _crc;

        // Builds a crc32 table given a polynomial
        /// <summary>
        /// _builds the CR C32 table.
        /// </summary>
        /// <param name="polynomial">The polynomial.</param>
        /// <returns>System.UInt32[][].</returns>
        private static uint[] _buildCRC32Table(uint polynomial)
        {
            uint crc;
            var table = new uint[256];

            // 256 values representing ASCII character codes. 
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

        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>System.Int32.</returns>
        private static int ToInt32(byte[] buffer)
        {
            return BitConverter.ToInt32(buffer, 0);
        }

        #endregion
    }
}