using System.Numerics;

namespace FileSort
{
    public static class CompareStringHelper
    {
        public static CompareLines ParseString(string line)
        {
            var res = new CompareLines();

            if (line == null)
            {
                return res;
            }
            
            var dotIndex = line.IndexOf(". ", System.StringComparison.Ordinal);

            if (dotIndex > -1)
            {
                var numPart = line.Substring(0, dotIndex);
                var textPart = line.Substring(dotIndex + 2);

                res.Text = textPart;

                if (long.TryParse(numPart, out long num))
                {
                    res.Number = num;
                }
                else
                {
                    if (BigInteger.TryParse(numPart, out BigInteger bigInt))
                    {
                        res.Number = bigInt;
                    }
                }
            }

            return res;
        }

        public static bool IsGreaterThan(this string x, string y)
        {
            var line1Test = ParseString(x);
            var line2Test = ParseString(y);

            var stringCompare = line1Test.Text.CompareTo(line2Test.Text);

            if (stringCompare > 0 || (stringCompare == 0 && line1Test.Number > line2Test.Number))
            {
                return true;
            }

            return false;
        }
    }
}
