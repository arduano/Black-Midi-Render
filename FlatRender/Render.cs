using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using BMEngine;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace FlatRender
{
    public class Render : IPluginRender
    {
        #region PreviewConvert
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
        #endregion

        #region Info
        public string Name { get; } = "Flat";
        public string Description { get; } = "Flat renderer, requested by SquareWaveMidis for his channel";
        public bool Initialized { get; private set; } = false;
        public ImageSource PreviewImage { get; private set; }
        #endregion

        #region Shaders
        string noteShaderVert = @"#version 330 compatibility

layout(location = 0) in vec3 position;
layout(location = 1) in vec4 glColor;

out vec4 color;

void main()
{
    gl_Position = vec4(position.x * 2 - 1, position.y * 2 - 1, 1.0f, 1.0f);
    color = glColor;
}
";
        string noteShaderFrag = @"#version 330 compatibility
 
in vec4 color;
 
out vec4 outputF;
layout(location = 0) out vec4 texOut;

void main()
{
    outputF = color;
	texOut = outputF;
}
";
        #endregion

        RenderSettings renderSettings;
        Settings settings;

        SettingsCtrl settingsControl;

        public long LastNoteCount { get; private set; }

        public Control SettingsControl { get { return settingsControl; } }

        public double NoteScreenTime
        {
            get
            {
                if (settings.tickBased) return settings.deltaTimeOnScreen;
                return (double)settings.deltaTimeOnScreen * (500000 / 96.0) / LastMidiTimePerTick / 10;
            }
        }

        public double LastMidiTimePerTick { get; set; } = 500000 / 96.0;

        int noteShader;

        int vertexBufferID;
        int colorBufferID;

        int quadBufferLength = 2048 * 2;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 4 * 6];

        bool[] blackKeys = new bool[256];
        int[] keynum = new int[256];

        public void Dispose()
        {
            if (!Initialized) return;
            GL.DeleteBuffers(3, new int[] { vertexBufferID, colorBufferID });
            GL.DeleteProgram(noteShader);
            Initialized = false;
            Console.WriteLine("Disposed of FlatRender");
        }

        public Render(RenderSettings settings)
        {
            this.renderSettings = settings;
            this.settings = new Settings();
            PreviewImage = BitmapToImageSource(Properties.Resources.preview);
            settingsControl = new SettingsCtrl(this.settings);
            for (int i = 0; i < blackKeys.Length; i++) blackKeys[i] = isBlackNote(i);
            int b = 0;
            int w = 0;
            for (int i = 0; i < keynum.Length; i++)
            {
                if (blackKeys[i]) keynum[i] = b++;
                else keynum[i] = w++;
            }
        }

        public void Init()
        {
            int _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            int _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj, noteShaderVert);
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, noteShaderFrag);
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            noteShader = GL.CreateProgram();
            GL.AttachShader(noteShader, _fragObj);
            GL.AttachShader(noteShader, _vertexObj);
            GL.LinkProgram(noteShader);

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            for (uint i = 0; i < indexes.Length / 6; i++)
            {
                indexes[i * 6 + 0] = i * 4 + 0;
                indexes[i * 6 + 1] = i * 4 + 1;
                indexes[i * 6 + 2] = i * 4 + 3;
                indexes[i * 6 + 3] = i * 4 + 1;
                indexes[i * 6 + 4] = i * 4 + 3;
                indexes[i * 6 + 5] = i * 4 + 2;
            }

            GL.GenBuffers(1, out indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(indexes.Length * 4),
                indexes,
                BufferUsageHint.StaticDraw);
            Initialized = true;
            Console.WriteLine("Initialised FlatRender");
        }

        public void RenderFrame(FastList<Note> notes, double midiTime, int finalCompositeBuff)
        {
            GL.Enable(EnableCap.Blend);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, finalCompositeBuff);
            GL.Viewport(0, 0, renderSettings.width, renderSettings.height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            GL.UseProgram(noteShader);

            #region Vars
            long nc = 0;
            int firstNote = settings.firstNote;
            int lastNote = settings.lastNote;

            if (blackKeys[firstNote]) firstNote--;
            if (blackKeys[lastNote - 1]) lastNote++;

            double deltaTimeOnScreen = NoteScreenTime;
            double pianoHeight = settings.pianoHeight;
            bool sameWidth = settings.sameWidthNotes;
            Color4[] keyColors = new Color4[512];
            for (int i = 0; i < 512; i++) keyColors[i] = Color4.Transparent;
            double wdth;
            float r, g, b, a;
            float r2, g2, b2, a2;
            double x1;
            double x2;
            double y1;
            double y2;
            quadBufferPos = 0;

            double[] x1array = new double[lastNote - firstNote];
            double[] wdtharray = new double[lastNote - firstNote];
            if (settings.sameWidthNotes)
            {
                for (int i = 0; i < lastNote - firstNote; i++)
                {
                    x1array[i] = (float)i / (lastNote - firstNote);
                    wdtharray[i] = 1.0f / (lastNote - firstNote);
                }
            }
            else
            {
                for (int i = 0; i < lastNote - firstNote; i++)
                {
                    if (!blackKeys[i + firstNote])
                    {
                        x1array[i] = (float)(keynum[firstNote + i] - keynum[firstNote]) / (keynum[lastNote - 1] - keynum[firstNote] + 1);
                        wdtharray[i] = 1.0f / (keynum[lastNote - 1] - keynum[firstNote] + 1);
                    }
                    else
                    {
                        int _i = i + 1;
                        wdth = 0.6f / (keynum[lastNote - 1] - keynum[firstNote] + 1);
                        int bknum = keynum[i] % 5;
                        double offset = wdth / 2;
                        if (bknum == 0 || bknum == 2)
                        {
                            offset *= 1.3;
                        }
                        else if (bknum == 1 || bknum == 4)
                        {
                            offset *= 0.7;
                        }
                        x1array[i] = (float)(keynum[firstNote + _i] - keynum[firstNote]) / (keynum[lastNote - 1] - keynum[firstNote] + 1) - offset;
                        wdtharray[i] = wdth;
                    }
                }
            }
            #endregion

            #region Notes
            quadBufferPos = 0;
            double notePosFactor = 1 / deltaTimeOnScreen * (1 - pianoHeight);
            foreach (Note n in notes)
            {
                double renderCutoff = midiTime + deltaTimeOnScreen;
                if (n.end >= midiTime || !n.hasEnded)
                    if (n.start < renderCutoff)
                    {
                        if (n.note >= firstNote && n.note < lastNote)
                        {
                            nc++;
                            int k = n.note;
                            if (!(k >= firstNote && k < lastNote)) continue;
                            Color4 coll = n.track.trkColor[n.channel * 2];
                            Color4 colr = n.track.trkColor[n.channel * 2 + 1];
                            if (n.start < midiTime)
                            {
                                keyColors[k * 2] = coll;
                                keyColors[k * 2 + 1] = colr;
                            }
                            x1 = x1array[k - firstNote];
                            wdth = wdtharray[k - firstNote];
                            x2 = x1 + wdth;
                            y1 = 1 - (renderCutoff - n.end) * notePosFactor;
                            y2 = 1 - (renderCutoff - n.start) * notePosFactor;
                            if (!n.hasEnded)
                                y1 = 1;

                            int pos = quadBufferPos * 8;
                            quadVertexbuff[pos++] = x2;
                            quadVertexbuff[pos++] = y2;
                            quadVertexbuff[pos++] = x2;
                            quadVertexbuff[pos++] = y1;
                            quadVertexbuff[pos++] = x1;
                            quadVertexbuff[pos++] = y1;
                            quadVertexbuff[pos++] = x1;
                            quadVertexbuff[pos++] = y2;

                            pos = quadBufferPos * 16;
                            r = coll.R;
                            g = coll.G;
                            b = coll.B;
                            a = coll.A;
                            quadColorbuff[pos++] = r;
                            quadColorbuff[pos++] = g;
                            quadColorbuff[pos++] = b;
                            quadColorbuff[pos++] = a;
                            quadColorbuff[pos++] = r;
                            quadColorbuff[pos++] = g;
                            quadColorbuff[pos++] = b;
                            quadColorbuff[pos++] = a;
                            r = colr.R;
                            g = colr.G;
                            b = colr.B;
                            a = colr.A;
                            quadColorbuff[pos++] = r;
                            quadColorbuff[pos++] = g;
                            quadColorbuff[pos++] = b;
                            quadColorbuff[pos++] = a;
                            quadColorbuff[pos++] = r;
                            quadColorbuff[pos++] = g;
                            quadColorbuff[pos++] = b;
                            quadColorbuff[pos++] = a;

                            quadBufferPos++;
                            FlushQuadBuffer();
                        }
                    }
                    else break;

            }
            FlushQuadBuffer(false);
            quadBufferPos = 0;

            LastNoteCount = nc;
            #endregion

            #region Keyboard
            y1 = pianoHeight;
            y2 = 0;
            Color4[] origColors = new Color4[256];
            for (int k = firstNote; k < lastNote; k++)
            {
                if (isBlackNote(k))
                    origColors[k] = Color4.Black;
                else
                    origColors[k] = Color4.White;
            }

            for (int n = firstNote; n < lastNote; n++)
            {
                x1 = x1array[n - firstNote];
                wdth = wdtharray[n - firstNote];
                x2 = x1 + wdth;

                if (!blackKeys[n])
                {
                    y2 = 0;
                    if (settings.sameWidthNotes)
                    {
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
                }
                else continue;

                var coll = keyColors[n * 2];
                var colr = keyColors[n * 2 + 1];
                var origcol = origColors[n];
                float blendfac = coll.A;
                float revblendfac = 1 - blendfac;
                coll = new Color4(
                    coll.R * blendfac + origcol.R * revblendfac,
                    coll.G * blendfac + origcol.G * revblendfac,
                    coll.B * blendfac + origcol.B * revblendfac,
                    1);
                r = coll.R;
                g = coll.G;
                b = coll.B;
                a = coll.A;
                blendfac = colr.A;
                revblendfac = 1 - blendfac;
                colr = new Color4(
                    colr.R * blendfac + origcol.R * revblendfac,
                    colr.G * blendfac + origcol.G * revblendfac,
                    colr.B * blendfac + origcol.B * revblendfac,
                    1);
                r2 = colr.R;
                g2 = colr.G;
                b2 = colr.B;
                a2 = colr.A;

                int pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y1;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y1;

                pos = quadBufferPos * 16;
                quadColorbuff[pos++] = r;
                quadColorbuff[pos++] = g;
                quadColorbuff[pos++] = b;
                quadColorbuff[pos++] = a;
                quadColorbuff[pos++] = r;
                quadColorbuff[pos++] = g;
                quadColorbuff[pos++] = b;
                quadColorbuff[pos++] = a;
                quadColorbuff[pos++] = r2;
                quadColorbuff[pos++] = g2;
                quadColorbuff[pos++] = b2;
                quadColorbuff[pos++] = a2;
                quadColorbuff[pos++] = r2;
                quadColorbuff[pos++] = g2;
                quadColorbuff[pos++] = b2;
                quadColorbuff[pos++] = a2;
                quadBufferPos++;
                FlushQuadBuffer();
            }
            for (int n = firstNote; n < lastNote; n++)
            {
                x1 = x1array[n - firstNote];
                wdth = wdtharray[n - firstNote];
                x2 = x1 + wdth;

                if (blackKeys[n])
                {
                    y2 = pianoHeight / 5 * 2;
                }
                else continue;

                var coll = keyColors[n * 2];
                var colr = keyColors[n * 2 + 1];
                var origcol = origColors[n];
                float blendfac = coll.A;
                float revblendfac = 1 - blendfac;
                coll = new Color4(
                    coll.R * blendfac + origcol.R * revblendfac,
                    coll.G * blendfac + origcol.G * revblendfac,
                    coll.B * blendfac + origcol.B * revblendfac,
                    1);
                r = coll.R;
                g = coll.G;
                b = coll.B;
                a = coll.A;
                blendfac = colr.A;
                revblendfac = 1 - blendfac;
                colr = new Color4(
                    colr.R * blendfac + origcol.R * revblendfac,
                    colr.G * blendfac + origcol.G * revblendfac,
                    colr.B * blendfac + origcol.B * revblendfac,
                    1);
                r2 = colr.R;
                g2 = colr.G;
                b2 = colr.B;
                a2 = colr.A;
                
                int pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y1;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y1;

                pos = quadBufferPos * 16;
                quadColorbuff[pos++] = r;
                quadColorbuff[pos++] = g;
                quadColorbuff[pos++] = b;
                quadColorbuff[pos++] = a;
                quadColorbuff[pos++] = r;
                quadColorbuff[pos++] = g;
                quadColorbuff[pos++] = b;
                quadColorbuff[pos++] = a;
                quadColorbuff[pos++] = r2;
                quadColorbuff[pos++] = g2;
                quadColorbuff[pos++] = b2;
                quadColorbuff[pos++] = a2;
                quadColorbuff[pos++] = r2;
                quadColorbuff[pos++] = g2;
                quadColorbuff[pos++] = b2;
                quadColorbuff[pos++] = a2;
                quadBufferPos++;
                FlushQuadBuffer();
            }
            FlushQuadBuffer(false);
            #endregion
            
            GL.Disable(EnableCap.Blend);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.Disable(EnableCap.Texture2D);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
        }

        void FlushQuadBuffer(bool check = true)
        {
            if (quadBufferPos < quadBufferLength && check) return;
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
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.IndexPointer(IndexPointerType.Int, 1, 0);
            GL.DrawElements(PrimitiveType.Triangles, quadBufferPos * 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }

        public void SetTrackColors(Color4[][] trakcs)
        {
            for (int i = 0; i < trakcs.Length; i++)
            {
                for (int j = 0; j < trakcs[i].Length / 2; j++)
                {
                    trakcs[i][j * 2] = Color4.FromHsv(new OpenTK.Vector4((i * 16 + j) * 1.36271f % 1, 1.0f, settings.noteBrightness, 1f));
                    trakcs[i][j * 2 + 1] = Color4.FromHsv(new OpenTK.Vector4((i * 16 + j) * 1.36271f % 1, 1.0f, settings.noteBrightness, 1f));
                }
            }
        }
    }
}
