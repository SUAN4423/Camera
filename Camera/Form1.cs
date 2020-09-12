using Accord.Audio;
using Accord.DirectSound;
using AForge.Video.DirectShow;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.DirectSound;

namespace Camera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // フィールド
        public bool DeviceExist = false;
        public bool AudioDeviceExist = false;
        public bool OutputAudioDeviceExist = false;
        public FilterInfoCollection videoDevices;
        public AudioDeviceCollection audioDevices;
        public AudioDeviceCollection outputAudioDevices;
        public VideoCaptureDevice videoSource = null;
        public AudioCaptureDevice audioSource = null;
        public AudioOutputDevice audioOutput = null;
        public AudioOutputDevice[] audioOutputList = {
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null, null, null ,
            null, null, null, null, null, null, null, null };
        public Form2 videoWindow = null;
        private VideoCapabilities[] videoCapabilities;
        private VideoCapabilities[] videoCapabilities2;
        private Signal s = null;
        private float[] buffer;
        private bool first = true;

        private BufferedWaveProvider bufferedWaveProvider = null;
        private MMDevice mmDevice = null;
        private Thread thread = null;
        private int last = 0;

        // Loadイベント
        private void Form1_Load(object sender, EventArgs e)
        {
            this.getCameraInfo();
            this.getAudioInfo();
        }

        // カメラ情報の取得
        public void getCameraInfo()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                comboBox1.Items.Clear();

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                    comboBox1.SelectedIndex = 0;
                    DeviceExist = true;
                }
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                comboBox1.Items.Add("No Video Devices");
            }
        }

        public void getAudioInfo()
        {
            try
            {
                audioDevices = new AudioDeviceCollection(AudioDeviceCategory.Capture);
                comboBox2.Items.Clear();

                //if (audioDevices.Count == 0)
                //    throw new ApplicationException();

                foreach (AudioDeviceInfo device in audioDevices)
                {
                    comboBox2.Items.Add(device);
                    comboBox2.SelectedIndex = 0;
                    AudioDeviceExist = true;
                }
            }
            catch (ApplicationException)
            {
                AudioDeviceExist = false;
                comboBox2.Items.Add("No InputAudio Devices");
            }

            try
            {
                outputAudioDevices = new AudioDeviceCollection(AudioDeviceCategory.Output);
                comboBox5.Items.Clear();

                //if (audioDevices.Count == 0)
                //    throw new ApplicationException();

                foreach (AudioDeviceInfo device in outputAudioDevices)
                {
                    comboBox5.Items.Add(device);
                    comboBox5.SelectedIndex = 0;
                    OutputAudioDeviceExist = true;
                }
            }
            catch (ApplicationException)
            {
                OutputAudioDeviceExist = false;
                comboBox5.Items.Add("No OutputAudio Devices");
            }
        }

        // 開始or停止ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "開始")
            {

                if (DeviceExist)
                {
                    if (this.videoWindow == null)
                    {
                        this.videoWindow = new Form2();
                    }

                    this.CloseVideoSource();

                    videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                    if ((videoCapabilities != null) && (videoCapabilities.Length != 0))
                    {
                        videoSource.VideoResolution = videoSource.VideoCapabilities[comboBox3.SelectedIndex];
                        label1.Text = videoSource.VideoResolution.FrameSize + " ";
                        this.videoWindow.Size = videoSource.VideoResolution.FrameSize;
                    }
                    if ((videoCapabilities2 != null) && (videoCapabilities2.Length != 0))
                    {
                        videoSource.DesiredFrameRate = (int)comboBox4.SelectedItem;
                        label1.Text += " " + videoSource.VideoResolution.FrameRate;
                    }

                    this.videoWindow.videoSourcePlayer1.VideoSource = videoSource;
                    this.videoWindow.videoSourcePlayer1.Start();

                    button1.Text = "停止";

                    this.videoWindow.Show();

                    //timer1.Enabled = true;
                }
                else
                {
                    label1.Text = "No Video Devices";
                }
                if (checkBox1.Checked)
                {
                    first = true;
                    this.audioSource = new AudioCaptureDevice((AudioDeviceInfo)comboBox2.SelectedItem);
                    this.audioSource.NewFrame += source_NewFrame;
                    this.audioSource.Start();
                    Console.Out.WriteLine(audioSource.SampleRate + " " + audioSource.Channels);
                    /*
                    this.audioOutput = new AudioOutputDevice(this.Handle, audioSource.SampleRate, audioSource.Channels);
                    this.audioOutput.Output = ((AudioDeviceInfo)comboBox5.SelectedItem).Guid.ToString();
                    this.audioOutput.NewFrameRequested += output_NewFrameRequested;
                    this.audioOutput.FramePlayingStarted += output_FramePlayingStarted;
                    this.audioOutput.Play();
                    //*/
                    //*
                    for (int i = 0; i < audioOutputList.Length; i++)
                    {
                        this.audioOutputList[i] = new AudioOutputDevice(this.Handle, audioSource.SampleRate, audioSource.Channels);
                        this.audioOutputList[i].Output = ((AudioDeviceInfo)comboBox5.SelectedItem).Guid.ToString();
                    }
                    //*/
                    /*
                    bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(audioSource.SampleRate, 16, audioSource.Channels));
                    mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    Task t = waveOutput();
                    //*/
                }
            }
            else
            {
                if (this.videoWindow.videoSourcePlayer1.VideoSource != null)
                {
                    // stop camera
                    this.videoWindow.videoSourcePlayer1.SignalToStop();
                    this.videoWindow.videoSourcePlayer1.VideoSource = null;
                    this.CloseVideoSource();
                    this.videoWindow.Hide();
                    label1.Text = "停止中";
                    button1.Text = "開始";
                }
            }
        }

        // 停止の初期化
        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    videoSource = null;
                    Console.Out.WriteLine("videoSource Disposed");
                }
            if (!(audioSource == null))
            {
                if (audioSource.IsRunning)
                {
                    audioSource.SignalToStop();
                    audioSource.WaitForStop();
                }
                audioSource.Dispose();
                audioSource = null;
                Console.Out.WriteLine("audioSource Disposed");
            }
            if (!(audioOutput == null))
            {
                if (audioOutput.IsRunning)
                {
                    //audioOutput.Stop();
                    audioOutput.SignalToStop();
                    audioOutput.WaitForStop();
                }
                audioOutput.Dispose();
                audioOutput = null;
                Console.Out.WriteLine("audioOutput Disposed");
            }
            for (int i = 0; i < audioOutputList.Length; i++)
            {
                if (!(audioOutputList[i] == null))
                {
                    if (audioOutputList[i].IsRunning)
                    {
                        //audioOutput.Stop();
                        audioOutputList[i].SignalToStop();
                        audioOutputList[i].WaitForStop();
                    }
                    audioOutputList[i].Dispose();
                    audioOutputList[i] = null;
                    Console.Out.WriteLine("audioOutputList[" + i + "] Disposed");
                }
            }
        }

        private void output_FramePlayingStarted(object sender, PlayFrameEventArgs eventArgs)
        {
            Console.Out.WriteLine(eventArgs.FrameIndex);
            Console.Out.WriteLine(eventArgs.Count);
        }

        private async Task waveOutput()
        {
            using (IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200))
            {
                wavPlayer.Init(bufferedWaveProvider);
                wavPlayer.Play();


                //Console.WriteLine("Press ENTER to exit...");
                //Console.ReadLine();

                //*
                while (audioSource != null && audioSource.IsRunning)
                {
                    //Console.Out.WriteLine("Wait " + DateTime.Now);
                    await Task.Delay(32);
                }
                //*/

                wavPlayer.Stop();
            }

            Console.WriteLine("STOP WAVEOUT");
        }

        private void output_NewFrameRequested(object sender, NewFrameRequestedEventArgs eventArgs)
        {
            Console.Out.WriteLine(eventArgs.Frames + " " + eventArgs.FrameIndex + " " + DateTime.Now);
            //*
            if (s != null)
            {
                eventArgs.Buffer = buffer;
                eventArgs.Frames = buffer.Length;
                eventArgs.FrameIndex = 0;
            }
            //*/
            Console.Out.WriteLine("buffer " + DateTime.Now);
        }

        private void source_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            s = eventArgs.Signal;
            //for (int i = last; i != last - 1 || (last == 0 && i != audioOutputList.Length); i++)
            /*
            for (int i = 0; i < audioOutputList.Length; i++)
            {
                if (!audioOutputList[i].IsRunning)
                {
                    audioOutputList[i].Play(s.ToFloat());
                    last = i;
                    break;
                }
                //if (audioOutputList.Length - 1 == i) i = -1;
            }
            //*/
            audioOutputList[last].Play(s.ToFloat());
            last++;
            if (last == audioOutputList.Length) last = 0;
            //buffer = s.ToFloat();
            //bufferedWaveProvider.AddSamples(s.Data.ToByteArray(), 0, s.Data.ToByteArray().Length - 1);
            /*
            if (first)
            {
                first = false;
                audioOutput.Play(buffer);
            }
            //*/
        }

        // フレームレートの取得
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = videoSource.FramesReceived.ToString() + "FPS";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoDevices.Count != 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                EnumeratedSupportedFrameSizes(videoSource);
                EnumeratedSupportedFrameRate(videoSource);
            }
        }

        private void EnumeratedSupportedFrameRate(VideoCaptureDevice videoSource)
        {
            this.Cursor = Cursors.WaitCursor;

            comboBox4.Items.Clear();

            try
            {
                videoCapabilities2 = videoSource.VideoCapabilities;

                foreach (VideoCapabilities capabilty in videoCapabilities2)
                {
                    if (!comboBox4.Items.Contains(capabilty.FrameRate))
                    {
                        comboBox4.Items.Add(capabilty.FrameRate);
                    }
                }

                if (videoCapabilities2.Length == 0)
                {
                    comboBox4.Items.Add("Not supported");
                }

                comboBox4.SelectedIndex = 0;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }

        private void EnumeratedSupportedFrameSizes(VideoCaptureDevice videoSource)
        {
            this.Cursor = Cursors.WaitCursor;

            comboBox3.Items.Clear();

            try
            {
                videoCapabilities = videoSource.VideoCapabilities;

                foreach (VideoCapabilities capabilty in videoCapabilities)
                {
                    if (!comboBox3.Items.Contains(capabilty.FrameSize))
                    {
                        comboBox3.Items.Add(capabilty.FrameSize);
                    }
                }

                if (videoCapabilities.Length == 0)
                {
                    comboBox3.Items.Add("Not supported");
                }

                comboBox3.SelectedIndex = 0;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
