using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiUtils;

namespace MidiUtils
{
    public class MemoryByteReader : IByteReader
    {
        byte[] bytes;
        long pos;
        public MemoryByteReader(byte[] data)
        {
            bytes = data;
        }

        public long Location => pos;

        public void Dispose()
        {
            bytes = null;
        }

        public byte Read() => bytes[pos++];

        public void Reset()
        {
            pos = 0;
        }

        public void Skip(int count)
        {
            pos += count;
        }
    }
}
