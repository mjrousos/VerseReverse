using Abot2.Crawler;
using Abot2.Poco;
using Exploration.Models;
using VerseReverse;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Initiating crawl...");

var crawler = new DesiringGodMessagesCrawler(Enumerable.Empty<string>());
await foreach (var reference in crawler.GetReferences(new CancellationTokenSource()))
{
    Console.WriteLine("TODO");
}

