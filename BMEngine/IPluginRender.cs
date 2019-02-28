using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BMEngine
{
    public interface IPluginRender : IDisposable
    {
        string Name { get; }
        string Description { get; }
        bool Initialized { get; }
        ImageSource PreviewImage { get; }

        void Init();
        void RenderFrame(FastList<Note> notes, double midiTime, int finalCompositeBuff);
    }
}
