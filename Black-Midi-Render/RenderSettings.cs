using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Black_Midi_Render
{
    class RenderSettings
    {
        public string midiFile = @"E:\Midi\Pi.mid";

        public int firstNote = 0;
        public int lastNote = 128;
        public double pianoHeight = 0.2;
        public int deltaTimeOnScreen = 300;
        
        public int fps = 60;

        public int width = 1920;
        public int height = 1080;

        public bool ffRender = false;
        public string ffPath = "out.mp4";
        public bool imgRender = false;
        public string imgPath = "imgs";
        public bool vsync = false;

        public Color4[] keyColors;

        public void ResetVariableState()
        {
            keyColors = new Color4[256];
            for (int i = 0; i < 256; i++) keyColors[i] = Color4.Transparent;
        }
    }
}
