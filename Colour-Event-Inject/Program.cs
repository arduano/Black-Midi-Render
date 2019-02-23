using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MidiUtils;

namespace Colour_Event_Inject
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new ColourForm());

            var midiout = new StreamWriter(@"E:\test.mid");
            var midiin = new StreamReader(@"E:\Midi\Pi.mid");
            MidiWriter writer = new MidiWriter(midiout.BaseStream);
            writer.Init();

            var filter = new TrackFilter();


            MidiFileInfo info = MidiFileInfo.Parse(midiin.BaseStream);

            for (int i = 0; i < info.TrackCount; i++)
            {
                byte[] trackbytes = new byte[info.Tracks[i].Length];
                midiin.BaseStream.Position = info.Tracks[i].Start;
                midiin.BaseStream.Read(trackbytes, 0, (int)info.Tracks[i].Length);
                writer.InitTrack();
                bool added = false;
                byte[] e = new byte[] { 0, 0xFF, 0x7F, 0x08, 0x00, 0x0F, 0x7F, 0x00, 0x00, 0x00, 0xFF, 0x80 };
                filter.MidiEventFilter = (byte[] dtimeb, int dtime, byte[] data, long time) =>
                {
                    if((data[0] & 0xF0) == 0b10000000 || (data[0] & 0xF0) == 0b10010000)
                    {
                        data[1] = 3;
                    }
                    if (!added)
                    {
                        added = true;
                        return e.Concat(dtimeb.Concat(data)).ToArray();
                    }
                    return dtimeb.Concat(data).ToArray();
                };
                var newtrackbytes = filter.FilterTrack(new MemoryByteReader(trackbytes));
                writer.Write(newtrackbytes);
                writer.EndTrack();
            }

            writer.Close();
        }
    }
}
