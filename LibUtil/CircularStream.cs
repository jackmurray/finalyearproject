using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibUtil
{
    public class CircularStream : Stream
    {
        public byte[] Buffer { get; private set; }
        private long head = 0, tail = 0;
        private int capacity;

        public CircularStream(int capacity = 1048576) //default = 1MiB
        {
            this.capacity = capacity;
            Buffer = new byte[capacity];
        }

        public override void Flush()
        {
            return;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0 || offset > capacity)
                        throw new ArgumentOutOfRangeException();
                    
                    head = offset;
                    break;
                case SeekOrigin.Current:
                    if ((head + offset > capacity) || (head + offset < 0))
                        throw new ArgumentOutOfRangeException();

                    head += offset;
                    break;
                case SeekOrigin.End:
                    if ((head - offset < 0) || (head - offset > capacity))
                        throw new ArgumentOutOfRangeException();

                    head = capacity - offset;
                    break;
            }

            return head;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] dest, int offset, int count)
        {
            for (int i = 0; i < count; i++, offset++, head++)
            {
                if (head == capacity)
                    head = 0;
                dest[offset] = Buffer[head];
            }

            return count;
        }

        public override void Write(byte[] source, int offset, int count)
        {
            for (int i = 0; i < count; i++, offset++, tail++)
            {
                if (tail == capacity)
                    tail = 0;
                Buffer[tail] = source[offset];
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return capacity; } //not really meaningful, but NAudio's WaveFileWriter will try and update the RIFF/WAVE headers when it's closed, and we don't want it to crash (even though what it writes is meaningless for us).
        }

        public override long Position { get { return head; } set { head = value; } }
    }
}
