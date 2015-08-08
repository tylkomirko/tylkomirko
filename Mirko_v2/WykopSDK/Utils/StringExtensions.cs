using System;
using System.Linq;
using System.Collections.Generic;

namespace WykopSDK.Utils
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Get the first several words from the summary.
        /// </summary>
        public static string FirstWords(this string input, int numberWords)
        {
            try
            {
                // Number of words we still want to display.
                int words = numberWords;

                // Loop through entire summary.
                for (int i = 0; i < input.Length; i++)
                {
                    // Increment words on a space.
                    if (input[i] == ' ')
                        words--;

                    // If we have no more words to display, return the substring.
                    if (words == 0)
                        return input.Substring(0, i);
                }

                return input; // numberWords is bigger than actual word count in string.
            }
            catch (Exception)
            {
                // Log the error.
            }

            return string.Empty;
        }

        public static string Between(this string source, int index1, int index2)
        {
            if (string.IsNullOrEmpty(source))
                return "";

            if (index1 == -1 && index2 != -1)
                return source.Substring(0, index2);

            if (index1 != -1 && index2 == -1)
                return source.Substring(index1, source.Length - index1);

            return source.Substring(index1, index2 - index1);
        }

        public static int IndexOf2(this string source, char ch, int startIdx)
        {
            var idx = source.IndexOf(ch, startIdx);
            if (idx == -1)
                return source.Length;
            else
                return idx;
        }

        public static int IndexOf2(this string source, string str, int startIdx)
        {
            var idx = source.IndexOf(str, startIdx);
            if (idx == -1)
                return source.Length;
            else
                return idx;
        }

        public static int IndexOfArray(this string source, string[] array, int startIdx = 0)
        {
            var list = new List<int>(array.Count());

            foreach (var str in array)
                list.Add(source.IndexOf2(str, startIdx));

            return list.Min();
        }
    }
}
