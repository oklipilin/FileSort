using System.Collections.Generic;

namespace FileSort
{
    public class SplitJob
    {
        public ushort WholeKey { get; set; }

        public char FirstChar { get; set; }

        public char SecondChar { get; set; }

        public List<SplitLineItem> Lines { get; set; }
    }
}
