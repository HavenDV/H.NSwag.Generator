using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace H.NSwag.Generator.Core.Extensions
{
    /// <summary>
    /// Extensions that work with <see langword="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Retrieves the strings between the starting fragment and the ending.
        /// All available fragments are retrieved.
        /// <para/>Returns empty <see cref="List{T}"/> if nothing is found.
        /// <para/>Default <paramref name="comparison"/> is <see cref="StringComparison.Ordinal"/>.
        /// <![CDATA[Version: 1.0.0.1]]>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static IEnumerable<(int Start, int Length)> ExtractAllIndexes(this string text, string start, string end, StringComparison? comparison = null)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            start = start ?? throw new ArgumentNullException(nameof(start));
            end = end ?? throw new ArgumentNullException(nameof(end));

            var index2 = -end.Length;
            while (true)
            {
                var index1 = text.IndexOf(start, index2 + end.Length, comparison ?? StringComparison.Ordinal);
                if (index1 < 0)
                {
                    yield break;
                }

                index1 += start.Length;
                index2 = text.IndexOf(end, index1, comparison ?? StringComparison.Ordinal);
                if (index2 < 0)
                {
                    yield break;
                }

                yield return (index1, index2 - index1);
            }
        }

        /// <summary>
        /// Retrieves the strings between the starting fragment and the ending.
        /// All available fragments are retrieved.
        /// <para/>Returns empty <see cref="List{T}"/> if nothing is found.
        /// <para/>Default <paramref name="comparison"/> is <see cref="StringComparison.Ordinal"/>.
        /// <![CDATA[Version: 1.0.0.4]]>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static IEnumerable<string> ExtractAll(this string text, string start, string end, StringComparison? comparison = null)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            start = start ?? throw new ArgumentNullException(nameof(start));
            end = end ?? throw new ArgumentNullException(nameof(end));

            return text
                .ExtractAllIndexes(start, end, comparison)
                .Select(index => text.Substring(index.Start, index.Length));
        }
    }
}
