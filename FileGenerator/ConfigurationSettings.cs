using System.Configuration;

namespace FileGenerator
{
    public static class ConfigurationSettings
    {
        private const int DefaultTextMaxLength = 1000000;
        private const int DefaultTextMinLength = 1;
        private const string DefaultChars = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static decimal FileSize { get; } = 1024;

        public static string Chars { get; }

        public static string OutFile { get; }

        public static int NumberMinValue { get; } = int.MinValue;

        public static int NumberMaxValue { get; } = int.MaxValue;

        public static int TextMinLength { get; } = 1;

        public static int DuplicatesProbability { get; } = 0;

        public static int TextMaxLength { get; } = DefaultTextMaxLength;

        static ConfigurationSettings()
        {
            Chars = ConfigurationManager.AppSettings["chars"];

            if (string.IsNullOrEmpty(Chars))
            {
                Chars = DefaultChars;
            }

            OutFile = ConfigurationManager.AppSettings["outFile"];

            if (decimal.TryParse(ConfigurationManager.AppSettings["size"], out var size))
            {
                FileSize = size;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["numberMinValue"], out var minVal))
            {
                NumberMinValue = minVal;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["numberMaxValue"], out var maxVal))
            {
                NumberMaxValue = maxVal;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["duplicatesProbability"], out var duplicate))
            {
                if (duplicate > 0 && duplicate < 101)
                {
                    DuplicatesProbability = duplicate;
                }
            }

            if (int.TryParse(ConfigurationManager.AppSettings["textMinLength"], out var minTextLength))
            {
                if (minTextLength >= DefaultTextMinLength && minTextLength < DefaultTextMaxLength)
                {
                    TextMinLength = minTextLength;
                }
            }

            if (int.TryParse(ConfigurationManager.AppSettings["textMaxLength"], out var maxTextLength))
            {
                if (maxTextLength >= minTextLength && maxTextLength <= DefaultTextMaxLength)
                {
                    TextMaxLength = maxTextLength;
                }
                else
                {
                    TextMaxLength = minTextLength + 1;
                }
            }
        }
    }
}
