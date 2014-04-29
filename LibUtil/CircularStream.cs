using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LibUtil
{
    public class CircularStream : Stream
    {
        public byte[] Buffer { get; private set; }
        private long head = 0, tail = 0;
        private Queue<byte[]> buffer = new Queue<byte[]>();
        private int targetsize;
        private byte[] leftover = new byte[0];
        private byte[] workingarr = new byte[100000];

        public CircularStream(int targetsize)
        {
            this.targetsize = targetsize;
        }

        public override void Flush()
        {
            return;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] dest, int offset, int count)
        {
            while (buffer.Count == 0)
            {
                Console.WriteLine("circbuf empty");
                Thread.Sleep(1);                
            }

            byte[] b;
            lock (buffer)
            {
                b = buffer.Dequeue();
            }
            Array.Copy(b, dest, targetsize);
            return targetsize;
        }

        public override void Write(byte[] source, int offset, int count)
        {
            Array.Copy(leftover, workingarr, leftover.Length);
            Array.Copy(source, offset, workingarr, leftover.Length, count);
            int actualcount = count + leftover.Length;

            lock (buffer)
            {
                for (int i = 0; i < actualcount / targetsize; i++)
                    buffer.Enqueue(source.Skip(i*targetsize).Take(targetsize).ToArray());
            }

            leftover = workingarr.Skip(actualcount / targetsize).Take(actualcount % targetsize).ToArray();
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
            get { return 0; } //not really meaningful, but NAudio's WaveFileWriter will try and update the RIFF/WAVE headers when it's closed, and we don't want it to crash (even though what it writes is meaningless for us).
        }

        public override long Position { get { return head; } set { head = value;
            Console.WriteLine("Jumping to " + value);
        } }
    }
}
