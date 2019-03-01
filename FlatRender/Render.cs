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
        string noteShaderVert = @"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec4 glColor;

out vec4 color;

void main()
{
    gl_Position = vec4(position.x * 2 - 1, position.y * 2 - 1, 1.0f, 1.0f);
    color = glColor;
}
";
        string noteShaderFrag = @"#version 330
 
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

        public int NoteScreenTime { get { return settings.deltaTimeOnScreen; } }

        int noteShader;

        int vertexBufferID;
        int colorBufferID;

        int quadBufferLength = 2048 * 2;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 4 * 6];

        public void Dispose()
        {
            GL.DeleteBuffers(3, new int[] { vertexBufferID, colorBufferID });
            GL.DeleteProgram(noteShader);
            Initialized = false;
        }

        public Render(RenderSettings settings)
        {
            this.renderSettings = settings;
            this.settings = new Settings();
            PreviewImage = BitmapToImageSource(Properties.Resources.preview);
            settingsControl = new SettingsCtrl(this.settings);
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
        }

        public void RenderFrame(FastList<Note> notes, double midiTime, int finalCompositeBuff)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.LineWidth(1);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, finalCompositeBuff);
            GL.Viewport(0, 0, renderSettings.width, renderSettings.height);
            GL.Clear(ClearBufferMask.ColorBufferBit);


            renderSettings.ResetVariableState();
            GL.UseProgram(noteShader);

            #region Vars
            long nc = 0;
            int firstNote = settings.firstNote;
            int lastNote = settings.lastNote;
            int deltaTimeOnScreen = settings.deltaTimeOnScreen;
            double pianoHeight = settings.pianoHeight;
            Color4[] keyColors = renderSettings.keyColors;
            double wdth;
            float r, g, b, a;
            double y1;
            double y2;
            quadBufferPos = 0;
            #endregion

            #region Notes
            quadBufferPos = 0;
            foreach (Note n in notes)
            {
                double renderCutoff = midiTime - deltaTimeOnScreen;
                if (n.end >= renderCutoff || !n.hasEnded)
                    if (n.start < midiTime)
                    {
                        if (n.note >= firstNote && n.note < lastNote)
                        {
                            nc++;
                            Color4 coll = n.track.trkColor[n.channel * 2];
                            Color4 colr = n.track.trkColor[n.channel * 2 + 1];
                            int k = n.note;
                            if (n.start < renderCutoff)
                            {
                                keyColors[k * 2] = coll;
                                keyColors[k * 2 + 1] = colr;
                            }

                            double x1;
                            x1 = (float)(k - firstNote) / (lastNote - firstNote);
                            wdth = 1.0f / (lastNote - firstNote);
                            double x2 = x1 + wdth;

                            y1 = 1 - (midiTime - n.end) / deltaTimeOnScreen * (1 - pianoHeight);
                            y2 = 1 - (midiTime - n.start) / deltaTimeOnScreen * (1 - pianoHeight);
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

            for (byte black = 0; black < 2; black++)
            {
                for (int n = firstNote; n < lastNote; n++)
                {
                    double x1;
                    x1 = (float)(n - firstNote) / (lastNote - firstNote);
                    wdth = 1.0f / (lastNote - firstNote);
                    double x2 = x1 + wdth;

                    if (isBlackNote(n))
                    {
                        if (black == 0)
                            continue;
                        y2 = pianoHeight / 5 * 2;
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
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;
                    blendfac = colr.A;
                    revblendfac = 1 - blendfac;
                    colr = new Color4(
                        colr.R * blendfac + origcol.R * revblendfac,
                        colr.G * blendfac + origcol.G * revblendfac,
                        colr.B * blendfac + origcol.B * revblendfac,
                        1);
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
            FlushQuadBuffer(false);
            #endregion
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
                    trakcs[i][j * 2] = Color4.FromHsv(new OpenTK.Vector4((i * 16 + i) * 1.36271f % 1, 1.0f, settings.noteBrightness, 1f));
                    trakcs[i][j * 2 + 1] = Color4.FromHsv(new OpenTK.Vector4((i * 16 + i) * 1.36271f % 1, 1.0f, settings.noteBrightness, 1f));
                }
            }
        }
    }
}
