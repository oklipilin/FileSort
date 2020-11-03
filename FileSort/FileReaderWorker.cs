using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileSort
{
    public class FileReaderWorker
    {
        private Dictionary<ushort, FileStream> fileStreams;

        public FileReaderWorker(Dictionary<ushort, FileStream> fileStreams)
        {
            this.fileStreams = fileStreams;
        }

        public static BlockingCollection<LineItem> FileLineJobs { get; private set; }

        public static void Reset()
        {
            FileLineJobs = new BlockingCollection<LineItem>(Program.LinesReadLimit);
        }

        public Task StartFileReader(string filePath)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine($"FileReader started");
                try
                {
                    var directory = Path.GetDirectoryName(filePath);

                    var dirs = directory.Split('\\');

                    byte dir1 = byte.Parse(dirs[1]);
                    byte dir2 = 0;

                    if (dirs.Length > 2)
                    {
                        dir2 = byte.Parse(dirs[2]);
                    }

                    var key = BitConverter.ToUInt16(new byte[] { dir2, dir1 }, 0);

                    // Re-use already opened streams
                    fileStreams[key].Seek(0, SeekOrigin.Begin);

                    using (var file = new StreamReader(fileStreams[key], Encoding.UTF8, true, 4096))
                    {
                        string line;
                        while ((line = await file.ReadLineAsync()) != null)
                        {
                            // Add job to preprocess line
                            FileLineJobs.Add(new LineItem
                            {
                                OriginalLine = line,
                                Directory = directory
                            });
                        }
                    }

                    FileLineJobs.CompleteAdding();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reader failed with ex: {ex}");
                }

                Console.WriteLine($"FileReader finished");
            });
        }
    }
}
