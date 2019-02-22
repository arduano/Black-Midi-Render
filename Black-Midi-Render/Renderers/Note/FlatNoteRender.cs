using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    class FlatNoteRender : INoteRender
    {
        RenderSettings settings;

        int noteShader;

        int vertexBufferID;
        int colorBufferID;

        int quadBufferLength = 2048;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 4 * 6];

        public FlatNoteRender(RenderSettings rendersettings)
        {
            settings = rendersettings;
            noteShader = GLUtils.MakeShaderProgram("notes_flat");

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
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.IndexPointer(IndexPointerType.Int, 1, 0);
            GL.DrawElements(PrimitiveType.Triangles, quadBufferPos * 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }

        public long Render(FastList<Note> notes, double midiTime)
        {
            GL.UseProgram(noteShader);
            long nc = 0;
            int firstNote = settings.firstNote;
            int lastNote = settings.lastNote;
            int deltaTimeOnScreen = settings.deltaTimeOnScreen;
            double pianoHeight = settings.pianoHeight;
            Color4[] keyColors = settings.keyColors;
            lock (notes)
            {
                double wdth;
                quadBufferPos = 0;
                float r, g, b, a;
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

                                double y1 = 1 - (midiTime - n.end) / deltaTimeOnScreen * (1 - pianoHeight);
                                double y2 = 1 - (midiTime - n.start) / deltaTimeOnScreen * (1 - pianoHeight);
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
                                if (quadBufferPos >= quadBufferLength)
                                {
                                    FlushQuadBuffer();
                                }
                            }
                        }
                        else break;
                }

                FlushQuadBuffer();
                quadBufferPos = 0;
            }
            return nc;
        }

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }
    }
}
