using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using BMEngine;

namespace ClassicRender
{
    public class Render
    {
        RenderSettings settings;
        
        public long LastNoteCount { get; private set; }

        ShadedNoteRender noteRender;
        NewKeyboardRender keyboardRender;
        
        GLPostbuffer baseRenderBuff;

        int postShader;

        public Render(RenderSettings settings)
        {
            this.settings = settings;
            noteRender = new ShadedNoteRender(settings);
            keyboardRender = new NewKeyboardRender(settings);

            postShader = GLUtils.MakePostShaderProgram("post");
            
            baseRenderBuff = new GLPostbuffer(settings);
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
            GL.EnableVertexAttribArray(2);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.LineWidth(1);

            baseRenderBuff.BindBuffer();
            GL.Viewport(0, 0, settings.width, settings.height);
            GL.Clear(ClearBufferMask.ColorBufferBit);


            settings.ResetVariableState();
            LastNoteCount = noteRender.Render(notes, midiTime);
            keyboardRender.Render();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, finalCompositeBuff);
            GL.Viewport(0, 0, settings.width, settings.height);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(postShader);
            baseRenderBuff.BindTexture();
            DrawScreenQuad();
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
