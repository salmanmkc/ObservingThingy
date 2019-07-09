using System;
using System.Collections.Generic;
using System.IO;

namespace ObservingThingy
{
    public static class StringExtensionMethods
    {
        // https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines/41176852#41176852
        public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = true)
        {
            using (var sr = new StringReader(str))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (removeEmptyLines && String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    yield return line;
                }
            }
        }
    }
}
