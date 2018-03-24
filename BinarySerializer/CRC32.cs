using System;
using System.Collections.Generic;
using System.IO;

namespace BinarySerializer
{
    sealed class Crc32
    {
        public const UInt32 DefaultPolynomial = 0xedb88320u;
        public const UInt32 DefaultSeed = 0xffffffffu;

        static UInt32[] defaultTable;

        byte[] HashValue;
        readonly UInt32 seed;
        readonly UInt32[] table;
        UInt32 hash;

        public Crc32()
            : this(DefaultPolynomial, DefaultSeed)
        {
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = hash = seed;
        }

        public void Initialize()
        {
            hash = seed;
        }

        void HashCore(byte[] array, int ibStart, int cbSize)
        {
            hash = CalculateHash(table, hash, array, ibStart, cbSize);
        }

        byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public int HashSize { get { return 32; } }

        public static UInt32 Compute(byte[] buffer)
        {
            return Compute(DefaultSeed, buffer);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(Stream stream)
        {
            Crc32 crc32 = new Crc32();
            return BitConverter.ToUInt32(crc32.ComputeHash(stream), 0);
        }

        public void HashBlock(Stream inputStream)
        {
            // Default the buffer size to 4K.
            byte[] buffer = new byte[4096];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, 4096);
                if (bytesRead > 0)
                {
                    HashCore(buffer, 0, bytesRead);
                }
            } while (bytesRead > 0);
        }

        public void HashBlock(byte[] data,int start,int size)
        {
            HashCore(data, start, size);
        }

        public UInt32 FinalBlockAndHash()
        {
            HashValue = HashFinal();
            byte[] Tmp = (byte[])HashValue.Clone();
            Initialize();
            return BitConverter.ToUInt32(Tmp,0);
        }

        public byte[] ComputeHash(Stream inputStream)
        {
            // Default the buffer size to 4K.
            byte[] buffer = new byte[4096];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, 4096);
                if (bytesRead > 0)
                {
                    HashCore(buffer, 0, bytesRead);
                }
            } while (bytesRead > 0);

            HashValue = HashFinal();
            byte[] Tmp = (byte[])HashValue.Clone();
            Initialize();
            return (Tmp);
        }

        static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new UInt32[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (UInt32)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
        {
            var hash = seed;
            for (var i = start; i < start + size; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
            return hash;
        }

        static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }

    sealed class Crc32Stream : Stream
    {
        Stream internalStream;
        Crc32 crc32;

        public Crc32Stream(Stream stream)
        {
            internalStream = stream;
            crc32 = new Crc32();
        }

        public override bool CanRead
        {
            get
            {
                return internalStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return internalStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return internalStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return internalStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return internalStream.Position;
            }

            set
            {
                internalStream.Position = value;
            }
        }

        public override void Flush()
        {
            internalStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            crc32.HashBlock(buffer, offset, count);
            return internalStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return internalStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            internalStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            crc32.HashBlock(buffer, offset, count);
            internalStream.Write(buffer, offset, count);
        }

        public uint GetHash()
        {
            internalStream.Flush();
            return crc32.FinalBlockAndHash();
        }

        public Stream SourceStream
        {
            get
            {
                return internalStream;
            }
        }
    }
}