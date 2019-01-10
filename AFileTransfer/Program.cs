using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AFileTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ourPlayer = new FrequencyPlayer(1.0, 1.1);
            var ourListener = new FrequencyListener(1, 1, 0);
            var rng = new Random();
            var weAreCorrect = true;
            
            var file = @"Q:\Toolz\Csharp-Data-Visualization\projects\17-07-16_microphone\Test.txt";
            // ourPlayer.StartTone();
            var playingString = ourPlayer.GetFileString(file);
            //playingString = "01";
            while (weAreCorrect)
            {
               // playingString += rng.NextDouble() > .5 ? "0" : "1";
                ourListener.Listen();
                ourPlayer.PlayString(playingString);
                ourListener.StopListening();

                // ourListener.ReportOnData();
                //  var prediction = Analyser.MakePredictionString(ourListener.recordingTime, ourPlayer.StartTime);
                //Console.WriteLine("Our Test: " + prediction);
                weAreCorrect = ourListener.OutputData() == playingString;
                Console.WriteLine($"Original String:\t\t{playingString}");
                Console.WriteLine("Second Prediction:\t\t" + ourListener.OutputData());
                Console.WriteLine($"Correct :\t\t{weAreCorrect}");
                ourListener.ConvertOutputToFile(@"Q:\ToDeleteEveryDay\Test.txt");
                Console.ReadLine();

            }
        }
    }
}
