using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSort
{
    public class FileMergeWorker
    {
        public Task StartMergeWorker(string name)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"{name} started");

                var fileName = string.Empty;
                while (!FilesMergeManager.FilesForMerge.IsCompleted)
                {
                    if (FilesMergeManager.FilesForMerge.TryTake(out var items, Timeout.Infinite))
                    {
                        // Merge two sorted files
                        fileName = MergeSort(items.Item1, items.Item2);

                        // When files are merged return them back to the queue for merge
                        // until all files are merged
                        Console.WriteLine($"{name} merged: {items.Item1}, {items.Item2}");
                        FileWriterWorker.SortedFiles.Add(fileName);
                    }
                }

                Console.WriteLine($"{name} finished");
            });
        }

        private string MergeSort(string fileOne, string fileTwo)
        {
            var dir = Path.GetDirectoryName(fileOne);
            var result = Path.Combine(dir, $"{Guid.NewGuid()}.txt");

            using (var firstFile = new StreamReader(fileOne, Encoding.UTF8, true, 4096))
            using (var secondFile = new StreamReader(fileTwo, Encoding.UTF8, true, 4096))
            {
                string lineOne, lineTwo;
                using (var target = new StreamWriter(result, false, new UTF8Encoding(false), 4096))
                {
                    lineOne = firstFile.ReadLine();
                    lineTwo = secondFile.ReadLine();

                    // Write to the target file line by line the smallest item
                    while (
                        !string.IsNullOrEmpty(lineOne)
                        || !string.IsNullOrEmpty(lineTwo))
                    {
                        if (lineOne != null && (!lineOne.IsGreaterThan(lineTwo) || lineTwo == null))
                        {
                            target.WriteLine(lineOne);
                            lineOne = firstFile.ReadLine();
                        }
                        else
                        {
                            if (lineTwo != null)
                            {
                                target.WriteLine(lineTwo);
                                lineTwo = secondFile.ReadLine();
                            }
                        }
                    }

                    target.Close();
                }

                secondFile.Close();
                firstFile.Close();
            }

            _ = Task.Run(() =>
            {
                File.Delete(fileOne);
                File.Delete(fileTwo);
            });

            return result;
        }
    }
}
