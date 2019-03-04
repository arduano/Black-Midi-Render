using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFARender
{
    public enum TopColor
    {
        Red,
        Blue,
        Green
    }

    public class Settings
    {
        public int firstNote = 0;
        public int lastNote = 128;
        public double pianoHeight = 0.155;
        public double deltaTimeOnScreen = 294.067;
        public bool sameWidthNotes = false;
        public TopColor topColor = TopColor.Red;
        public bool middleC = false;
        public bool blackNotesAbove = true;

        public bool tickBased = true;
    }
}
