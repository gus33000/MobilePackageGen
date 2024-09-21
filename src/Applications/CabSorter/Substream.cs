namespace MobilePackageGen
{
    public class Substream : Stream
    {
        private readonly Stream stream;
        private readonly long length;

        public Substream(Stream stream, long length)
        {
            this.stream = stream;
            this.length = length;
        }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => length;

        public override long Position
        {
            get => stream.Position;
            set
            {
                if (value > length)
                {
                    value = length;
                }

                stream.Position = value;
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (stream.Position + count > length)
            {
                count = (int)(length - stream.Position);
            }

            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset > length)
                        {
                            offset = length;
                        }
                        break;
                    }
                case SeekOrigin.End:
                    {
                        offset -= (stream.Length - length);
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        if (stream.Position + offset > length)
                        {
                            offset = length - stream.Position;
                        }
                        break;
                    }
            }

            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (value > length)
            {
                value = length;
            }

            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (stream.Position + count > length)
            {
                count = (int)(length - stream.Position);
            }

            stream.Write(buffer, offset, count);
        }
    }
}
