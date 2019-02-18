using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Black_Midi_Render
{
    class MidiFile : IDisposable
    {
        Stream MidiFileReader;
        public ushort division;
        public ushort trackcount;
        public ushort format;

        List<long> trackBeginnings = new List<long>();
        List<uint> trackLengths = new List<uint>();

        public MidiTrack[] tracks;

        public long maxTrackTime;

        public long currentSyncTime = 0;

        public FastList<Note> globalDisplayNotes = new FastList<Note>();
        public FastList<Tempo> globalTempoEvents = new FastList<Tempo>();

        public int unendedTracks = 0;

        public MidiFile(string filename)
        {
            MidiFileReader = new StreamReader(filename).BaseStream;
            ParseHeaderChunk();
            while (MidiFileReader.Position < MidiFileReader.Length) ParseTrackChunk();
            tracks = new MidiTrack[trackcount];

            Console.WriteLine("Loading tracks into memory");
            for (int i = 0; i < trackcount; i++)
            {
                byte[] trackbytes = new byte[trackLengths[i]];
                MidiFileReader.Position = trackBeginnings[i];
                MidiFileReader.Read(trackbytes, 0, (int)trackLengths[i]);
                tracks[i] = new MidiTrack(i, trackbytes, globalDisplayNotes, globalTempoEvents);
            }
            Console.WriteLine("Loaded!");
            unendedTracks = trackcount;
        }

        void AssertText(string text)
        {
            foreach (char c in text)
            {
                if (MidiFileReader.ReadByte() != c)
                {
                    throw new Exception("Corrupt chunk headers");
                }
            }
        }

        uint ReadInt32()
        {
            uint length = 0;
            for (int i = 0; i != 4; i++)
                length = (uint)((length << 8) | (byte)MidiFileReader.ReadByte());
            return length;
        }

        ushort ReadInt16()
        {
            ushort length = 0;
            for (int i = 0; i != 2; i++)
                length = (ushort)((length << 8) | (byte)MidiFileReader.ReadByte());
            return length;
        }

        void ParseHeaderChunk()
        {
            AssertText("MThd");
            uint length = ReadInt32();
            if (length != 6) throw new Exception("Header chunk size isn't 6");
            format = ReadInt16();
            ReadInt16();
            division = ReadInt16();
            if (format == 2) throw new Exception("Midi type 2 not supported");
            if (division < 0) throw new Exception("Division < 0 not supported");
        }

        void ParseTrackChunk()
        {
            AssertText("MTrk");
            uint length = ReadInt32();
            trackBeginnings.Add(MidiFileReader.Position);
            trackLengths.Add(length);
            MidiFileReader.Position += length;
            trackcount++;
            Console.WriteLine("Track " + trackcount + ", Size " + length);
        }


        public bool ParseUpTo(long targetTime)
        {
            for (; currentSyncTime <= targetTime; currentSyncTime++)
            {
                int ut = 0;
                for (int trk = 0; trk < trackcount; trk++)
                {
                    var t = tracks[trk];
                    if (!t.trackEnded) ut++;
                    //while (t.trackTime < targetTime && !t.trackEnded)
                    {
                        t.Step(currentSyncTime);
                    }
                }
                unendedTracks = ut;
            }
            foreach (var t in tracks)
            {
                if (!t.trackEnded) return true;
            }
            return false;
        }

        public void ParseAll()
        {
            Parallel.For(0, tracks.Length, (i) =>
            {
                //Console.WriteLine("Opening track " + _p + "/" + tracks.Length);
                var t = tracks[i];
                while (!t.trackEnded)
                {
                    try
                    {
                        t.ParseNextEvent(true);
                    }
                    catch
                    {
                        break;
                    }
                }
                Console.WriteLine("Scanned track " + i + "/" + tracks.Length);
            });
            maxTrackTime = 0;
            foreach (var t in tracks)
            {
                if (maxTrackTime < t.trackTime) maxTrackTime = t.trackTime;
                t.Reset();
            }
            unendedTracks = trackcount;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
