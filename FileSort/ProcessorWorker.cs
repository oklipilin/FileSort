using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileSort
{
    public class ProcessorWorker
    {
        private static List<LineItem> CurrentlyProcessedLines;
        public static BlockingCollection<List<LineItem>> JobsForFlush { get; private set; }

        public static void Reset()
        {
            CurrentlyProcessedLines = new List<LineItem>();
            JobsForFlush = new BlockingCollection<List<LineItem>>(Program.LinesListsLimit);
        }

        public Task StartProcessor(string name)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"{name} started");

                while (!FileReaderWorker.FileLineJobs.IsCompleted)
                {
                    if (FileReaderWorker.FileLineJobs.TryTake(out LineItem line, Timeout.Infinite))
                    {
                        var parsedLine = CompareStringHelper.ParseString(line.OriginalLine);

                        if (parsedLine.Number.HasValue)
                        {
                            line.Text = parsedLine.Text;
                            line.Number = parsedLine.Number.Value;

                            CurrentlyProcessedLines.Add(line);

                            if (CurrentlyProcessedLines.Count >= Program.LinesReadLimit)
                            {
                                Console.WriteLine($"{name} Sorted lines moved to flush {CurrentlyProcessedLines.Count} records");
                                Console.WriteLine($"{name} Jobs count left: {FileReaderWorker.FileLineJobs.Count}");

                                // Add jobs to sort and write to the file
                                JobsForFlush.Add(CurrentlyProcessedLines);

                                Console.WriteLine($"{name} ForFlush jobs count: {JobsForFlush.Count}");
                                CurrentlyProcessedLines = new List<LineItem>();
                            }
                        }
                    }
                }

                if (CurrentlyProcessedLines.Count > 0)
                {
                    Console.WriteLine($"{name} Sorted lines moved to flush {CurrentlyProcessedLines.Count} records");
                    JobsForFlush.Add(CurrentlyProcessedLines);
                }

                if (!JobsForFlush.IsAddingCompleted)
                {
                    JobsForFlush.CompleteAdding();
                }
            });
        }
    }
}
