using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    static class GLUtils
    {
        public static int MakeShaderProgram(string path)
        {
            int _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            int _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj, File.ReadAllText(path + ".vert"));
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            Console.Write(string.Format("triangle.vert compile: {0}", info));
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, File.ReadAllText(path + ".frag"));
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            Console.Write(string.Format("triangle.frag compile: {0}", info));
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            int shader = GL.CreateProgram();
            GL.AttachShader(shader, _fragObj);
            GL.AttachShader(shader, _vertexObj);
            GL.LinkProgram(shader);
            Console.Write(string.Format("link program: {0}", GL.GetProgramInfoLog(shader)));
            Console.Write(string.Format("use program: {0}", GL.GetProgramInfoLog(shader)));
            return shader;
        }

    }
}
