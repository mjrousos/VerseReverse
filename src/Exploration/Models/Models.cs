using VerseReverse.Models;

namespace Exploration.Models;

public record ArticleVerseReference(string Provider, string Url, Books Book, int Chapter, int? Verse);

public record Reference(Books Book, int Chapter, int? Verse, int? EndVerse);
