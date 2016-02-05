using System.Collections.Generic;
using System.Text;

namespace Cow.Net.Core.Extensions
{
    public static class StringExtensions
    {
        public static string CompressToLZW(this string value)
        {
            var dictionary = new Dictionary<string, int>();
            for (var i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);
 
            var w = string.Empty;          
            var compressed = new StringBuilder();

            foreach (char c in value)
            {
                var wc = w + c;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    compressed.Append((char) dictionary[w]);
                    dictionary.Add(wc, dictionary.Count);
                    w = c.ToString();
                }
            }

            if (!string.IsNullOrEmpty(w))
                compressed.Append((char)dictionary[w]);

            return compressed.ToString();
        }

        public static string DecompressLZW(this string value)
        {
            var compressed = new List<int>();
            foreach (var charValue in value)
            {
                compressed.Add((int)charValue);
            }

            var dictionary = new Dictionary<int, string>();
            for (var i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            var w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            var decompressed = new StringBuilder(w);

            foreach (var k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);
                dictionary.Add(dictionary.Count, w + entry[0]);

                w = entry;
            }

            return decompressed.ToString();
        }
    }
}
