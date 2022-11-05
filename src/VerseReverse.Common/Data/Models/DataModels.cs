namespace VerseReverse.Data.Models;

internal record Article(int Id, string Provider, string Url);

internal record Reference(int Id, int Book, int Chapter, int? Verse);

internal record ArticleXReference(int ArticleId, int ReferenceId);
