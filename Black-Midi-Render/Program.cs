using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SettingsWPF;

namespace Black_Midi_Render
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Black Midi Render";
            Application app = new Application();
            app.Run(new MainWindow());
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new SettingsForm());
        }
    }
}
