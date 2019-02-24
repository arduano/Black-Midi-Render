using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Black_Midi_Render
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RenderSettings settings;
        MidiFile midifile = null;
        string midipath = "";

        public MainWindow()
        {
            InitializeComponent();
            settings = new RenderSettings();
            InitialiseSettingsValues();
        }

        void InitialiseSettingsValues()
        {
            maxBufferSize.Value = settings.maxTrackBufferSize;
            viewWidth.Value = settings.width;
            viewHeight.Value = settings.height;
            viewFps.Value = settings.fps;
            firstNote.Value = settings.firstNote;
            lastNote.Value = settings.lastNote - 1;
            pianoHeight.Value = (int)(settings.pianoHeight * 100);
            noteBrightness.Value = (decimal)settings.noteBrightness;
            noteDeltaScreenTime.Value = settings.deltaTimeOnScreen;
            vsyncEnabled.IsChecked = settings.vsync;
            tempoSlider.Value = Math.Log(settings.tempoMultiplier, 2);
        }

        Task renderThread = null;
        RenderWindow win = null;
        void RunRenderWindow()
        {
            bool winStarted = false;
            Task winthread = new Task(() =>
            {
                win = new RenderWindow(midifile, settings);
                winStarted = true;
                win.Run();
            });
            winthread.Start();
            SpinWait.SpinUntil(() => winStarted);
            long time = 0;
            int nc = -1;
            long maxRam = 0;
            long avgRam = 0;
            long ramSample = 0;
            Stopwatch timewatch = new Stopwatch();
            timewatch.Start();
            try
            {
                while ((midifile.ParseUpTo(time += (long)(win.tempoFrameStep * 10)) || nc != 0) && settings.running)
                {
                    SpinWait.SpinUntil(() => midifile.currentSyncTime < win.midiTime + (long)(win.tempoFrameStep * 10) || !settings.running);
                    if (!settings.running) break;
                    nc = 0;
                    Note n;
                    lock (midifile.globalDisplayNotes)
                    {
                        var i = midifile.globalDisplayNotes.Iterate();
                        double cutoffTime = win.midiTime - settings.deltaTimeOnScreen;
                        while (i.MoveNext(out n))
                        {
                            if (n.hasEnded && n.end < cutoffTime)
                                i.Remove();
                            else nc++;
                        }
                    }
                    Console.WriteLine(
                        Math.Round((double)time / midifile.maxTrackTime * 10000) / 100 + "%\tNotes loaded: " + nc +
                        "\tNotes drawn: " + settings.notesOnScreen +
                        "\tRender FPS: " + Math.Round(settings.liveFps) + "        "
                        );
                    long ram = Process.GetCurrentProcess().PrivateMemorySize64;
                    if (maxRam < ram) maxRam = ram;
                    avgRam = (long)((double)avgRam * ramSample + ram) / (ramSample + 1);
                    ramSample++;
                }
            }
            catch
            {
                MessageBox.Show("An error occurred while opeining render window. Please try again.");
                settings.running = false;
            }
            winthread.GetAwaiter().GetResult();
            settings.running = false;
            midifile.Reset();
            Console.WriteLine(
                    "Finished render\nRAM usage (Private bytes)\nPeak: " + Math.Round((double)maxRam / 1000 / 1000 / 1000 * 100) / 100 +
                    "GB\nAvg: " + Math.Round((double)avgRam / 1000 / 1000 / 1000 * 100) / 100 +
                    "GB\nMinutes to render: " + Math.Round((double)timewatch.ElapsedMilliseconds / 1000 / 60 * 100) / 100);
            Dispatcher.Invoke(() =>
            {
                Resources["notRendering"] = true;
                Resources["notPreviewing"] = true;
            });
        }

        private void BrowseMidiButton_Click(object sender, RoutedEventArgs e)
        {
            var open = new OpenFileDialog();
            open.Filter = "Midi files (*.mid)|*.mid";
            if ((bool)open.ShowDialog())
            {
                midiPath.Text = open.FileName;
                midipath = open.FileName;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(midipath))
            {
                MessageBox.Show("Midi file doesn't exist");
                return;
            }
            try
            {
                settings.maxTrackBufferSize = (int)maxBufferSize.Value;

                if (midifile != null) midifile.Dispose();
                midifile = null;
                GC.Collect();
                GC.WaitForFullGCComplete();
                midifile = new MidiFile(midipath, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            Resources["midiLoaded"] = true;
        }

        private void UnloadButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Unloading midi");
            midifile.Dispose();
            midifile = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.WriteLine("Unloaded");
            Resources["midiLoaded"] = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            settings.running = true;
            settings.width = (int)viewWidth.Value;
            settings.height = (int)viewHeight.Value;
            settings.fps = (int)viewFps.Value;
            renderThread = Task.Factory.StartNew(RunRenderWindow);
            Resources["notPreviewing"] = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            settings.running = false;
        }

        private void Nud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (sender == firstNote) settings.firstNote = (int)firstNote.Value;
                if (sender == lastNote) settings.lastNote = (int)lastNote.Value + 1;
                if (sender == pianoHeight) settings.pianoHeight = (double)pianoHeight.Value / 100;
                if (sender == noteBrightness) settings.noteBrightness = (float)noteBrightness.Value;
                if (sender == noteDeltaScreenTime) settings.deltaTimeOnScreen = (int)noteDeltaScreenTime.Value;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void StartRenderButton_Click(object sender, RoutedEventArgs e)
        {
            if(videoPath.Text == "")
            {
                MessageBox.Show("Please specify a destination path");
                return;
            }

            settings.running = true;
            settings.width = (int)viewWidth.Value;
            settings.height = (int)viewHeight.Value;
            settings.fps = (int)viewFps.Value;
            settings.ffRender = true;
            settings.ffPath = videoPath.Text;

            settings.paused = false;
            previewPaused.IsChecked = false;
            settings.tempoMultiplier = 1;
            tempoSlider.Value = 0;

            settings.useBitrate = (bool)bitrateOption.IsChecked;
            if (settings.useBitrate) settings.bitrate = (int)bitrate.Value;
            else
            {
                settings.crf = (int)crfFactor.Value;
                settings.crfPreset = (string)((ListBoxItem)crfPreset.SelectedItem).Content;
            }

            settings.includeAudio = (bool)includeAudio.IsChecked;
            settings.audioPath = audioPath.Text;
            renderThread = Task.Factory.StartNew(RunRenderWindow);
            Resources["notPreviewing"] = false;
            Resources["notRendering"] = false;
        }

        private void BrowseVideoSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var save = new SaveFileDialog();
            save.OverwritePrompt = true;
            save.Filter = "H.264 video (*.mp4)|*.mp4";
            if ((bool)save.ShowDialog())
            {
                videoPath.Text = save.FileName;
            }
        }

        private void BrowseAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var audio = new OpenFileDialog();
            audio.Filter = "Common audio files (*.mp3;*.wav;*.ogg;*.flac)|*.mp3;*.wav;*.ogg;*.flac";
            if ((bool)audio.ShowDialog())
            {
                audioPath.Text = audio.FileName;
            }
        }

        private void Paused_Checked(object sender, RoutedEventArgs e)
        {
            settings.paused = (bool)previewPaused.IsChecked;
        }

        private void VsyncEnabled_Checked(object sender, RoutedEventArgs e)
        {
            settings.vsync = (bool)vsyncEnabled.IsChecked;
        }

        private void TempoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                settings.tempoMultiplier = Math.Pow(2, tempoSlider.Value);
                tempoValue.Content = Math.Round(settings.tempoMultiplier * 100) / 100;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void ForceReRender_Checked(object sender, RoutedEventArgs e)
        {
            settings.forceReRender= (bool)forceReRender.IsChecked;
        }

        private void ClickDebug_Checked(object sender, RoutedEventArgs e)
        {
            settings.clickDebug = (bool)clickDebug.IsChecked;
        }

        private void TempoSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Pressed)
            {
                previewPaused.IsChecked = !settings.paused;
                settings.paused = (bool)previewPaused.IsChecked;
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                previewPaused.IsChecked = !settings.paused;
                settings.paused = (bool)previewPaused.IsChecked;
            }
        }

        private void PianoStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                settings.kbrender = (KeyboardRenderers)pianoStyle.SelectedIndex;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void NoteStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                settings.ntrender = (NoteRenderers)noteStyle.SelectedIndex;
            }
            catch (NullReferenceException)
            {

            }
        }
    }

    public class AndValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = true;
            for (int i = 0; i < values.Length; i++) b = b && (bool)values[i];

            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = false;
            for (int i = 0; i < values.Length; i++) b = b || (bool)values[i];

            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
