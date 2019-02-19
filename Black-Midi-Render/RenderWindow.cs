using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    class RenderWindow : GameWindow
    {
        FastList<Note> globalDisplayNotes;
        public FastList<Tempo> globalTempoEvents;
        MidiFile midi;

        int firstNote = 0;
        int lastNote = 128;

        int fps = 60;

        double pianoHeight = 0.2;

        View view = new View(new Vector2(0.5f, 0.5f), 0, 2);

        RenderSettings settings = new RenderSettings();

        public double midiTime = 0;
        public double tempoFrameStep = 10;

        public int deltaTimeOnScreen = 300;

        Color4[] keyColors = new Color4[256];

        Process ffmpeg = new Process();
        bool ffRender = false;
        bool imgRender = false;
        long imgnumber = 0;
        Task lastRenderPush = null;

        bool Running = true;

        int realWidth;
        int realHeight;

        int fbuffer;
        int rtexture;

        int noteShader;
        int postShader;
        int defaultShader;

        int vertexBufferID;
        int colorBufferID;
        int attrib1BufferID;

        int quadBufferLength = 2048;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        double[] quadAttribbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 16];

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        public RenderWindow(int width, int height, MidiFile midi, string outFileName) : base((int)(DisplayDevice.Default.Width / 1.5), (int)(DisplayDevice.Default.Height / 1.5), new GraphicsMode(new ColorFormat(8, 8, 8, 8)), "Render", GameWindowFlags.Default, DisplayDevice.Default)
        {
            realWidth = width;
            realHeight = height;

            //WindowBorder = WindowBorder.Hidden;
            globalDisplayNotes = midi.globalDisplayNotes;
            globalTempoEvents = midi.globalTempoEvents;
            this.midi = midi;
            tempoFrameStep = ((double)midi.division / 50000) * (1000000 / fps);
            VSync = VSyncMode.Off;
            if (ffRender)
            {
                string args = "-y -f rawvideo -s " + realWidth + "x" + realHeight + " -pix_fmt rgb32 -r " + fps + " -i - -c:v h264 -vf vflip -an -b:v 12000k " + outFileName;
                ffmpeg.StartInfo = new ProcessStartInfo("ffmpeg", args);
                ffmpeg.StartInfo.RedirectStandardInput = true;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.Start();
            }
            if (imgRender)
            {
                if (!Directory.Exists("imgs"))
                {
                    Directory.CreateDirectory("imgs");
                }
            }

            int a, b;
            GLUtils.GenFrameBufferTexture(realWidth, realHeight, out fbuffer, out rtexture);
            GLUtils.GenFrameBufferTexture(realWidth, realHeight, out a, out b);

            defaultShader = GLUtils.MakeShaderProgram("default");
            noteShader = GLUtils.MakeShaderProgram("notes");
            postShader = GLUtils.MakePostShaderProgram("post");
            
            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];
            quadAttribbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            GL.GenBuffers(1, out attrib1BufferID);

            for (uint i = 0; i < indexes.Length; i++) indexes[i] = i;
            for(int i = 0; i < quadAttribbuff.Length;)
            {

                quadAttribbuff[i++] = 0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = 0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.3;
                quadAttribbuff[i++] = 0;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, attrib1BufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadAttribbuff.Length * 8),
                quadAttribbuff,
                BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(indexes.Length * 4),
                indexes,
                BufferUsageHint.StaticDraw);

            noteRender = new NoteRender(settings);
            keyboardRender = new KeyboardRender(settings);
        }
        NoteRender noteRender;
        KeyboardRender keyboardRender;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            long nc = -1;
            int nonoteframes = 0;
            while (Running && (nonoteframes < fps * 5 || midi.unendedTracks != 0))
            {
                if (nc == 0) nonoteframes++;
                else nonoteframes = 0;
                SpinWait.SpinUntil(() => midi.currentSyncTime > midiTime + tempoFrameStep || midi.unendedTracks == 0);

                GL.UseProgram(noteShader);

                GL.Enable(EnableCap.LineSmooth);
                GL.Enable(EnableCap.Blend);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.Enable(EnableCap.Texture2D);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.LineWidth(1);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbuffer);
                GL.Viewport(0, 0, realWidth, realHeight);
                view.ApplyTransform(1, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                
                settings.ResetVariableState();
                nc = noteRender.Render(globalDisplayNotes, midiTime);
                keyboardRender.Render();
                ProcessEvents();

                if (globalTempoEvents.First != null)
                    if (midiTime + tempoFrameStep > globalTempoEvents.First.pos && globalTempoEvents.First.pos >= midiTime)
                    {
                        var t = globalTempoEvents.Pop();
                        tempoFrameStep = ((double)midi.division / t.tempo) * (1000000 / fps);
                    }
                midiTime += tempoFrameStep;
                
                if (ffRender)
                {
                    byte[] pixels = new byte[realWidth * realHeight * 4];
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pixels.Length);
                    GL.ReadPixels(0, 0, realWidth, realHeight, PixelFormat.Bgra, PixelType.UnsignedByte, unmanagedPointer);
                    Marshal.Copy(unmanagedPointer, pixels, 0, pixels.Length);
                    if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                    lastRenderPush = Task.Run(() => ffmpeg.StandardInput.BaseStream.Write(pixels, 0, pixels.Length));
                    Marshal.FreeHGlobal(unmanagedPointer);
                }
                if (imgRender)
                {
                    byte[] pixels = new byte[realWidth * realHeight * 4];
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pixels.Length);
                    GL.ReadPixels(0, 0, realWidth, realHeight, PixelFormat.Bgra, PixelType.UnsignedByte, unmanagedPointer);
                    Marshal.Copy(unmanagedPointer, pixels, 0, pixels.Length);
                    if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                    lastRenderPush = Task.Run(() =>
                    {
                        Bitmap output = new Bitmap(realWidth, realHeight, realWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, unmanagedPointer);
                        output.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        output.Save("imgs\\img_" + imgnumber++ + ".png");
                        output.Dispose();
                        Marshal.FreeHGlobal(unmanagedPointer);
                    });
                }

                GL.UseProgram(postShader);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Viewport(0, 0, Width, Height);
                GL.BindTexture(TextureTarget.Texture2D, rtexture);
                DrawScreenQuad();
                GL.BindTexture(TextureTarget.Texture2D, 0);

                SwapBuffers();
            }
            if (ffRender) ffmpeg.Close();
            this.Close();
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

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }

        void getXAndWidth(int n, out double x, out double width)
        {
            x = (float)(n - firstNote) / (lastNote - firstNote);
            width = 1.0f / (lastNote - firstNote);
        }

        void FlushQuadBuffer()
        {
            if (quadBufferPos == 0) return;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadVertexbuff.Length * 8),
                quadVertexbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadColorbuff.Length * 4),
                quadColorbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 16, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, attrib1BufferID);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.DrawElements(PrimitiveType.Quads, quadBufferPos * 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }

    }
}
