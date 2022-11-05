using VerseReverse.Crawlers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Initiating crawl...");

var crawler = new DesiringGodMessagesCrawler();
await foreach (var reference in crawler.GetReferences(Enumerable.Empty<string>(), new CancellationTokenSource()))
{
    Console.WriteLine($"[{reference.Provider}] {reference.Url}: {reference}");
}
