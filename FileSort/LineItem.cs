using System.Numerics;

namespace FileSort
{
    public class LineItem
    {
        public BigInteger Number { get; set; }

        public string Text { get; set; }

        public string OriginalLine { get; set; }

        public int DotIndex { get; set; }

        public string Directory { get; set; }
    }
}
