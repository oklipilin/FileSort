using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSort
{
    public class FileSplitReader
    {
        private const int fileLimitToSplit = 1024 * 1024 * 1024; // 1 GB

        public static string RootPath = "Tmp";

        private ManualResetEvent syncReader;

        public FileSplitReader()
        {
            syncReader = new ManualResetEvent(true);
        }

        /// <summary>
        /// Splits file by first two characters
        /// Each character is a folder
        /// </summary>
        /// <returns></returns>
        public Dictionary<ushort, FileStream> StartSplitter()
        {
            // Leave all streams to re-use
            var streams = new Dictionary<ushort, FileStream>();

            string path = ConfigurationManager.AppSettings["File"];

            if (!File.Exists(path))
            {
                Console.WriteLine($"Input file \"{path}\" does not exist");
                return streams;
            }

            int SplitterLinesLimit = int.Parse(ConfigurationManager.AppSettings["SplitterLinesLimit"]);

            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }

            var fileSize = new FileInfo(path).Length;

            Console.WriteLine($"Splitter start");
            var idx = 0;
            try
            {
                var bufferWithLines = new Dictionary<ushort, SplitJob>();

                using (var file = new StreamReader(path, Encoding.UTF8, true, 4096))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        idx++;
                        var dotIndex = line.IndexOf(". ", StringComparison.Ordinal);

                        if (dotIndex < 0)
                        {
                            // Do not include line since this is not according to format
                            continue;
                        }

                        char c1 = '\0';
                        char c2 = '\0';

                        if (line.Length > dotIndex + 2)
                        {
                            c1 = char.ToLower(line[dotIndex + 2]);
                        }

                        if (line.Length > dotIndex + 3 && fileSize > fileLimitToSplit)
                        {
                            c2 = char.ToLower(line[dotIndex + 3]);
                        }

                        var key = BitConverter.ToUInt16(new byte[] { (byte)c2, (byte)c1 }, 0);

                        var lineToAdd = new SplitLineItem
                        {
                            Line = line,
                            DotIndex = dotIndex
                        };

                        if (!bufferWithLines.ContainsKey(key))
                        {
                            bufferWithLines.Add(key, new SplitJob
                            {
                                WholeKey = key,
                                FirstChar = c1,
                                SecondChar = c2,
                                Lines = new List<SplitLineItem> { lineToAdd }
                            });
                        }
                        else
                        {
                            bufferWithLines[key].Lines.Add(lineToAdd);
                        }

                        // Flush buffer to the file
                        if (idx > SplitterLinesLimit)
                        {
                            // Flush only one buffer at a time
                            syncReader.WaitOne();
                            syncReader.Reset();

                            var tmp = bufferWithLines;
                            _ = Task.Run(() =>
                            {
                                foreach (var buffer in tmp)
                                {
                                    FlushBuf(RootPath, buffer.Value, streams);
                                }

                                Console.WriteLine($"Splitted {tmp.Values.Sum(s => s.Lines.Count)} lines");

                                syncReader.Set();
                            });

                            bufferWithLines = new Dictionary<ushort, SplitJob>();

                            idx = 0;
                        }
                    }
                }

                syncReader.WaitOne();
                foreach (var buffer in bufferWithLines)
                {
                    FlushBuf(RootPath, buffer.Value, streams);
                }

                Console.WriteLine($"Splitted {bufferWithLines.Values.Sum(s => s.Lines.Count)} lines");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Splitter failed on line {idx} with ex: {ex}");
            }

            Console.WriteLine($"Splitter finished");

            return streams;
        }

        private void FlushBuf(
            string rootPath,
            SplitJob job,
            Dictionary<ushort, FileStream> streams)
        {
            var newPath = Path.Combine(rootPath);

            if (job.FirstChar != '\0')
            {
                newPath = Path.Combine(newPath, ((byte)job.FirstChar).ToString("000"));
            }

            if (job.SecondChar != '\0')
            {
                newPath = Path.Combine(newPath, ((byte)job.SecondChar).ToString("000"));
            }

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            newPath = Path.Combine(newPath, Program.TmpFileName);

            if (!streams.ContainsKey(job.WholeKey))
            {
                var fileWriter = File.Open(newPath, FileMode.Create);
                streams.Add(job.WholeKey, fileWriter);
            }

            foreach (var lineItem in job.Lines)
            {
                streams[job.WholeKey].Write(Encoding.UTF8.GetBytes(lineItem.Line), 0, lineItem.Line.Length);
                streams[job.WholeKey].Write(new byte[] { 13, 10 }, 0, 2);
            }
        }
    }
}
