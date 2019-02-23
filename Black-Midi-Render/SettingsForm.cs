using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Black_Midi_Render
{
    public partial class SettingsForm : Form
    {
        RenderSettings settings;

        Thread renderThread = null;

        public SettingsForm()
        {
            InitializeComponent();
            settings = new RenderSettings();
            width_nud.Value = settings.width;
            height_nud.Value = settings.height;
            fps_nud.Value = settings.fps;
            minNote_nud.Value = settings.firstNote + 1;
            maxNote_nud.Value = settings.lastNote;
            pianoHeight_nud.Value = (decimal)settings.pianoHeight * 100;
            noteDT_nud.Value = settings.deltaTimeOnScreen;
            maxBuffer_nud.Value = settings.maxTrackBufferSize;
            glowCheck.Checked = settings.glowEnabled;
            glowRad_nud.Enabled = glowCheck.Checked;
            glowRad_nud.Value = settings.glowRadius;
            bitrate_nud.Value = settings.bitrate;
            keyboardRenderBox.SelectedIndex = (int)settings.kbrender;
            noteRenderBox.SelectedIndex = (int)settings.ntrender;
            noteBrightness_nud.Value = (decimal)settings.noteBrightness;
        }

        MidiFile midifile = null;

        private void browseButton_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            open.Filter = "Midi files (*.mid)|*.mid";
            if (open.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            MidiBox.Text = open.FileName;
            loadButton.Enabled = true;
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(MidiBox.Text))
            {
                MessageBox.Show("Midi file doesn't exist");
                return;
            }
            try
            {
                midifile = new MidiFile(MidiBox.Text, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            unloadButton.Enabled = true;
            startButton.Enabled = true;
        }

        private void unloadButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Unloading midi");
            midifile.Dispose();
            midifile = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            unloadButton.Enabled = false;
            startButton.Enabled = false;
        }

        private void width_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.width = (int)width_nud.Value;
        }

        private void height_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.height = (int)height_nud.Value;
        }

        private void fps_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.fps = (int)fps_nud.Value;
        }

        private void minNote_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.firstNote = (int)(minNote_nud.Value - 1);
        }

        private void maxNote_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.lastNote = (int)maxNote_nud.Value;
        }

        private void pianoHeight_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.pianoHeight = (float)pianoHeight_nud.Value / 100;
        }

        private void noteDT_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.deltaTimeOnScreen = (int)noteDT_nud.Value;
        }

        private void maxBuffer_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.maxTrackBufferSize = (int)maxBuffer_nud.Value;
        }


        private void glowCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.glowEnabled = glowCheck.Checked;
            glowRad_nud.Enabled = glowCheck.Checked;
        }

        private void glowRad_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.glowRadius = (int)glowRad_nud.Value;
        }

        private void bitrate_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.bitrate = (int)bitrate_nud.Value;
        }

        RenderWindow win = null;
        bool inRenderLoop = false;
        private void startButton_Click(object sender, EventArgs e)
        {
            if (midifile == null)
            {
                MessageBox.Show("Please open and load a midi file first");
                return;
            }

            settings.running = true;
            midifile.Reset();

            settings.vsync = noneVsync_radio.Checked;
            settings.ffRender = ff_radio.Checked;
            settings.imgRender = img_radio.Checked;
            settings.ffPath = ffpath.Text;
            settings.imgPath = imgpath.Text;
            settings.includeAudio = useAudioCheck.Checked;
            if (useAudioCheck.Checked) settings.audioPath = wavPath.Text;
            if (settings.ffRender && settings.ffPath == "")
            {
                MessageBox.Show("Please specify a save file");
                return;
            }
            if (settings.imgRender && settings.imgPath == "")
            {
                MessageBox.Show("Please specify a save folder");
                return;
            }

            renderThread = new Thread(() =>
            {
                bool winStarted = false;
                Thread winthread = new Thread(() =>
                {
                    win = new RenderWindow(midifile, settings);
                    winStarted = true;
                    win.Run();
                });
                winthread.Start();
                SpinWait.SpinUntil(() => winStarted);
                long time = 0;
                int nc = -1;
                inRenderLoop = true;
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
                    inRenderLoop = false;
                }
                inRenderLoop = false;
                winthread.Join();
                settings.running = false;
                midifile.Reset();
                Console.WriteLine(
                        "Finished render\nRAM usage (Private bytes)\nPeak: " + Math.Round((double)maxRam / 1000 / 1000 / 1000 * 100) / 100 +
                        "GB\nAvg: " + Math.Round((double)avgRam / 1000 / 1000 / 1000 * 100) / 100 +
                        "GB\nMinutes to render: " + Math.Round((double)timewatch.ElapsedMilliseconds / 1000 / 60 * 100) / 100);
            });
            inRenderLoop = false;
            renderThread.Start();
            startButton.Enabled = false;
            stopButton.Enabled = true;
            groupBox1.Enabled = false;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to stop?", "Stop rendering", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            Console.WriteLine("Stopping and reseting");
            settings.running = false;
            renderThread.Join();
            stopButton.Enabled = false;
            startButton.Enabled = true;
            groupBox1.Enabled = true;
            midifile.Reset();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close?", "Close", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            settings.running = false;
            if (renderThread != null) renderThread.Join();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (win != null && !inRenderLoop)
            {
                startButton.Enabled = true;
                stopButton.Enabled = false;
                groupBox1.Enabled = true;
                win = null;
            }
        }

        private void browseFFButton_Click(object sender, EventArgs e)
        {
            var save = new SaveFileDialog();
            save.OverwritePrompt = true;
            save.Filter = "H.264 video (*.mp4)|*.mp4";
            if (save.ShowDialog() != DialogResult.OK) return;
            ffpath.Text = save.FileName;
        }

        private void browseImgButton_Click(object sender, EventArgs e)
        {
            var save = new FolderBrowserDialog();
            if (save.ShowDialog() != DialogResult.OK) return;
            imgpath.Text = save.SelectedPath;
        }

        private void ff_radio_CheckedChanged(object sender, EventArgs e)
        {
            browseFFButton.Enabled = ff_radio.Checked;
            bitrate_nud.Enabled = ff_radio.Checked;
            ffpath.Enabled = ff_radio.Checked;
            useAudioCheck.Enabled = ff_radio.Checked;
            wavPath.Enabled = useAudioCheck.Checked && ff_radio.Checked;
            browseWavButton.Enabled = useAudioCheck.Checked && ff_radio.Checked;
        }

        private void img_radio_CheckedChanged(object sender, EventArgs e)
        {
            browseImgButton.Enabled = img_radio.Checked;
            imgpath.Enabled = img_radio.Checked;
        }

        private void useAudioCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.includeAudio = useAudioCheck.Checked;
            wavPath.Enabled = useAudioCheck.Checked;
            browseWavButton.Enabled = useAudioCheck.Checked;
        }

        private void browseWavButton_Click(object sender, EventArgs e)
        {
            var save = new SaveFileDialog();
            save.OverwritePrompt = false;
            save.Filter = "Common audio files (*.mp3;*.wav;*.ogg;*.flac)|*.mp3;*.wav;*.ogg;*.flac";
            if (save.ShowDialog() != DialogResult.OK) return;
            wavPath.Text = save.FileName;
        }

        private void keyboardRenderBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings.kbrender = (KeyboardRenderers)keyboardRenderBox.SelectedIndex;
        }

        private void noteRenderBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings.ntrender = (NoteRenderers)noteRenderBox.SelectedIndex;
        }

        private void noteBrightness_nud_ValueChanged(object sender, EventArgs e)
        {
            settings.noteBrightness = (float)noteBrightness_nud.Value;
        }
    }
}
