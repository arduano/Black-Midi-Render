using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicRender
{
    public class Settings
    {
        public int firstNote = 0;
        public int lastNote = 128;
        public double pianoHeight = 0.2;
        public int deltaTimeOnScreen = 300;
        public bool sameWidthNotes = true;

        public float noteBrightness = 1;
    }
}
