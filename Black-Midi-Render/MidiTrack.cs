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

    interface IByteReader : IDisposable
    {
        byte Read();
        void Reset();
        void Skip(int count);
        long Location
        {
            get;
        }
    }

    class MidiTrack : IDisposable
    {
        public int trackID;

        public bool trackEnded = false;

        public long trackTime = 0;

        byte channelPrefix = 0;

        public FastList<Note>[] UnendedNotes = new FastList<Note>[256 * 16];
        public LinkedList<Tempo> Tempos = new LinkedList<Tempo>();

        FastList<Note> globalDisplayNotes;
        FastList<Tempo> globalTempoEvents;

        public Color4[] trkColor;

        bool readDelta = false;

        IByteReader reader;

        public void Reset()
        {
            foreach (var un in UnendedNotes) un.Unlink();
            reader.Reset();
            ResetColors();
            trackTime = 0;
            trackEnded = false;
            readDelta = false;
            channelPrefix = 0;
        }

        public void ResetColors()
        {
            trkColor = new Color4[32];
            for (int i = 0; i < 16; i++)
            {
                trkColor[i * 2] = Color4.FromHsv(new OpenTK.Vector4((trackID * 16 + i) * 1.36271f % 1, 1.0f, 1.0f, 1f));
                trkColor[i * 2 + 1] = Color4.FromHsv(new OpenTK.Vector4((trackID * 16 + i) * 1.36271f % 1, 1.0f, 1.0f, 1f));
            }
        }

        public MidiTrack(int id, IByteReader reader, FastList<Note> globalNotes, FastList<Tempo> globalTempos)
        {
            globalDisplayNotes = globalNotes;
            globalTempoEvents = globalTempos;
            this.reader = reader;
            trackID = id;
            for (int i = 0; i < 256 * 16; i++)
            {
                UnendedNotes[i] = new FastList<Note>();
            }
            ResetColors();
        }

        long ReadVariableLen()
        {
            byte c;
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                c = reader.Read();
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

        void EndTrack()
        {
            trackEnded = true;

            foreach (var un in UnendedNotes)
            {
                var iter = un.Iterate();
                Note n;
                while (iter.MoveNext(out n))
                {
                    n.end = trackTime;
                    n.hasEnded = true;
                }
                un.Unlink();
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
                byte command = reader.Read();
                byte comm = (byte)(command & 0b11110000);
                if (comm == 0b10010000)
                {
                    byte channel = (byte)(command & 0b00001111);
                    byte note = reader.Read();
                    byte vel = reader.Read();
                    if (vel == 0)
                    {
                        if (!readOnly)
                            try
                            {
                                var q = UnendedNotes[note << 4 | channel];
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
                            UnendedNotes[note << 4 | channel].Add(n);
                            globalDisplayNotes.Add(n);
                        }
                    }
                }
                else if (comm == 0b10000000)
                {
                    int channel = command & 0b00001111;
                    byte note = reader.Read();
                    byte vel = reader.Read();

                    if (!readOnly)
                        try
                        {
                            var q = UnendedNotes[note << 4 | channel];
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
                    byte note = reader.Read();
                    byte vel = reader.Read();
                }
                else if (comm == 0b10100000)
                {
                    int channel = command & 0b00001111;
                    byte number = reader.Read();
                    byte value = reader.Read();
                }
                else if (comm == 0b11000000)
                {
                    int channel = command & 0b00001111;
                    byte program = reader.Read();
                }
                else if (comm == 0b11010000)
                {

                    int channel = command & 0b00001111;
                    byte pressure = reader.Read();
                }
                else if (comm == 0b11100000)
                {
                    int channel = command & 0b00001111;
                    byte l = reader.Read();
                    byte m = reader.Read();
                }
                else if (comm == 0b10110000)
                {
                    int channel = command & 0b00001111;
                    byte cc = reader.Read();
                    byte vv = reader.Read();
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
                    byte ll = reader.Read();
                    byte mm = reader.Read();

                }
                else if (command == 0b11110011)
                {
                    byte ss = reader.Read();
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
                    command = reader.Read();
                    if (command == 0x00)
                    {
                        if (reader.Read() != 0)
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
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x02)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x03)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x04)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x05)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x06)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x07)
                    {
                        int size = (int)ReadVariableLen();
                        char[] text = new char[size];
                        for (int i = 0; i < size; i++)
                        {
                            text[i] = (char)reader.Read();
                        }
                        string str = new string(text);
                    }
                    else if (command == 0x20)
                    {
                        command = reader.Read();
                        if (command != 1)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        channelPrefix = reader.Read();
                    }
                    else if (command == 0x21)
                    {
                        command = reader.Read();
                        if (command != 1)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        reader.Skip(1);
                        //TODO:  MIDI port
                    }
                    else if (command == 0x2F)
                    {
                        command = reader.Read();
                        if (command != 0)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        EndTrack();
                    }
                    else if (command == 0x51)
                    {
                        command = reader.Read();
                        if (command != 3)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        int btempo = 0;
                        for (int i = 0; i != 3; i++)
                            btempo = (int)((btempo << 8) | reader.Read());
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
                        command = reader.Read();
                        if (command != 5)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        reader.Skip(4);
                    }
                    else if (command == 0x58)
                    {
                        command = reader.Read();
                        if (command != 4)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        reader.Skip(4);
                    }
                    else if (command == 0x59)
                    {
                        command = reader.Read();
                        if (command != 2)
                        {
                            throw new Exception("Corrupt Track");
                        }
                        reader.Skip(2);
                        //TODO: Key Signature
                    }
                    else if (command == 0x7F)
                    {
                        int size = (int)ReadVariableLen();
                        byte[] data = new byte[size];
                        for (int i = 0; i < size; i++)
                        {
                            data[i] = reader.Read();
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
                EndTrack();
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
