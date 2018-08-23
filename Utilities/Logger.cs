using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BoltFreezer.Utilities
{
    public static class Logger
    {
        public static bool Active = true;
        public static string directory;
        public static Dictionary<string, List<Tuple<string, string>>> Database = new Dictionary<string, List<Tuple<string, string>>>();
        public static List<Tuple<string, string>> namedItems = new List<Tuple<string, string>>();
        public static Stopwatch LogTimer = System.Diagnostics.Stopwatch.StartNew();

        public static void SetDirectory(string here)
        {
            directory = here;
        }

        public static void InitiateTimer()
        {
            LogTimer = System.Diagnostics.Stopwatch.StartNew();
        }

        public static void LogTime(string operationName, long timeItTook)
        {
            namedItems.Add(new Tuple<string, string>(operationName, timeItTook.ToString()));
        }

        public static void LogThing(string operationName, string thingToLog)
        {
            namedItems.Add(new Tuple<string, string>(operationName, thingToLog));
        }

        public static void LogTimeToType(string operationName, long timeItTook, string whereToPutIt)
        {
            if (!Database.ContainsKey(whereToPutIt))
            {
                Database[whereToPutIt] = new List<Tuple<string, string>>() { new Tuple<string, string>(operationName, timeItTook.ToString()) };
            }
            else
            {
                Database[whereToPutIt].Add(new Tuple<string, string>(operationName, timeItTook.ToString()));
            }
        }

        public static void LogThingToType(string operationName, string thingToLog, string whereToPutIt)
        {
            if (!Database.ContainsKey(whereToPutIt))
            {
                Database[whereToPutIt] = new List<Tuple<string, string>>() { new Tuple<string, string>(operationName, thingToLog) };
            }
            else
            {
                Database[whereToPutIt].Add(new Tuple<string, string>(operationName, thingToLog));
            }
        }

        public static void WriteItemsFromDatabaseToFile(string nameOfDatabase)
        {
            var file = directory + @"/Times/" + nameOfDatabase + ".txt";
            Directory.CreateDirectory(directory + @"/Times/");
            var data = Database[nameOfDatabase];
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in data)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
                writer.WriteLine("\n");
            }
        }


        public static void WriteTimesToFile(string whatToCallIt)
        {
            var file = directory + @"/Times/" + whatToCallIt + ".txt";
            Directory.CreateDirectory(directory + @"/Times/");
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in namedItems)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
                writer.WriteLine("\n");
            }
        }

        public static long Log()
        {
            return LogTimer.ElapsedMilliseconds;
        }

    }
}
