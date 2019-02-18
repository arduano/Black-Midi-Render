using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Black_Midi_Render
{
    class MemoryByteReader : IByteReader
    {
        byte[] bytes;
        long pos;
        public MemoryByteReader(byte[] data)
        {
            bytes = data;
        }

        public long Location => pos;

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
