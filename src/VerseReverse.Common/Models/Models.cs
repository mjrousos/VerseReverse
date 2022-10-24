namespace VerseReverse.Models;

public record ArticleVerseReference(string Provider, string Url, Books Book, int Chapter, int? Verse);

public record Reference(Books Book, int Chapter, int? Verse, int? EndVerse)
{
    public override string ToString() =>
        $"{Book.ToDisplayString()} {Chapter}{(Verse.HasValue ? $"{Verse}{(EndVerse.HasValue ? $"-{EndVerse}" : string.Empty)}" : string.Empty)}";
}
