namespace VerseReverse.Models;

public record Article(string Provider, string Url);

public record ArticleReference(string Provider, string Url, Reference Reference);

public record Reference(Books Book, int Chapter, int? Verse, int? EndVerse)
{
    public override string ToString() =>
        $"{Book.ToDisplayString()} {Chapter}{(Verse.HasValue ? $":{Verse}{(EndVerse.HasValue ? $"-{EndVerse}" : string.Empty)}" : string.Empty)}";
}
