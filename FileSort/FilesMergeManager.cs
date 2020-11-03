using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSort
{
    public class FilesMergeManager
    {
        private static int mergedTimes;

        public static BlockingCollection<(string, string)> FilesForMerge { get; private set; }

        public static void Reset()
        {
            mergedTimes = 0;
            FilesForMerge = new BlockingCollection<(string, string)>();
        }

        public Task StartManager()
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"FilesMergeManager started");

                // Wait for two files to merge them into one
                var takenFiles = new List<string>();
                while (!ProcessorWorker.JobsForFlush.IsCompleted 
                    || (FileWriterWorker.ProcessedFiles > mergedTimes))
                {
                    if (takenFiles.Count < 2)
                    {
                        if (FileWriterWorker.SortedFiles.TryTake(out var item, TimeSpan.FromMilliseconds(100)))
                        {
                            takenFiles.Add(item);

                            // If this is the last file then handle it
                            if (FileWriterWorker.ProcessedFiles - 1 == mergedTimes
                                && ProcessorWorker.JobsForFlush.IsCompleted)
                            {
                                Console.WriteLine($"FilesMergeManager last file: {takenFiles.First()}");
                                Interlocked.Increment(ref mergedTimes);
                            }
                        }
                    }
                    else
                    {
                        // If we have two file to merge then add job for it
                        FilesForMerge.Add((takenFiles.FirstOrDefault(), takenFiles.LastOrDefault()));
                        Interlocked.Increment(ref mergedTimes);

                        Console.WriteLine($"FilesMergeManager added to merge files: {string.Join(", ", takenFiles)}");

                        takenFiles.Clear();
                    }
                }

                FilesForMerge.CompleteAdding();

                FileWriterWorker.SortedFiles.CompleteAdding();
                var mergedFile = takenFiles.First();

                var dir = Path.GetDirectoryName(mergedFile);
                var outPath = Path.Combine(dir, Program.OutFileName);

                if (File.Exists(outPath))
                {
                    File.Delete(outPath);
                }

                File.Move(mergedFile, outPath);

                _ = Task.Run(() =>
                {
                    File.Delete(Path.Combine(dir, Program.TmpFileName));
                });

                Console.WriteLine($"FilesMergeManager finished");
            });
        }
    }
}
