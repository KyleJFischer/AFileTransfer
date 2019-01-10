using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AFileTransfer
{
    public class FrequencyPlayer
    {
        double level;
        double frequency;

        int NoteTime = 200;
        int SpacerTime = 300;


        public DateTime StartTime = DateTime.MinValue;

        public FrequencyPlayer(double level, double frequency)
        {
            this.frequency = frequency;
            this.level = level;
        }

        public void StartTone()
        {
            var firstTone = new SignalGenerator()
            {
                Gain = 0.1,
                Frequency = 18500,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(1));
            var secondTone = new SignalGenerator()
            {
                Gain = 0.1,
                Frequency = 17500,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(1));
            var thirdTone = new SignalGenerator()
            {
                Gain = 0.1,
                Frequency = 15,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(1));
            var list = new List<ISampleProvider>();
            list.Add(firstTone);
            list.Add(secondTone);
            list.Add(thirdTone);
            PlayTonesFromList(list);
        }

        public void PlayTonesFromList(List<ISampleProvider> tones)
        {

            using (var wo = new WaveOutEvent())
            {
                wo.DeviceNumber = 4;
                StartTime = DateTime.Now;
                for (var i = 0; i < tones.Count; i++)
                {
                    wo.Init(tones[i]);
                    wo.Play();
                    while (wo.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1);
                    }
                }
               
            }
        }

        public void Example()
        {
            var firstTone = new SignalGenerator()
            {
                Gain = 1,
                Frequency = 17500,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(1));
            var secondTone = new SignalGenerator()
            {
                Gain = 0.0,
                Frequency = 17500,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(1));

            var list = new List<ISampleProvider>();
            list.Add(firstTone);
            list.Add(secondTone);
            //list.Add(thirdTone);
            PlayTonesFromList(list);
        }

        public void PlayFile(string filePath)
        {
            PlayString(GetFileString(filePath));
        }

        public string GetFileString(string filePath)
        {
            var allBytes = File.ReadAllBytes(filePath);
            var output = "";
            BitArray bits = new BitArray(allBytes);
            foreach(bool bit in bits)
            {
                output += bit ? "1" : "0";
            }
            return output;
        }

        public void Play()
        {
            
            var sine20Seconds = new SignalGenerator()
            {
                Gain = 1,
                Frequency = 17500,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(2));
            using (var wo = new WaveOutEvent())
            {
                wo.Init(sine20Seconds);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void PlayString(string ToPlay)
        {

            Console.WriteLine($"String to Play {ToPlay}");
            byte test = 0x10;
            var result = GetBitString(test);
            var list = new List<ISampleProvider>();
            foreach (var letter in ToPlay)
            {
                ISampleProvider note = null;
                if (letter == '1')
                {
                    note = new SignalGenerator()
                    {
                        Gain = 1.0,
                        Frequency = 20000,
                        Type = SignalGeneratorType.Sin
                    }.Take(TimeSpan.FromMilliseconds(NoteTime));
                }
                else
                {
                    note = new SignalGenerator()
                    {
                        Gain = 1.0,
                        Frequency = 17500,
                        Type = SignalGeneratorType.Sin
                    }.Take(TimeSpan.FromMilliseconds(NoteTime));
                }
                var spacerNote = new SignalGenerator()
                {
                    Gain = 1.0,
                    Frequency = 21000,
                    Type = SignalGeneratorType.Sin
                }.Take(TimeSpan.FromMilliseconds(SpacerTime));
                list.Add(note);
                list.Add(spacerNote);
            }
            var endNote = new SignalGenerator()
            {
                Gain = 1.0,
                Frequency = 22000,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromMilliseconds(1000));
            list.Add(endNote);
            PlayTonesFromList(list);

        }

        public string GetBitString(byte o)
        {

            var bits = GetBits(o);
            var returnString = "";
            foreach(var bit in bits)
            {
                returnString += bit ? "1" : "0"; 
            }
            return returnString;
        }

        IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }
        
        public List<bool> GetAllBits(byte[] byteArray)
        {
            List<bool> boolList = new List<bool>();
            foreach(var by in byteArray)
            {
                boolList.AddRange(GetBits(by));
            }
            return boolList;
        }
    }
}
