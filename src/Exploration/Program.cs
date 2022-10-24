using VerseReverse;
using VerseReverse.Crawlers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Initiating crawl...");

var crawler = new DesiringGodMessagesCrawler(Enumerable.Empty<string>());
await foreach (var reference in crawler.GetReferences(new CancellationTokenSource()))
{
    Console.WriteLine($"[{reference.Provider}] {reference.Url}: {reference.Book.ToDisplayString()} {reference.Chapter}{(reference.Verse.HasValue ? $":{reference.Verse}" : string.Empty)}");
}
