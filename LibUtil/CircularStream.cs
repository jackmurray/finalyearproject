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
        public int capacity;

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
            Console.WriteLine("Seek to " + offset + " from " + origin);
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
            int totalread = 0;
            Console.WriteLine("Read at " + head + " for " + count);
            int canread = ((capacity - head) > int.MaxValue ? int.MaxValue : (int)(capacity - head));
            int willread = Math.Min(canread, count);
            Array.Copy(Buffer, head, dest, 0, willread);
            head += willread;
            totalread += willread;
            count -= willread;

            if (count > 0)
            {
                head = 0;
                Array.Copy(Buffer, head, dest, offset + totalread, count); //and if there's any left copy that too
            }

            return totalread;
        }

        public override void Write(byte[] source, int offset, int count)
        {
            Console.WriteLine("Write at " + tail + " for " + count);
            int canwrite = ((capacity - tail) > int.MaxValue ? int.MaxValue : (int) (capacity - tail));
            int willwrite = Math.Min(canwrite, count);
            Array.Copy(source, offset, Buffer, tail, willwrite);
            tail += willwrite;
            count -= willwrite; //copy all that we can
            
            if (count > 0)
            {
                tail = 0;
                Array.Copy(source, offset + canwrite, Buffer, tail, count); //and if there's any left copy that too
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

        public override long Position { get { return head; } set { head = value;
            Console.WriteLine("Jumping to " + value);
        } }
    }
}
