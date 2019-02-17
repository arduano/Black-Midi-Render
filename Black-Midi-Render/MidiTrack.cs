using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Black_Midi_Render
{
    class Note
    {
        public long start;
        public long end;
        public bool hasEnded;
        public byte channel;
        public byte note;
        public byte vel;
        public MidiTrack track;
    }

    class Tempo
    {
        public long pos;
        public int tempo;
    }

    class MidiTrack
    {
        byte[] bytes;
        public int trackID;

        public bool trackEnded = false;

        public long trackTime = 0;

        byte channelPrefix = 0;

        public FastList<Note>[] UnendedNotes = new FastList<Note>[256 * 16];
        public LinkedList<Tempo> Tempos = new LinkedList<Tempo>();

        FastList<Note> globalDisplayNotes;
        FastList<Tempo> globalTempoEvents;

        long readpos = 0;

        public Color4 trkColor;

        bool readDelta = false;

        public void Reset()
        {
            readpos = 0;
            trackTime = 0;
            trackEnded = false;
            channelPrefix = 0;
        }

        public MidiTrack(int id, byte[] bytes, FastList<Note> globalNotes, FastList<Tempo> globalTempos)
        {
            globalDisplayNotes = globalNotes;
            globalTempoEvents = globalTempos;
            this.bytes = bytes;
            trackID = id;
            for (int i = 0; i < 256 * 16; i++)
            {
                UnendedNotes[i] = new FastList<Note>();
            }
            trkColor = Color4.FromHsv(new OpenTK.Vector4(id * 1.36271f % 1, 1.0f, 1.0f, 1));
        }

        long ReadVariableLen()
        {
            byte c;
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                c = bytes[readpos++];
                if (c > 127)
                {
                    val = (c - 128) + val * 128;
                }
                else
                {
                    val = val * 128 + c;
                    return val;
                }
            }
            return val;
        }

        public void Step(long time)
        {

            if (time >= trackTime)
            {
                if (readDelta)
                {
                    long d = trackTime;
                    do
                    {
                        ParseNextEvent();
                        if (trackEnded) return;
                        trackTime += ReadVariableLen();
                        readDelta = true;
                    }
                    while (trackTime == d);
                    }
                else
                {
                    if (trackEnded) return;
                    trackTime += ReadVariableLen();
                    readDelta = true;
                }
            }
        }

        public void ParseNextEvent(bool readOnly = false)
        {
            try
            {
                if (!readDelta)
                {
                    trackTime += ReadVariableLen();
                }
                readDelta = false;
                byte command = bytes[readpos++];
                byte comm = (byte)(command & 0b11110000);
                if (comm == 0b10010000)
                {
                    byte channel = (byte)(command & 0b00001111);
                    byte note = bytes[readpos++];
                    byte vel = bytes[readpos++];
                    if (vel == 0)
                    {
                        if (!readOnly)
                            try
                            {
                                var q = UnendedNotes[channel * note];
                                var iter = q.Iterate();
                                Note n;
                                while (iter.MoveNext(out n))
                                {
                                    n.end = trackTime;
                                    n.hasEnded = true;
                                }
                                q.Unlink();
                            }
                            catch { }
                    }
                    else
                    {
                        Note n = new Note();
                        n.start = trackTime;
                        n.note = note;
                        n.vel = vel;
                        n.track = this;
                        n.channel = channel;
                        if (!readOnly)
                        {
                            UnendedNotes[channel * note].Add(n);
                            globalDisplayNotes.Add(n);
                        }
                    }
                }
                else if (comm == 0b10000000)
                {
                    int channel = command & 0b00001111;
                    byte note = bytes[readpos++];
                    byte vel = bytes[readpos++];

                    if (!readOnly)
                        try
                        {
                            var q = UnendedNotes[channel * note];
                            var iter = q.Iterate();
                            Note n;
                            while (iter.MoveNext(out n))
                            {
                                n.end = trackTime;
                                n.hasEnded = true;
                            }
                            q.Unlink();
                        }
                        catch { }
                }
                else if (comm == 0b10100000)
                {
                    int channel = command & 0b00001111;
                    byte note = bytes[readpos++];
                    byte vel = bytes[readpos++];
                }
                else if (comm == 0b10100000)
                {
                    int channel = command & 0b00001111;
                    byte number = bytes[readpos++];
                    byte value = bytes[readpos++];
                }
                else if (comm == 0b11000000)
                {
                    int channel = command & 0b00001111;
                    byte program = bytes[readpos++];
                }
                else if (comm == 0b11010000)
                {

                    int channel = command & 0b00001111;
                    byte pressure = bytes[readpos++];
                }
                else if (comm == 0b11100000)
                {
                    int channel = command & 0b00001111;
                    byte l = bytes[readpos++];
                    byte m = bytes[readpos++];
                }
                else if (comm == 0b10110000)
                {
                    int channel = command & 0b00001111;
                    byte cc = bytes[readpos++];
                    byte vv = bytes[readpos++];
                }
                else if (command == 0b11110000)
                {
                }
                else if (command == 0b11110100 || command == 0b11110001 || command == 0b11110101 || command == 0b11111001 || command == 0b11111101)
                {
                    //printf("Undefined\n");
                }
                else if (command == 0b11110010)
                {
                    int channel = command & 0b00001111;
                    byte ll = bytes[readpos++];
                    byte mm = bytes[readpos++];

                }
                else if (command == 0b11110011)
                {
                    byte ss = bytes[readpos++];
                }
                else if (command == 0b11110110)
                {
                }
                else if (command == 0b11110111)
                {
                }
                else if (command == 0b11111000)
                {
                }
                else if (command == 0b11111010)
                {
                }
                else if (command == 0b11111100)
                {
                }
                else if (command == 0b11111110)
                {
                }
                else if (command == 0xFF)
                {
                    command = bytes[readpos++];
                    if (command == 0x00)
                    {
                        if (bytes[readpos++] != 0)
                        {
                            throw new Exception("Corrupt Track");
                        }
                    }
                    else if (command == 0x01)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x02)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x03)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x04)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x05)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x06)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x07)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)bytes[readpos++];
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x20)
                    {
                        command = bytes[readpos++];
                        if (command != 1)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        channelPrefix = bytes[readpos++];
                    }
                    else if (command == 0x21)
                    {
                        command = bytes[readpos++];
                        if (command != 1)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        readpos += 1;
                        //TODO:  MIDI port
                    }
                    else if (command == 0x2F)
                    {
                        command = bytes[readpos++];
                        if (command != 0)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        trackEnded = true;
                    }
                    else if (command == 0x51)
                    {
                        command = bytes[readpos++];
                        if (command != 3)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        int btempo = 0;
                        for (int i = 0; i != 3; i++)
                            btempo = (int)((btempo << 8) | bytes[readpos++]);
                        Tempo t = new Tempo();
                        t.pos = trackTime;
                        t.tempo = btempo;

                        if (!readOnly)
                        {
                            globalTempoEvents.Add(t);
                        }
                    }
                    else if (command == 0x54)
                    {
                        command = bytes[readpos++];
                        if (command != 5)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        readpos += 4;
                    }
                    else if (command == 0x58)
                    {
                        command = bytes[readpos++];
                        if (command != 4)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        readpos += 4;
                    }
                    else if (command == 0x59)
                    {
                        command = bytes[readpos++];
                        if (command != 2)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        readpos += 2;
                        //TODO: Key Signature
                    }
                    else if (command == 0x7F)
                    {
                        int size = (int)ReadVariableLen();
                        byte[] data = new byte[size];
                        for (int i = 0; i < size; i++)
                        {
                            data[i] = bytes[readpos++];
                        }
                    }
                    else
                    {
                        throw new Exception("Corrupt Track");
                    }
                }
                else
                {
                    throw new Exception("Corrupt Track");
                }
            }
            catch
            {
                trackEnded = true;
            }
        }
    }
}
