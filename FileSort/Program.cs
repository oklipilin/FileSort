using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSort
{
    class Program
    {
        public const string OutFileName = "output.txt";
        public const string TmpFileName = "1.txt";

        public static int LinesReadLimit = 300_000;
        public static int LinesListsLimit = 3;

        static Program()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["LinesReadLimit"], out var readLimit))
            {
                LinesReadLimit = readLimit;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["LinesListsLimit"], out var listsLimit))
            {
                LinesListsLimit = listsLimit;
            }
        }

        static async Task Main(string[] args)
        {
            using (var standardOutput = new StreamWriter("logs.txt", false))
            {
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);

                try
                {
                    if (File.Exists(OutFileName))
                    {
                        File.Delete(OutFileName);
                    }

                    if (Directory.Exists(FileSplitReader.RootPath))
                    {
                        Directory.Delete(FileSplitReader.RootPath, true);
                    }

                    var sw = new Stopwatch();
                    sw.Start();

                    // Split files by first two chars
                    var splitReaderManager = new FileSplitReader();
                    var fileStreams = splitReaderManager.StartSplitter();

                    if (fileStreams.Count > 0)
                    {
                        // Get all files created after Splitter
                        var files = Directory.GetFiles(FileSplitReader.RootPath, TmpFileName, SearchOption.AllDirectories);

                        // Process each file separately
                        foreach (var item in files)
                        {
                            await ProcessFile(fileStreams, item);
                        }

                        // Combine initially splitted files
                        CombineFiles();
                    }

                    try
                    {
                        Directory.Delete(FileSplitReader.RootPath, true);
                    }
                    catch { }

                    sw.Stop();
                    Console.WriteLine($"Time elapsed: {sw.Elapsed.TotalSeconds} seconds");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                standardOutput.Close();
            }
        }

        private static async Task ProcessFile(
            Dictionary<ushort, FileStream> fileStreams,
            string item)
        {
            FileWriterWorker.Reset();
            FileReaderWorker.Reset();
            ProcessorWorker.Reset();
            FilesMergeManager.Reset();

            // Read file and pass line to process
            var readerWorker = new FileReaderWorker(fileStreams);
            var reader = readerWorker.StartFileReader(item);

            // Process each line and combine them into one set
            // Split file into multi if exceeds LinesReadLimit
            var processorWorker = new ProcessorWorker();
            var processor = processorWorker.StartProcessor("[Proessor 1]");

            // Order lines and write them into a file
            var writeWorker = new FileWriterWorker();
            var writer = writeWorker.StartWriter("[Writer 1]");

            // Manage jobs queue for merging splitted files
            var filesMergeManager = new FilesMergeManager();
            var manager = filesMergeManager.StartManager();

            // If file was splitted then merge it into one
            var fileMergeWorker = new FileMergeWorker();
            var mergeWorker = fileMergeWorker.StartMergeWorker("[Merger 1]");

            await Task.WhenAll(reader, processor, writer, manager, mergeWorker);
        }

        private static void CombineFiles()
        {
            var outFiles = Directory.GetFiles(FileSplitReader.RootPath, OutFileName, SearchOption.AllDirectories)
                .OrderBy(f => Path.GetDirectoryName(f));

            using (Stream destStream = File.Open(OutFileName, FileMode.Create))
            {
                foreach (string srcFileName in outFiles)
                {
                    using (Stream srcStream = File.OpenRead(srcFileName))
                    {
                        srcStream.CopyTo(destStream);
                        srcStream.Close();
                    }
                }

                destStream.Close();
            }
        }
    }
}
