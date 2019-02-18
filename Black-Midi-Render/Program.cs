using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Black_Midi_Render
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            MidiFile f = new MidiFile(@"E:\Midi\Pi.mid");
            f.ParseAll();
            RenderWindow win = null;
            bool winStarted = false;
            Thread wintask = new Thread(() =>
            {
                win = new RenderWindow(1920, 1080, f, "out.mp4");
                winStarted = true;
                win.Run();
            });
            wintask.Start();
            SpinWait.SpinUntil(() => winStarted);
            long time = 0;
            int nc = -1;
            while (f.ParseUpTo(time += (long)(win.tempoFrameStep * 10)) || nc != 0)
            {
                SpinWait.SpinUntil(() => f.currentSyncTime < win.midiTime + (long)(win.tempoFrameStep * 10));
                nc = 0;
                Note n;
                //nc += f.globalDisplayNotes.Count();
                //f.globalDisplayNotes.Unlink();
                lock (f.globalDisplayNotes)
                {
                    var i = f.globalDisplayNotes.Iterate();
                    double cutoffTime = win.midiTime - win.deltaTimeOnScreen;
                    while (i.MoveNext(out n))
                    {
                        //nc++;
                        if (n.hasEnded && n.end < cutoffTime)
                            i.Remove();
                        else nc++;
                    }
                }
                Console.WriteLine(Math.Round((double)time / f.maxTrackTime * 10000) / 100 + "% Notes: " + nc);
            }
        }
    }
}
