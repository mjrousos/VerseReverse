using VerseReverse.Models;

namespace VerseReverse;

public static class BooksExtensions
{
    public static Books? ToBookName(this string str)
    {
        var normalizedName = str
            .ToUpperInvariant()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("1", "FIRST", StringComparison.Ordinal)
            .Replace("2", "SECOND", StringComparison.Ordinal)
            .Replace("3", "THIRD", StringComparison.Ordinal);

        Books? book = normalizedName switch
        {
            "GEN" => Books.Genesis,

            // TODO: Add additional abbreviations

            "PSALM" => Books.Psalms,
            "SONGOFSONGS" => Books.SongOfSolomon,
            _ => null,
        };

        if (book is null && Enum.TryParse<Books>(normalizedName, true, out var parsedBook))
        {
            book = parsedBook;
        }

        return book;
    }

    public static string ToDisplayString(this Books book) =>
            book switch
            {
                Books.FirstSamuel => "1 Samuel",
                Books.SecondSamuel => "2 Samuel",
                Books.FirstKings => "1 Kings",
                Books.SecondKings => "2 Kings",
                Books.FirstChronicles => "1 Chronicles",
                Books.SecondChronicles => "2 Chronicles",
                Books.SongOfSolomon => "Song of Solomon",
                Books.FirstCorinthians => "1 Corinthians",
                Books.SecondCorinthians => "2 Corinthians",
                Books.FirstThessalonians => "1 Thessalonians",
                Books.SecondThessalonians => "2 Thessalonians",
                Books.FirstTimothy => "1 Timothy",
                Books.SecondTimothy => "2 Timothy",
                Books.FirstPeter => "1 Peter",
                Books.SecondPeter => "2 Peter",
                Books.FirstJohn => "1 John",
                Books.SecondJohn => "2 John",
                Books.ThirdJohn => "3 John",
                _ => book.ToString(),
            };
}
