using EnigmaContest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EnigmaContest.Code
{
    public static class Helpers
    {
        public static char[] delims = { ' ', ',', '!', ';', ':', '?' };
        private static Random r = new Random();
        public static T Choice<T>(this List<T> lst)
        {
            return lst[r.Next(0, lst.Count - 1)];
        }

        public static double FuzzyCompare2(this string left, string right)
        {
            return CompareDictionary(left.ToDecomposedDict(), right.ToDecomposedDict());
        }

        public static double FuzzyCompare(this string one, string two)
        {
            one = one.Purge();
            two = two.Purge();
            return ((double)LongestCommonSubstring(one, two).Length) / Math.Max(one.Length, two.Length);
        }

        public static string Purge(this string input)
        {
            string[] remove = new string[] { "-", ",", "."," "};
            foreach (var c in remove) input = input.Replace(c, "");
            return input;
        }

        public static string LongestCommonSubstring(string source, string target)
        {
            if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target)) { return null; }
            source = source.ToUpper();
            target = target.ToUpper();

            int[,] L = new int[source.Length, target.Length];
            int maximumLength = 0;
            int lastSubsBegin = 0;
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                for (int j = 0; j < target.Length; j++)
                {
                    if (source[i] != target[j])
                    {
                        L[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                            L[i, j] = 1;
                        else
                            L[i, j] = 1 + L[i - 1, j - 1];

                        if (L[i, j] > maximumLength)
                        {
                            maximumLength = L[i, j];
                            int thisSubsBegin = i - L[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                stringBuilder.Append(source[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                stringBuilder.Length = 0; //clear it
                                stringBuilder.Append(source.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private static Dictionary<string, int> ToDecomposedDict(this string input)
        {
            var w = input
                        .ToLower()
                        .Split(delims, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => string.Join("", a.ToCharArray().OrderBy(b => b)))
                        .ToList();
            var d = new Dictionary<string, int>();
            foreach (var s in w)
            {
                if (d.ContainsKey(s)) d[s]++;
                else d[s] = 1;
            }

            return d;
        }

        private static double CompareDictionary(Dictionary<string, int> left, Dictionary<string, int> right)
        {
            var bd = left.Count > right.Count ? left : right;
            var sd = left.Count > right.Count ? right : left;
            int hits = 0; int totals = 0;
            foreach (var k in bd)
            {
                if (sd.ContainsKey(k.Key))
                {
                    hits += Math.Min(k.Value, sd[k.Key]);
                    totals += Math.Max(k.Value, sd[k.Key]);
                }
                else
                {
                    totals += k.Value;
                }

            }
            return 1.0 * hits / totals;
        }

    }
}