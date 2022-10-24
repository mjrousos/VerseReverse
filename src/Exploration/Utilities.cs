using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exploration.Models;

namespace VerseReverse;

public static class Utilities
{
    public static readonly Regex VersesRegex = new(
        @"(?<Book>Song of Songs|Song of Solomon|((First|Second|1|2|I|II) )?[a-z]+) *(?<Chapter>\d{1,3}) *(: *(?<Verse>\d+)((-|–)(?<EndVerse>\d+))?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex ChapterRegex = new(
        @"(?<Book>Song of Songs|Song of Solomon|((First|Second|1|2|I|II) )?[a-z]+) *(?<Chapter>\d{1,3})(?!(\d|\s)*:)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IEnumerable<Reference> GetPassages(this string str)
    {
        foreach (Match match in VersesRegex.Matches(str))
        {
            var book = match.Groups["Book"].Value.ToBookName();

            if (book.HasValue)
            {
                var chapter = int.Parse(match.Groups["Chapter"].Value);
                var verse = int.Parse(match.Groups["Verse"].Value);
                var endVerseString = match.Groups["EndVerse"].Value;
                int? endVerse = string.IsNullOrEmpty(endVerseString)
                    ? null
                    : int.Parse(endVerseString);

                yield return new Reference(book.Value, chapter, verse, endVerse);
            }
        }

        foreach (Match match in ChapterRegex.Matches(str))
        {
            var book = match.Groups["Book"].Value.ToBookName();

            if (book.HasValue)
            {
                var chapter = int.Parse(match.Groups["Chapter"].Value);

                yield return new Reference(book.Value, chapter, null, null);
            }
        }
    }
}
