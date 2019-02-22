using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    class NewKeyboardRender : IKeyboardRender
    {
        RenderSettings settings;


        int noteShader;

        int vertexBufferID;
        int colorBufferID;
        int attrib1BufferID;

        int quadBufferLength = 2048 * 2;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        double[] quadAttribbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 4 * 6];

        public NewKeyboardRender(RenderSettings rendersettings)
        {
            settings = rendersettings;
            noteShader = GLUtils.MakeShaderProgram("notes");

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];
            quadAttribbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            GL.GenBuffers(1, out attrib1BufferID);
            for (uint i = 0; i < indexes.Length / 6; i++)
            {
                indexes[i * 6 + 0] = i * 4 + 0;
                indexes[i * 6 + 1] = i * 4 + 2;
                indexes[i * 6 + 2] = i * 4 + 1;
                indexes[i * 6 + 3] = i * 4 + 0;
                indexes[i * 6 + 4] = i * 4 + 3;
                indexes[i * 6 + 5] = i * 4 + 2;
            }
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, attrib1BufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadAttribbuff.Length * 8),
                quadAttribbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.IndexPointer(IndexPointerType.Int, 1, 0);
            GL.DrawElements(PrimitiveType.Triangles, quadBufferPos * 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
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
            float r, g, b, a, r2, g2, b2, a2;
            double xx1, xx2, yy1, yy2;

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
                double x1;
                x1 = (float)(n - firstNote) / (lastNote - firstNote);
                wdth = 1.0f / (lastNote - firstNote);
                double x2 = x1 + wdth;

                if (!isBlackNote(n))
                {
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

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = 0.0;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.1;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.0;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.3;
                quadAttribbuff[pos++] = 0;

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

                pos = quadBufferPos * 8;
                x2 = x1 + wdth / 15;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y1;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y1;

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = -0.3;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.3;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.1;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.1;
                quadAttribbuff[pos++] = 0;

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
                double x1;
                x1 = (float)(n - firstNote) / (lastNote - firstNote);
                wdth = 1.0f / (lastNote - firstNote);
                double x2 = x1 + wdth;

                if (isBlackNote(n))
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

                xx1 = x1 + wdth / 6;
                xx2 = x2 - wdth / 6;
                yy1 = y1 + 0.005;
                yy2 = y2 + 0.02;

                //Middle
                int pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = xx1;
                quadVertexbuff[pos++] = yy2;
                quadVertexbuff[pos++] = xx2;
                quadVertexbuff[pos++] = yy2;
                quadVertexbuff[pos++] = xx2;
                quadVertexbuff[pos++] = yy1;
                quadVertexbuff[pos++] = xx1;
                quadVertexbuff[pos++] = yy1;

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.4;
                quadAttribbuff[pos++] = 0;

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

                //Bottom
                pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = xx2;
                quadVertexbuff[pos++] = yy2;
                quadVertexbuff[pos++] = xx1;
                quadVertexbuff[pos++] = yy2;

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.0;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;

                pos = quadBufferPos * 16;
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
                FlushQuadBuffer();

                //Left
                pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = xx1;
                quadVertexbuff[pos++] = yy2;
                quadVertexbuff[pos++] = xx1;
                quadVertexbuff[pos++] = yy1;
                quadVertexbuff[pos++] = x1;
                quadVertexbuff[pos++] = y1;

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.4;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = 0.2;
                quadAttribbuff[pos++] = 0;

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


                //Right
                pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y2;
                quadVertexbuff[pos++] = xx2;
                quadVertexbuff[pos++] = yy2;
                quadVertexbuff[pos++] = xx2;
                quadVertexbuff[pos++] = yy1;
                quadVertexbuff[pos++] = x2;
                quadVertexbuff[pos++] = y1;

                pos = quadBufferPos * 8;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;
                quadAttribbuff[pos++] = -0.2;
                quadAttribbuff[pos++] = 0;

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
        }

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }
    }
}
