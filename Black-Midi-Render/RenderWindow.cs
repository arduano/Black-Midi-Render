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

        int quadBufferLength = 2048;
        int quadVertexBufferID;
        double[] quadVertexbuff;
        int quadColorBufferID;
        float[] quadColorbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 16];

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        public RenderWindow(int width, int height, MidiFile midi, string outFileName) : base((int)(DisplayDevice.Default.Width / 1.5), (int)(DisplayDevice.Default.Height / 1.5), new GraphicsMode(new ColorFormat(8, 8, 8, 8)), "Render", GameWindowFlags.Default, DisplayDevice.Default)
        {
            //this.ClientRectangle = new Rectangle(DisplayDevice.Default.Bounds.Left, DisplayDevice.Default.Bounds.Top, (int)(DisplayDevice.Default.Width / 1.5), (int)(DisplayDevice.Default.Height / 1.5));
            //this.ClientRectangle = new Rectangle(0, 0, width + (width - Width), height + (height - Height));

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
                string args = "-y -f rawvideo -s " + width + "x" + height + " -pix_fmt rgb32 -r " + fps + " -i - -c:v h264 -vf vflip -an -b:v 12000k " + outFileName;
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

            fbuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbuffer);

            rtexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, rtexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.Byte, (IntPtr)0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, rtexture, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) throw new Exception();


            int _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            int _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj, File.ReadAllText(@"Shaders\notes.vert"));
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            Console.Write(string.Format("triangle.vert compile: {0}", info));
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, File.ReadAllText(@"Shaders\notes.frag"));
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            Console.Write(string.Format("triangle.frag compile: {0}", info));
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            int noteShader = GL.CreateProgram();
            GL.AttachShader(noteShader, _fragObj);
            GL.AttachShader(noteShader, _vertexObj);
            GL.LinkProgram(noteShader);
            Console.Write(string.Format("link program: {0}", GL.GetProgramInfoLog(noteShader)));
            Console.Write(string.Format("use program: {0}", GL.GetProgramInfoLog(noteShader)));

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];

            GL.GenBuffers(1, out quadVertexBufferID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadVertexbuff.Length * 8),
                quadVertexbuff,
                BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out quadColorBufferID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadColorBufferID);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(quadColorbuff.Length * 4),
                quadColorbuff,
                BufferUsageHint.StaticDraw);

            for (uint i = 0; i < indexes.Length; i++) indexes[i] = i;
            GL.GenBuffers(1, out indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(indexes.Length * 4),
                indexes,
                BufferUsageHint.StaticDraw);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            while (Running)
            {
                SpinWait.SpinUntil(() => midi.currentSyncTime > midiTime + tempoFrameStep || midi.unendedTracks == 0);
                if (midi.unendedTracks == 0) break;

                GL.Enable(EnableCap.LineSmooth);
                GL.Enable(EnableCap.Blend);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.LineWidth(1);
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbuffer);
                GL.Viewport(0, 0, realWidth, realHeight);
                view.ApplyTransform(1, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                drawNotes();
                drawKeyboard();
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

                GL.UseProgram(noteShader);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                //GL.Color3(1.0, 0, 0);
                GL.BindTexture(TextureTarget.Texture2D, rtexture);
                //GL.Begin(PrimitiveType.Quads);
                //GL.Vertex2(0, 0);
                //GL.Vertex2(1, 0);
                //GL.Vertex2(1, 1);
                //GL.Vertex2(0, 1);
                //GL.End();
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.UseProgram(0);
                SwapBuffers();
            }
            if (ffRender) ffmpeg.Close();
            this.Close();
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadVertexbuff.Length * 8),
                quadVertexbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexPointer(2, VertexPointerType.Double, 16, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadColorBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadColorbuff.Length * 4),
                quadColorbuff,
                BufferUsageHint.StaticDraw);
            GL.ColorPointer(4, ColorPointerType.Float, 16, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.DrawElements(PrimitiveType.Quads, quadBufferPos * 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }

        void drawNotes()
        {
            lock (globalDisplayNotes)
            {
                double wdth;
                for (int k = firstNote; k < lastNote; k++)
                {
                    if (isBlackNote(k))
                        keyColors[k] = Color4.Black;
                    else
                        keyColors[k] = Color4.White;
                }
                foreach (Note n in globalDisplayNotes)
                {
                    double renderCutoff = midiTime - deltaTimeOnScreen;
                    if (n.end >= renderCutoff || !n.hasEnded)
                        if (n.start < midiTime)
                        {
                            if (n.note >= firstNote && n.note < lastNote)
                            {
                                Color4 col = n.track.trkColor;
                                int k = n.note;
                                if (n.start < renderCutoff)
                                    keyColors[k] = col;

                                double x1;
                                getXAndWidth(k, out x1, out wdth);
                                double x2 = x1 + wdth;

                                double y1 = 1 - (midiTime - n.end) / deltaTimeOnScreen * (1 - pianoHeight);
                                double y2 = 1 - (midiTime - n.start) / deltaTimeOnScreen * (1 - pianoHeight);
                                if (!n.hasEnded)
                                    y1 = 1;
                                //else continue;

                                int pos = quadBufferPos * 8;
                                quadVertexbuff[pos++] = x1;
                                quadVertexbuff[pos++] = y2;
                                quadVertexbuff[pos++] = x2;
                                quadVertexbuff[pos++] = y2;
                                quadVertexbuff[pos++] = x2;
                                quadVertexbuff[pos++] = y1;
                                quadVertexbuff[pos++] = x1;
                                quadVertexbuff[pos] = y1;

                                pos = quadBufferPos * 16;
                                quadColorbuff[pos++] = col.R;
                                quadColorbuff[pos++] = col.G;
                                quadColorbuff[pos++] = col.B;
                                quadColorbuff[pos++] = col.A;
                                quadColorbuff[pos++] = col.R;
                                quadColorbuff[pos++] = col.G;
                                quadColorbuff[pos++] = col.B;
                                quadColorbuff[pos++] = col.A;
                                quadColorbuff[pos++] = col.R;
                                quadColorbuff[pos++] = col.G;
                                quadColorbuff[pos++] = col.B;
                                quadColorbuff[pos++] = col.A;
                                quadColorbuff[pos++] = col.R;
                                quadColorbuff[pos++] = col.G;
                                quadColorbuff[pos++] = col.B;
                                quadColorbuff[pos++] = col.A;

                                quadBufferPos++;
                                if (quadBufferPos >= quadBufferLength) {
                                    FlushQuadBuffer();
                                }

                                //GL.Color4(col);
                                //GL.Begin(PrimitiveType.Quads);
                                //GL.Vertex2(x1, y2);
                                //GL.Vertex2(x2, y2);
                                //GL.Vertex2(x2, y1);
                                //GL.Vertex2(x1, y1);
                                //GL.End();
                            }
                        }
                        else break;
                }

                FlushQuadBuffer();
                //GL.DrawElements(PrimitiveType.Quads, quadBufferPos * 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
                //GL.ColorPointer(4, ColorPointerType.Float, 0, quadColorbuff);
                //GL.VertexPointer(2, VertexPointerType.Double, 0, quadVertexbuff);
                //GL.DrawArrays(PrimitiveType.Quads, 0, quadBufferPos);
                //GL.Flush();
                quadBufferPos = 0;
            }
            drawKeyboard();
        }

        void drawKeyboard()
        {
            double wdth;
            double y1 = pianoHeight;
            double y2 = 0;
            for (byte black = 0; black < 2; black++)
            {
                for (int n = firstNote; n < lastNote; n++)
                {
                    double x1;
                    getXAndWidth(n, out x1, out wdth);
                    double x2 = x1 + wdth;

                    if (isBlackNote(n))
                    {
                        if (black == 0)
                            continue;
                        y2 = pianoHeight / 2;
                    }
                    else
                    {
                        if (black == 1)
                            continue;
                        y2 = 0;

                        int _n = n % 12;
                        if (_n == 0)
                            x2 += wdth * 0.666;
                        else if (_n == 2)
                        {
                            x1 -= wdth / 3;
                            x2 += wdth / 3;
                        }
                        else if (_n == 4)
                            x1 -= wdth / 3 * 2;
                        else if (_n == 5)
                            x2 += wdth * 0.75;
                        else if (_n == 7)
                        {
                            x1 -= wdth / 4;
                            x2 += wdth / 2;
                        }
                        else if (_n == 9)
                        {
                            x1 -= wdth / 2;
                            x2 += wdth / 4;
                        }
                        else if (_n == 11)
                            x1 -= wdth * 0.75;
                    }

                    GL.Color4(keyColors[n]);
                    GL.Begin(PrimitiveType.Quads);
                    GL.Vertex2(x1, y2);
                    GL.Vertex2(x2, y2);
                    GL.Vertex2(x2, y1);
                    GL.Vertex2(x1, y1);

                    GL.Color4(1, 1, 1, 0.3f);
                    GL.Vertex2(x1, y2);
                    GL.Color4(0, 0, 0, 0.3f);
                    GL.Vertex2(x2, y2);
                    GL.Color4(0, 0, 0, 0.0f);
                    GL.Vertex2(x2, y1);
                    GL.Color4(0, 0, 0, 0.3f);
                    GL.Vertex2(x1, y1);
                    GL.End();

                    GL.Color4(0, 0, 0, 0.1f);
                    GL.Begin(PrimitiveType.Lines);
                    //GL.Vertex2(x1, y1);
                    //GL.Vertex2(x1, y2);
                    //GL.Vertex2(x1, y2);
                    //GL.Vertex2(x2, y2);
                    //GL.Vertex2(x2, y2);
                    //GL.Vertex2(x2, y1);
                    //GL.Vertex2(x2, y1);
                    //GL.Vertex2(x1, y1);
                    GL.End();
                }
            }
        }
    }
}
