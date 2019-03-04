﻿using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Polygon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using System.Drawing.Imaging;

namespace BMEngine
{
    public class GLTextEngine
    {
        #region Shaders
        string textShaderVert = @"#version 330 compatibility

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 uv;

out vec2 UV;

void main()
{
    gl_Position = vec4(position.x * 2 - 1, position.y * 2 - 1, 0, 1.0f);
    UV = uv;
}
";
        string textShaderFrag = @"#version 330 compatibility

in vec2 UV;

out vec4 color;

uniform sampler2D myTextureSampler;

void main()
{
    float mask = texture( myTextureSampler, UV ).y;
    color = vec4(0.5, 0.5, 0.5, mask);
}
";
        #endregion

        int charMapTex;
        Size mapCharSize;
        SizeF[] charSizes;

        int textShader;

        int vertexBufferID;
        int uvBufferID;

        int quadBufferLength = 2048 * 2;
        double[] quadVertexbuff;
        double[] quaduvbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 4 * 6];

        public void Dispose()
        {
            GL.DeleteBuffers(3, new int[] { vertexBufferID });
            GL.DeleteProgram(textShader);
            GL.DeleteTexture(charMapTex);
        }

        public GLTextEngine()
        {
            int _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            int _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj, textShaderVert);
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, textShaderFrag);
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            textShader = GL.CreateProgram();
            GL.AttachShader(textShader, _fragObj);
            GL.AttachShader(textShader, _vertexObj);
            GL.LinkProgram(textShader);

            quadVertexbuff = new double[quadBufferLength * 8];
            quaduvbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out uvBufferID);
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

            int posloc = GL.GetAttribLocation(textShader, "position");
            int uvloc = GL.GetAttribLocation(textShader, "uv");

            var bitmap = GenerateCharacters(100, "Brush Script Std", out mapCharSize, out charSizes);
            charMapTex = loadImage(bitmap);
            bitmap.Dispose();
        }

        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            image.UnlockBits(data);

            return texID;
        }

        public void Render()
        {
            GL.Enable(EnableCap.Blend);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.Enable(EnableCap.Texture2D);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(textShader);
            GL.BindTexture(TextureTarget.Texture2D, charMapTex);

            quadBufferPos = 0;
            string text = "qwertQWERT:\n/';<1234.4567";
            Vector2 curpos = new Vector2(0, 0);
            foreach(char c in text)
            {
                if(c == '\n')
                {
                    curpos.Y += mapCharSize.Height;
                    curpos.X = 0;
                }
                if (!Characters.Contains(c)) continue;
                var chari = Characters.IndexOf(c);
                var sz = charSizes[chari];
                sz.Width *= 1.0f;
                double charwidth = 1.0 / Characters.Length;
                double s = charwidth * chari;
                double e = s + charSizes[chari].Width / mapCharSize.Width * charwidth;
                float padding = mapCharSize.Width / 8f;
                sz.Width -= padding * 2;
                Vector2 endpos = curpos + new Vector2(sz.Width, sz.Height);

                int pos = quadBufferPos * 8;
                quadVertexbuff[pos++] = (curpos.X - padding) / 1920;
                quadVertexbuff[pos++] = curpos.Y / 1080;
                quadVertexbuff[pos++] = (curpos.X - padding) / 1920;
                quadVertexbuff[pos++] = endpos.Y / 1080;
                quadVertexbuff[pos++] = (endpos.X + padding) / 1920;
                quadVertexbuff[pos++] = endpos.Y / 1080;
                quadVertexbuff[pos++] = (endpos.X + padding) / 1920;
                quadVertexbuff[pos++] = curpos.Y / 1080;

                curpos.X += sz.Width;

                pos = quadBufferPos * 8;
                quaduvbuff[pos++] = s;
                quaduvbuff[pos++] = 0;
                quaduvbuff[pos++] = s;
                quaduvbuff[pos++] = 1;
                quaduvbuff[pos++] = e;
                quaduvbuff[pos++] = 1;
                quaduvbuff[pos++] = e;
                quaduvbuff[pos++] = 0;
                quadBufferPos++;
                FlushQuadBuffer();
            }
            FlushQuadBuffer(false);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D); 
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);

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
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quaduvbuff.Length * 8),
                quaduvbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.IndexPointer(IndexPointerType.Int, 1, 0);
            GL.DrawElements(PrimitiveType.Triangles, quadBufferPos * 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }
        
        private const string Characters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789µ§½!""#¤%&/()=?^*@£€${[]}\~¨'-_.:,;<>|°©®±¥";
        public Bitmap GenerateCharacters(int fontSize, string fontName, out Size charSize, out SizeF[] charSizes)
        {
            charSizes = new SizeF[Characters.Length];
            var characters = new List<Bitmap>();
            using (var font = new Font(fontName, fontSize))
            {
                for (int i = 0; i < Characters.Length; i++)
                {
                    var charBmp = GenerateCharacter(font, Characters[i]);
                    charSizes[i] = GetSize(font, Characters[i]);
                    characters.Add(charBmp);
                }
                charSize = new Size(characters.Max(x => x.Width), characters.Max(x => x.Height));
                var charMap = new Bitmap(charSize.Width * characters.Count, charSize.Height);
                using (var gfx = Graphics.FromImage(charMap))
                {
                    gfx.FillRectangle(Brushes.Black, 0, 0, charMap.Width, charMap.Height);
                    for (int i = 0; i < characters.Count; i++)
                    {
                        var c = characters[i];
                        gfx.DrawImageUnscaled(c, i * charSize.Width, 0);

                        c.Dispose();
                    }
                }
                return charMap;
            }
        }

        private Bitmap GenerateCharacter(Font font, char c)
        {
            var size = GetSize(font, c);
            var bmp = new Bitmap((int)size.Width, (int)size.Height);
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
                gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
            }
            return bmp;
        }
        private SizeF GetSize(Font font, char c)
        {
            using (var bmp = new Bitmap(512, 512))
            {
                using (var gfx = Graphics.FromImage(bmp))
                {
                    return gfx.MeasureString(c.ToString(), font);
                }
            }
        }
    }
}
