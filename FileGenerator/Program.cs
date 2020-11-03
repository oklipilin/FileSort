using FileGenerator;
using System;
using System.IO;
using System.Linq;

namespace FileGnerator
{
    class Program
    {
        private static Random rndNumber = new Random();
        private static Random rndCharIndex = new Random();
        private static Random rndStringLength = new Random();
        private static Random rndDuplicate = new Random();

        public static string GetRandomString()
        {
            const string chars = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var stringLength = rndStringLength.Next(
                ConfigurationSettings.TextMinLength,
                ConfigurationSettings.TextMaxLength);

            return new string(Enumerable.Repeat(chars, stringLength)
                .Select(s => s[rndCharIndex.Next(s.Length)]).ToArray());
        }

        private static int GetRandomNumber()
        {
            return rndNumber.Next(
                ConfigurationSettings.NumberMinValue,
                ConfigurationSettings.NumberMaxValue);
        }

        static void Main(string[] args)
        {
            int lastPercentageNotified = 0;

            string lineForDuplicate = string.Empty;

            int generatedFileSize = 0;
            using (var generatedFile = File.Open(ConfigurationSettings.OutFile, FileMode.Create))
            using (var sw = new StreamWriter(generatedFile))
            {
                while (generatedFileSize <= ConfigurationSettings.FileSize)
                {
                    var stringPart = GetRandomString();

                    if (generatedFileSize == 0)
                    {
                        lineForDuplicate = stringPart;
                    }
                    else if (rndDuplicate.Next(1, 101) <= ConfigurationSettings.DuplicatesProbability)
                    {
                        stringPart = lineForDuplicate;
                    }
                    else
                    {
                        lineForDuplicate = stringPart;
                    }

                    var numberPart = GetRandomNumber();

                    var newLine = $"{numberPart}. {stringPart}";
                    sw.WriteLine(newLine);

                    int newPercentage = (int)(generatedFileSize / ConfigurationSettings.FileSize * 100);

                    if (newPercentage != lastPercentageNotified)
                    {
                        Console.WriteLine($"{newPercentage} done");

                        lastPercentageNotified = newPercentage;
                    }

                    generatedFileSize += newLine.Length + 2;
                }

                sw.Close();
                generatedFile.Close();
            }
        }
    }
}
