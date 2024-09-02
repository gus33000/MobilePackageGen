namespace ToCBS.Wof
{
    public partial class WofDecompressorStream : Stream
    {
        private readonly Stream input;
        private readonly uint uncompressedSize;
        private long position = 0;

        public WofDecompressorStream(Stream input, uint uncompressedSize)
        {
            this.input = input;
            this.uncompressedSize = uncompressedSize;
        }

        public override bool CanRead => true;

        public override bool CanSeek => input.CanSeek;

        public override bool CanWrite => false;

        public override long Length => uncompressedSize;

        public override long Position
        {
            get => position;
            set => position = value;
        }

        public override void Flush()
        {
            // Nothing to do!
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position + count > uncompressedSize)
            {
                count = (int)uncompressedSize - (int)position;
            }

            input.Seek(0, SeekOrigin.Begin);
            Wof.WofDecompress(input, uncompressedSize, position, buffer, offset, count);

            position += count;
            
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > uncompressedSize)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset < 0)
                        {
                            throw new IOException();
                        }

                        position = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        int tempPosition = (int)position + (int)offset;
                        if (tempPosition < 0)
                        {
                            throw new IOException();
                        }

                        position = tempPosition;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        int tempPosition = (int)uncompressedSize + (int)offset;
                        if (tempPosition < 0)
                        {
                            throw new IOException();
                        }

                        position = tempPosition;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(nameof(origin));
                    }
            }

            return position;
        }

        public override void SetLength(long value)
        {
            // Nothing to do!
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Nothing to do!
        }
    }
}