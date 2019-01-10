using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace AFileTransfer
{
    class FrequencyListener
    {
        double sensitivity;
        double frequency;
        int deviceNumber;
        WaveInEvent ourWave;
        Timer timer1;
        DateTime lastSpacer = DateTime.MaxValue;
        bool FileEnded = false;
        public BufferedWaveProvider bwp;

        public bool ReadByte = false;

        public Int32 envelopeMax;

        private int RATE = 44100; // sample rate of the sound card
        private int BUFFERSIZE = (int)Math.Pow(2, 11); // must be a multiple of 2

        public Dictionary<DateTime, bool> recordingTime = new Dictionary<DateTime, bool>();
        public string output = "";

        public FrequencyListener(double Sens, double frequency, int deviceNumber = 0)
        {
            this.frequency = frequency;
            this.sensitivity = Sens;
            this.deviceNumber = deviceNumber;
            timer1 = new Timer();
            timer1.Interval = 10;
            timer1.Elapsed += UpdateT;

        }

        private void UpdateT(object sender, ElapsedEventArgs e)
        {
            Update();
        }

        public void Listen()
        {
            
            ourWave = new WaveInEvent();
            ourWave.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1);
            ourWave.BufferMilliseconds = (int)((double)BUFFERSIZE / (double)RATE * 1000.0);
            ourWave.DataAvailable += OnDataAvailable;
            bwp = new BufferedWaveProvider(ourWave.WaveFormat);
            bwp.BufferLength = BUFFERSIZE * 2;

            bwp.DiscardOnBufferOverflow = true;
            ourWave.StartRecording();
            recordingTime.Clear();
            timer1.Enabled = true;
            output = "";
            FileEnded = false;
        }

        public void StopListening()
        {
            ourWave.StopRecording();
            timer1.Enabled = false;

        }
        public void ReportOnData()
        {
            var falseCount = recordingTime.Values.Count(m => m == false);
            var trueCount = recordingTime.Values.Count(m => m == true);
            Console.WriteLine("Report");
            Console.WriteLine($"True Count: {trueCount}; False Count: {falseCount}");
            Console.WriteLine($"Was it Playing: {WasNotePlaying(recordingTime.Values.ToList())}");
        }

        public string OutputData()
        {
            return output;
        }

        public static bool WasNotePlaying(List<bool> timePeriodBool)
        {
            var falseCount = timePeriodBool.Count(m => m == false);
            var trueCount = timePeriodBool.Count(m => m == true);
            return (trueCount > falseCount);
        }


        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            bwp.AddSamples(args.Buffer, 0, args.BytesRecorded);
        }

        public void ConvertOutputToFile(string fileName)
        {
            var temp = ConvertToBytes(output);
            File.WriteAllBytes(fileName, temp);
        }

        byte[] ConvertToBytes(string b)
        {
            BitArray bits = new BitArray(b.ToList().ConvertAll<bool>(x => x == '1').ToArray());

            byte[] ret = new byte[bits.Length];
            bits.CopyTo(ret, 0);

            return ret;
        }


        public void Update()
        {
            // read the bytes from the stream
            int frameSize = BUFFERSIZE;
            var frames = new byte[frameSize];
            bwp.Read(frames, 0, frameSize);
            if (frames.Length == 0) return;
            if (frames[frameSize - 2] == 0) return;

            timer1.Enabled = false;

            // convert it to int32 manually (and a double for scottplot)
            int SAMPLE_RESOLUTION = 16;
            int BYTES_PER_POINT = SAMPLE_RESOLUTION / 8;
            Int32[] vals = new Int32[frames.Length / BYTES_PER_POINT];
            double[] Ys = new double[frames.Length / BYTES_PER_POINT];
            double[] Xs = new double[frames.Length / BYTES_PER_POINT];
            double[] Ys2 = new double[frames.Length / BYTES_PER_POINT];
            double[] Xs2 = new double[frames.Length / BYTES_PER_POINT];
            for (int i = 0; i < vals.Length; i++)
            {
                // bit shift the byte buffer into the right variable format
                byte hByte = frames[i * 2 + 1];
                byte lByte = frames[i * 2 + 0];
                vals[i] = (int)(short)((hByte << 8) | lByte);
                Xs[i] = i;
                Ys[i] = vals[i];
                Xs2[i] = (double)i / Ys.Length * RATE / 1000.0; // units are in kHz
            }


            //update scottplot (FFT, frequency domain)
            Ys2 = FFT(Ys);
            

            var temp = Ys2.Take(Ys2.Length / 2).ToArray();
            //Console.WriteLine($"{DateTime.Now} {temp[406]}");
            //464
            var topSens = 5;
            if (!ReadByte && !FileEnded)
            {
                if (temp[463] > topSens)
                {
                    output += "1";
                }
                else if (temp[406] > topSens)
                {
                    output += "0";
                }
                ReadByte = true;
                lastSpacer = DateTime.MaxValue;
            }
            else if (!FileEnded)
            {
                if ((temp[488] > topSens))
                {
                    ReadByte = false;
                    if (lastSpacer == DateTime.MaxValue)
                    {
                        lastSpacer = DateTime.Now;
                    } else
                    {
                        if ((DateTime.Now - lastSpacer).TotalSeconds > 3)
                        {
                            FileEnded = true;
                        }
                    }
                }
            }
            



            timer1.Enabled = true;
        }
        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length]; // this is where we will store the output (fft)
            Complex[] fftComplex = new Complex[data.Length]; // the FFT function requires complex format
            for (int i = 0; i < data.Length; i++)
            {
                fftComplex[i] = new Complex(data[i], 0.0); // make it complex format (imaginary = 0)
            }
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude; // back to double
                //fft[i] = Math.Log10(fft[i]); // convert to dB
            }
            return fft;
            //todo: this could be much faster by reusing variables
        }
    }
}
