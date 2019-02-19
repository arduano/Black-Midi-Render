using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    class KeyboardRender
    {
        RenderSettings settings;


        int noteShader;

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

        public KeyboardRender(RenderSettings rendersettings)
        {
            settings = rendersettings;
            noteShader = GLUtils.MakeShaderProgram("notes");

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];
            quadAttribbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            GL.GenBuffers(1, out attrib1BufferID);
            for (uint i = 0; i < indexes.Length; i++) indexes[i] = i;
            for (int i = 0; i < quadAttribbuff.Length;)
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

        public void Render()
        {
            int firstNote = settings.firstNote;
            int lastNote = settings.lastNote;
            int deltaTimeOnScreen = settings.deltaTimeOnScreen;
            double pianoHeight = settings.pianoHeight;
            Color4[] keyColors = settings.keyColors;
            double wdth;
            double y1 = pianoHeight;
            double y2 = 0;
            quadBufferPos = 0;
            float r, g, b, a;

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
                        y2 = pianoHeight / 5*2;
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
                    var col = keyColors[n];
                    var origcol = origColors[n];
                    float blendfac = col.A;
                    float revblendfac = 1 - blendfac;
                    col = new Color4(
                        col.R * blendfac + origcol.R * revblendfac,
                        col.G * blendfac + origcol.G * revblendfac,
                        col.B * blendfac + origcol.B * revblendfac,
                        1);
                    r = col.R;
                    g = col.G;
                    b = col.B;
                    a = col.A;
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;
                    quadColorbuff[pos++] = r;
                    quadColorbuff[pos++] = g;
                    quadColorbuff[pos++] = b;
                    quadColorbuff[pos++] = a;

                    quadBufferPos++;
                    if (quadBufferPos >= quadBufferLength)
                    {
                        FlushQuadBuffer();
                    }
                }
            }
            FlushQuadBuffer();
        }

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }
    }
}
