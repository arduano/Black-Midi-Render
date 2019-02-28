using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BMEngine;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Black_Midi_Render
{
    interface INoteRender : IDisposable
    {
        long Render(FastList<Note> notes, double midiTime);
    }

    interface IKeyboardRender : IDisposable
    {
        void Render();
    }

    class RenderWindow : GameWindow
    {
        FastList<Note> globalDisplayNotes;
        FastList<Tempo> globalTempoEvents;
        FastList<ColorChange> globalColorEvents;
        MidiFile midi;

        RenderSettings settings;

        public double midiTime = 0;
        public double tempoFrameStep = 10;

        Process ffmpeg = new Process();
        long imgnumber = 0;
        Task lastRenderPush = null;

        GLPostbuffer finalCompositeBuff;

        int postShader;

        byte[] pixels;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }
        
        CurrentRendererPointer render;
        public RenderWindow(CurrentRendererPointer renderer, MidiFile midi, RenderSettings settings) : base((int)(DisplayDevice.Default.Width / 1.5), (int)(DisplayDevice.Default.Height / 1.5), new GraphicsMode(new ColorFormat(8, 8, 8, 8)), "Render", GameWindowFlags.Default, DisplayDevice.Default)
        {
            render = renderer;
            this.settings = settings;
            midiTime = -settings.deltaTimeOnScreen;
            pixels = new byte[settings.width * settings.height * 4];

            //WindowBorder = WindowBorder.Hidden;
            globalDisplayNotes = midi.globalDisplayNotes;
            globalTempoEvents = midi.globalTempoEvents;
            globalColorEvents = midi.globalColorEvents;
            this.midi = midi;
            tempoFrameStep = ((double)96 / 50000) * (1000000 / settings.fps);
            if (settings.ffRender)
            {
                string args = "";
                if (settings.includeAudio)
                {
                    args = "" +
                        " -f rawvideo -s " + settings.width + "x" + settings.height +
                        " -pix_fmt rgb32 -r " + settings.fps + " -i -" +
                        " -itsoffset 0.21 -i \"" + settings.audioPath + "\"" +
                        " -vf vflip -vcodec libx264 -acodec aac";


                }
                else
                {
                    args = "" +
                        " -f rawvideo -s " + settings.width + "x" + settings.height +
                        " -pix_fmt rgb32 -r " + settings.fps + " -i -" +
                        " -vf vflip -vcodec libx264";
                }
                if (settings.useBitrate)
                {
                    args += " -b:v " + settings.bitrate + "k" +
                        " -maxrate " + settings.bitrate + "k" +
                        " -minrate " + settings.bitrate + "k";
                }
                else
                {
                    args += " -preset " + settings.crfPreset + " -crf " + settings.crf;
                }
                args += " -y \"" + settings.ffPath + "\"";
                ffmpeg.StartInfo = new ProcessStartInfo("ffmpeg", args);
                ffmpeg.StartInfo.RedirectStandardInput = true;
                ffmpeg.StartInfo.UseShellExecute = false;
                try
                {
                    ffmpeg.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error starting the ffmpeg process\nNo video will be written\n(Is ffmpeg.exe in the same folder as this program?)\n\n\"" + ex.Message + "\"");
                    settings.ffRender = false;
                }
            }
            else if (settings.imgRender)
            {
                if (!Directory.Exists(settings.imgPath))
                {
                    Directory.CreateDirectory(settings.imgPath);
                }
            }
            else
            {
                if (!settings.vsync) VSync = VSyncMode.Off;
            }

            finalCompositeBuff = new GLPostbuffer(settings);
            postShader = GLUtils.MakePostShaderProgram("post");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            long nc = -1;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (settings.running && (midiTime < midi.maxTrackTime + settings.deltaTimeOnScreen + settings.fps * tempoFrameStep * 5 || midi.unendedTracks != 0))
            {
                SpinWait.SpinUntil(() => midi.currentSyncTime > midiTime + tempoFrameStep || midi.unendedTracks == 0 || !settings.running);
                if (!settings.running) break;

                if (!settings.paused || settings.forceReRender)
                {
                    lock (render)
                    {
                        try
                        {
                            while (true)
                            {
                                var r = render.disposeQueue.Dequeue();
                                if(!r.Initialized) r.Dispose();
                            }
                        }
                        catch (InvalidOperationException) { }
                        if (!render.renderer.Initialized) render.renderer.Init();
                        render.renderer.RenderFrame(globalDisplayNotes, midiTime, finalCompositeBuff.BufferID);
                    }
                }
                double mv = 1;
                lock (globalTempoEvents)
                {
                    while (globalTempoEvents.First != null && midiTime + (tempoFrameStep * mv * settings.tempoMultiplier) - settings.deltaTimeOnScreen > globalTempoEvents.First.pos)
                    {
                        var t = globalTempoEvents.Pop();
                        var _t = ((t.pos + settings.deltaTimeOnScreen) - midiTime) / (tempoFrameStep * mv * settings.tempoMultiplier);
                        mv *= 1 - _t;
                        tempoFrameStep = ((double)midi.division / t.tempo) * (1000000.0 / settings.fps);
                        midiTime = t.pos + settings.deltaTimeOnScreen;
                    }
                }
                if (!settings.paused)
                {
                    midiTime += mv * tempoFrameStep * settings.tempoMultiplier;
                }

                while (globalColorEvents.First != null && globalColorEvents.First.pos + settings.deltaTimeOnScreen < midiTime)
                {
                    var c = globalColorEvents.Pop();
                    var track = c.track;
                    if (c.channel == 0x7F)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            c.track.trkColor[i * 2] = c.col1;
                            c.track.trkColor[i * 2 + 1] = c.col2;
                        }
                    }
                    else
                    {
                        c.track.trkColor[c.channel * 2] = c.col1;
                        c.track.trkColor[c.channel * 2 + 1] = c.col2;
                    }
                }

                if (settings.ffRender)
                {
                    finalCompositeBuff.BindBuffer();
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pixels.Length);
                    GL.ReadPixels(0, 0, settings.width, settings.height, PixelFormat.Bgra, PixelType.UnsignedByte, unmanagedPointer);
                    Marshal.Copy(unmanagedPointer, pixels, 0, pixels.Length);
                    if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                    lastRenderPush = Task.Run(() => ffmpeg.StandardInput.BaseStream.Write(pixels, 0, pixels.Length));
                    Marshal.FreeHGlobal(unmanagedPointer);
                }
                if (settings.imgRender)
                {
                    finalCompositeBuff.BindBuffer();
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pixels.Length);
                    GL.ReadPixels(0, 0, settings.width, settings.height, PixelFormat.Bgra, PixelType.UnsignedByte, unmanagedPointer);
                    if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                    lastRenderPush = Task.Run(() =>
                    {
                        Bitmap output = new Bitmap(settings.width, settings.height, settings.width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, unmanagedPointer);
                        output.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        output.Save(settings.imgPath + "\\img_" + imgnumber++ + ".png");
                        output.Dispose();
                        Marshal.FreeHGlobal(unmanagedPointer);
                    });
                }

                GL.UseProgram(postShader);
                GLPostbuffer.UnbindBuffers();
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Viewport(0, 0, Width, Height);
                finalCompositeBuff.BindTexture();
                DrawScreenQuad();
                GLPostbuffer.UnbindTextures();
                if(settings.ffRender) VSync = VSyncMode.Off;
                else if (settings.vsync) VSync = VSyncMode.On;
                else VSync = VSyncMode.Off;
                try
                {
                    SwapBuffers();
                }
                catch
                {
                    break;
                }
                ProcessEvents();
                settings.notesOnScreen = nc;
                double fr = 10000000.0 / watch.ElapsedTicks;
                settings.liveFps = (settings.liveFps * 2 + fr) / 3;
                watch.Reset();
                watch.Start();
            }
            settings.running = false;
            if (settings.ffRender)
            {
                if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                ffmpeg.StandardInput.Close();
                ffmpeg.Close();
            }
            try
            {
                while (true)
                {
                    var r = render.disposeQueue.Dequeue();
                    if (!r.Initialized) r.Dispose();
                }
            }
            catch (InvalidOperationException) { }
            render.renderer.Dispose();
            this.Close();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (settings.paused)
            {
                double x = (double)e.X / Width;
                double y = (double)e.Y / Height;
                y = 1 - y;
                int key = (int)Math.Floor(x * (settings.lastNote - settings.firstNote) + settings.firstNote);

                double time = (settings.deltaTimeOnScreen - midiTime + midiTime * settings.pianoHeight - settings.deltaTimeOnScreen * y) / (settings.pianoHeight - 1);
                Console.WriteLine("--------------------------");
                Console.WriteLine("Time (ticks): " + Math.Round(time * 10) / 10);
                Console.WriteLine("Key number: " + key);
                Console.WriteLine("Nearest quarter note ticks: " + Math.Round(time / midi.division) * midi.division);
                Console.WriteLine("Nearest full note ticks: " + Math.Round(time / midi.division / 4) * midi.division * 4);

                if (settings.clickDebug)
                {
                    Console.WriteLine("\nSearching for note...");
                    Note note = null;
                    lock (globalDisplayNotes)
                    {
                        foreach (Note n in globalDisplayNotes)
                        {
                            if (n.note == key && n.start < time && (n.end > time || !n.hasEnded))
                            {
                                note = n;
                            }
                        }
                    }
                    if (note == null) Console.WriteLine("No note was found");
                    else
                    {
                        Console.WriteLine("Note start (ticks): " + note.start);
                        if (note.hasEnded)
                            Console.WriteLine("Note end (ticks): " + note.end);
                        else
                            Console.WriteLine("Note end (ticks): [Not parsed yet]");
                        Console.WriteLine("Note velocity: " + note.vel);
                        Console.WriteLine("Note track: " + note.track.trackID);
                        Console.WriteLine("Note channel: " + note.channel);
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            settings.running = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        void DrawScreenQuad()
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(1, 0);
            GL.Vertex2(1, 1);
            GL.Vertex2(0, 1);
            GL.End();
        }
    }
}
