using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFileTransfer
{
    static public class Analyser
    {
        public static string MakePredictionString(Dictionary<DateTime, bool> list, DateTime startTime)
        {
            var returnedString = "";
            
            while (list.First().Key > startTime)
            {
                list.Remove(list.First().Key);
            }

            DateTime firstTime = list.First().Key;
            double counter = 0;
            double numberTrue = 0;

            foreach(var item in list)
            {
                if ((item.Key - firstTime).TotalMilliseconds >= 1000)
                {
                    returnedString += (numberTrue / counter >= .50) ? "0" : "1";
                    counter = 0;
                    numberTrue = 0;
                    firstTime = item.Key;
                    continue;
                }
                counter++;
                if (item.Value)
                {
                    numberTrue++;
                }
            }

            return returnedString;
        } 

    }
}
