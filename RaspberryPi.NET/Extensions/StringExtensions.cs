using System;
using System.Linq;

namespace RaspberryPi.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        ///   <para>Splits given string on a newline (<see cref="Environment.NewLine"/>) character into array of strings.</para>
        ///   <para>If source string is <see cref="string.Empty"/>, an empty array is returned.</para>
        /// </summary>
        /// <param name="self">Source string to be splitted.</param>
        /// <returns>Target array of strings, which are part of <paramref name="self"/> string.</returns>
        public static string[] Lines(this string value)
        {
            return string.IsNullOrEmpty(value)
                ? Enumerable.Empty<string>().ToArray()
                : value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        public static string Line(this string value, int index)
        {
            value ??= string.Empty;

            var lines = value.Lines();
            var maxIndex = lines.Length - 1;
            if (index > maxIndex)
            {
                throw new IndexOutOfRangeException($"Line index {index} exceeds maximum line index {maxIndex}");
            }

            var line = lines[index];
            return line;
        }
    }
}
