using StorageSpace.Data.Subtypes;

namespace StorageSpace
{
    public class OSPoolStream : Stream
    {
        private readonly Stream stream;
        private readonly Dictionary<int, Disk> parsedDisks = [];
        private readonly Disk store;
        private readonly long length;
        private readonly long blockSize = 0x100000;
        private readonly Dictionary<long, int> blockTable;
        private readonly StorageSpace storageSpace;

        private long currentPosition = 0;

        private readonly long ogSeek;

        public OSPoolStream(Stream stream, ulong storeIndex)
        {
            this.stream = stream;

            storageSpace = new(stream);

            PhysicalDisk.ParseEntryType2(storageSpace.SDBBPhysicalDisks, parsedDisks);
            Volume.ParseEntryType3(storageSpace.SDBBVolumes, parsedDisks);
            SlabAllocation.ParseEntryType4(storageSpace.SDBBSlabAllocation, parsedDisks);

            store = parsedDisks[(int)storeIndex];

            (length, blockTable) = BuildBlockTable();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        private (long, Dictionary<long, int>) BuildBlockTable()
        {
            Dictionary<long, int> blockTable = [];

            long blockSize = 0x10000000;

            foreach (DataEntry sdbbEntry in store.sdbbEntryType4)
            {
                int virtualDiskBlockNumber = sdbbEntry.virtual_disk_block_number;
                int physicalDiskBlockNumber = sdbbEntry.physical_disk_block_number;

                blockTable.Add(virtualDiskBlockNumber, physicalDiskBlockNumber);
            }

            long totalBlocks = store.TotalBlocks;

            return (totalBlocks * blockSize, blockTable);
        }

        private int GetBlockDataIndex(long realBlockOffset)
        {
            if (blockTable.TryGetValue(realBlockOffset, out int value))
            {
                return value;
            }

            return -1; // Invalid
        }

        public override long Position
        {
            get => currentPosition;
            set
            {
                if (currentPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                // Workaround for malformed MBRs
                /*if (currentPosition > Length)
                {
                    throw new EndOfStreamException();
                }*/

                currentPosition = value;
            }
        }

        public override void Flush()
        {
            // Nothing to do here
        }

        private byte[] ImageGetStoreDataBlock(int physicalDiskBlockNumber)
        {
            byte[] buffer = new byte[blockSize];

            long physicalDiskLocation = physicalDiskBlockNumber * blockSize + 0x2000 + 0x4000000;

            stream.Seek(ogSeek + physicalDiskLocation, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // Workaround for malformed MBRs
            if (Position >= Length)
            {
                return count;
            }

            long readBytes = count;

            if (Position + readBytes > Length)
            {
                readBytes = (int)(Length - Position);
            }

            byte[] readBuffer = new byte[readBytes];
            Array.Fill<byte>(readBuffer, 0);

            // Read the buffer from the FFU file.
            // First we have to figure out where do we land here.

            long overflowBlockStartByteCount = Position % blockSize;
            long overflowBlockEndByteCount = (Position + readBytes) % blockSize;

            long startBlockIndex = (Position - overflowBlockStartByteCount) / blockSize;
            long endBlockIndex = (Position + readBytes + (blockSize - overflowBlockEndByteCount)) / blockSize;

            byte[] allReadBlocks = new byte[(endBlockIndex - startBlockIndex + 1) * blockSize];

            for (long currentBlock = startBlockIndex; currentBlock < endBlockIndex; currentBlock++)
            {
                int virtualBlockIndex = GetBlockDataIndex(currentBlock);
                if (virtualBlockIndex != -1)
                {
                    byte[] block = ImageGetStoreDataBlock(virtualBlockIndex);
                    Array.Copy(block, 0, allReadBlocks, (int)((currentBlock - startBlockIndex) * blockSize), blockSize);
                }
            }

            Array.Copy(allReadBlocks, overflowBlockStartByteCount, buffer, offset, readBytes);

            Position += readBytes;

            if (Position == Length)
            {
                // Workaround for malformed MBRs
                //return 0;
            }

            return (int)readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        Position = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        Position += offset;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        Position = Length + offset;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(nameof(origin));
                    }
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            stream.Dispose();
        }
    }
}
