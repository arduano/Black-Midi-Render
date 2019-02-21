using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        RenderSettings settings;

        public double midiTime = 0;
        public double tempoFrameStep = 10;

        Color4[] keyColors = new Color4[256];

        Process ffmpeg = new Process();
        long imgnumber = 0;
        Task lastRenderPush = null;

        public bool Running = true;

        GLPostbuffer finalCompositeBuff;
        GLPostbuffer baseRenderBuff;
        GLPostbuffer glowMaskFirstPassBuff;
        GLPostbuffer glowMaskSecondPassBuff;

        int noteShader;
        int postShader;
        int glowShader;
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

        int glowTextureSize_var;
        int glowWidth_var;
        int glowSigma_var;
        int glowStrength_var;
        int glowPass_var;

        void BindUniforms()
        {
            glowTextureSize_var = GL.GetUniformLocation(glowShader, "u_textureSize");
            glowSigma_var = GL.GetUniformLocation(glowShader, "u_sigma");
            glowWidth_var = GL.GetUniformLocation(glowShader, "u_width");
            glowPass_var = GL.GetUniformLocation(glowShader, "u_pass");
            glowStrength_var = GL.GetUniformLocation(glowShader, "u_strength");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        public RenderWindow(MidiFile midi, RenderSettings settings) : base((int)(DisplayDevice.Default.Width / 1.5), (int)(DisplayDevice.Default.Height / 1.5), new GraphicsMode(new ColorFormat(8, 8, 8, 8)), "Render", GameWindowFlags.Default, DisplayDevice.Default)
        {
            this.settings = settings;

            //WindowBorder = WindowBorder.Hidden;
            globalDisplayNotes = midi.globalDisplayNotes;
            globalTempoEvents = midi.globalTempoEvents;
            this.midi = midi;
            tempoFrameStep = ((double)midi.division / 50000) * (1000000 / settings.fps);
            if (!settings.vsync) VSync = VSyncMode.Off;
            if (settings.ffRender)
            {
                string args = "-y -f rawvideo -s " + settings.width + "x" + settings.height + " -pix_fmt rgb32 -r " + settings.fps + " -i - -c:v h264 -vf vflip -an -b:v " + settings.bitrate + "k -maxrate " + settings.bitrate + "k -minrate " + settings.bitrate + "k \"" + settings.ffPath + "\"";
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
            if (settings.imgRender)
            {
                if (!Directory.Exists(settings.imgPath))
                {
                    Directory.CreateDirectory(settings.imgPath);
                }
            }

            finalCompositeBuff = new GLPostbuffer(settings);
            baseRenderBuff = new GLPostbuffer(settings);
            glowMaskFirstPassBuff = new GLPostbuffer(settings);
            glowMaskSecondPassBuff = new GLPostbuffer(settings);

            defaultShader = GLUtils.MakeShaderProgram("default");
            noteShader = GLUtils.MakeShaderProgram("notes");
            postShader = GLUtils.MakePostShaderProgram("post");
            glowShader = GLUtils.MakePostShaderProgram("glow");
            BindUniforms();

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];
            quadAttribbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            GL.GenBuffers(1, out attrib1BufferID);

            for (uint i = 0; i < indexes.Length; i++) indexes[i] = i;
            for (int i = 0; i < quadAttribbuff.Length;)
            {

                quadAttribbuff[i++] = 0.1;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.2;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = 0.1;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.0;
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
            while (Running && (midiTime < midi.maxTrackTime + settings.deltaTimeOnScreen + settings.fps * tempoFrameStep * 5 || midi.unendedTracks != 0))
            {
                SpinWait.SpinUntil(() => midi.currentSyncTime > midiTime + tempoFrameStep || midi.unendedTracks == 0 || !Running);
                if (!Running) break;

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

                baseRenderBuff.BindBuffer();
                GL.Viewport(0, 0, settings.width, settings.height);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                settings.ResetVariableState();
                nc = noteRender.Render(globalDisplayNotes, midiTime);
                keyboardRender.Render();

                if (settings.glowEnabled)
                {
                    GL.UseProgram(glowShader);
                    glowMaskFirstPassBuff.BindBuffer();
                    GL.Viewport(0, 0, settings.width, settings.height);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    GL.Uniform1(glowTextureSize_var, (float)settings.width);
                    GL.Uniform1(glowStrength_var, 10f);
                    GL.Uniform1(glowSigma_var, 1f);
                    GL.Uniform1(glowWidth_var, settings.glowRadius);
                    GL.Uniform1(glowPass_var, 0);

                    baseRenderBuff.BindTexture();
                    DrawScreenQuad();

                    glowMaskSecondPassBuff.BindBuffer();
                    GL.Viewport(0, 0, settings.width, settings.height);
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    GL.Uniform1(glowPass_var, 1);
                    GL.Uniform1(glowTextureSize_var, (float)settings.height);

                    glowMaskFirstPassBuff.BindTexture();
                    DrawScreenQuad();

                    finalCompositeBuff.BindBuffer();
                    GL.Viewport(0, 0, settings.width, settings.height);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    GL.UseProgram(postShader);
                    glowMaskSecondPassBuff.BindTexture();
                    DrawScreenQuad();
                    baseRenderBuff.BindTexture();
                    DrawScreenQuad();
                }
                else
                {
                    finalCompositeBuff.BindBuffer();
                    GL.Viewport(0, 0, settings.width, settings.height);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    GL.UseProgram(postShader);
                    baseRenderBuff.BindTexture();
                    DrawScreenQuad();
                }
                bool stepped = false;
                if (globalTempoEvents.First != null)
                {
                    if (midiTime + tempoFrameStep - settings.deltaTimeOnScreen > globalTempoEvents.First.pos)
                    {
                        var t = globalTempoEvents.Pop();
                        var _t = 1 - (t.pos - midiTime) / tempoFrameStep;
                        tempoFrameStep = ((double)midi.division / t.tempo) * (1000000 / settings.fps);
                        midiTime = t.pos + tempoFrameStep * _t;
                        stepped = true;
                    }
                }
                if(!stepped) midiTime += tempoFrameStep;

                if (settings.ffRender)
                {
                    finalCompositeBuff.BindBuffer();
                    byte[] pixels = new byte[settings.width * settings.height * 4];
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
                    byte[] pixels = new byte[settings.width * settings.height * 4];
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pixels.Length);
                    GL.ReadPixels(0, 0, settings.width, settings.height, PixelFormat.Bgra, PixelType.UnsignedByte, unmanagedPointer);
                    Marshal.Copy(unmanagedPointer, pixels, 0, pixels.Length);
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
                //GL.ClearColor(1, 1, 1, 1);
                DrawScreenQuad();
                GLPostbuffer.UnbindTextures();

                try
                {
                    SwapBuffers();
                }
                catch
                {
                    break;
                }
                ProcessEvents();
            }
            Running = false;
            if (settings.ffRender)
            {
                if (lastRenderPush != null) lastRenderPush.GetAwaiter().GetResult();
                ffmpeg.StandardInput.Close();
                ffmpeg.Close();
            }
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Running = false;
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
