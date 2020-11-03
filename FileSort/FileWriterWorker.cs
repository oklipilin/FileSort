using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSort
{
    public class FileWriterWorker
    {
        private static int processedFiles;

        public static int ProcessedFiles { get => processedFiles; private set => processedFiles = value; }
        public static BlockingCollection<string> SortedFiles { get; private set; }

        public static void Reset()
        {
            processedFiles = 0;
            SortedFiles = new BlockingCollection<string>();
        }

        public Task StartWriter(string name)
        {
            return Task.Run(async () =>
            {
                while (!ProcessorWorker.JobsForFlush.IsCompleted)
                {
                    Console.WriteLine($"{name} Wait for flush");

                    Interlocked.Increment(ref processedFiles);
                    if (ProcessorWorker.JobsForFlush.TryTake(out var item, Timeout.Infinite))
                    {
                        Console.WriteLine($"{name} Allowed flush");

                        var orderedResult = item
                            .OrderBy(i => i.Text)
                            .ThenBy(i => i.Number);

                        int count = 0;
                        var dir = item.FirstOrDefault()?.Directory;
                        var tmpFileName = Path.Combine(dir, $"{Guid.NewGuid()}.txt");

                        using (var fileWriter = File.OpenWrite(tmpFileName))
                        using (var streamWriter = new StreamWriter(fileWriter, new UTF8Encoding(false), 4096))
                        {
                            foreach (var line in orderedResult.Select(i => i.OriginalLine))
                            {
                                count++;
                                await streamWriter.WriteLineAsync(line);
                            }

                            streamWriter.Close();
                            fileWriter.Close();
                        }

                        SortedFiles.Add(tmpFileName);

                        item.Clear();
                        Console.WriteLine($"{name} Flushed {count} lines.");
                    }
                    else
                    {
                        Interlocked.Decrement(ref processedFiles);
                    }
                }

                Console.WriteLine($"{name} finished");
            });
        }
    }
}
