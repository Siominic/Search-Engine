using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using HtmlAgilityPack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private static Seed seed;
    private static Queue queue;
    private static Crawled crawled;

    static async Task Main(string[] args)
    {
        Initialize();

        await Crawl();
    }
    static void Initialize()
    {
        string path = Directory.GetCurrentDirectory();
        string seedPath = Path.Combine(path, "Seed.txt");
        string queuePath = Path.Combine(path, "Queue.txt");
        string crawledPath = Path.Combine(path, "Crawled.txt");

        seed = new(seedPath);
        var seedURLs = seed.Items;
        queue = new(queuePath, seedURLs);
        crawled = new(crawledPath);
    }
    static async Task Crawl()
    {
        do
        {
            string url = queue.Top;

            Crawl crawl = new(url);
            await crawl.Start();

            if (crawl.parsedURLs.Count > 0)
                await ProcessURLs(crawl.parsedURLs);

            await PostCrawl(url);

        } while (queue.HasURLs);
    }
    static async Task ProcessURLs(List<string> urls)
    {
        foreach (var url in urls)
        {
            if (!crawled.HasBeenCrawled(url) && !queue.IsInQueue(url))
                await queue.Add(url);
        }
    }
    static async Task PostCrawl(string url)
    {
        await queue.Remove(url);

        await crawled.Add(url);
    }
}
class Seed
{
    /// <summary>
    /// Returns all seed URLs.
    /// </summary>
    public string[] Items
    {
        get => File.ReadAllLines(path);
    }

    private readonly string path;

    public Seed(string path)
    {
        this.path = path;

        string[] seedURLs = new string[]
        {
            "https://crawler-test.com/"
        };

        using StreamWriter file = File.CreateText(path);

        foreach (string url in seedURLs)
            file.WriteLine(url.ToCleanURL());
    }
}
class Queue
{
    /// <summary>
    /// Returns the first item in the queue.
    /// </summary>
    public string Top
    {
        get => File.ReadAllLines(path).First();
    }

    /// <summary>
    /// Returns all items in the queue;
    /// </summary>
    public string[] All
    {
        get => File.ReadAllLines(path);
    }

    /// <summary>
    /// Returns a value based on whether there are URLs in the queue.
    /// </summary>
    public bool HasURLs
    {
        get => File.ReadAllLines(path).Length > 0;
    }

    private readonly string path;

    public Queue(string path, string[] seedURLs)
    {
        this.path = path;

        using StreamWriter file = File.CreateText(path);

        foreach (string url in seedURLs)
            file.WriteLine(url.ToCleanURL());
    }

    public async Task Add(string url)
    {
        using StreamWriter file = new(path, append: true);

        await file.WriteLineAsync(url.ToCleanURL());
    }

    public async Task Remove(string url)
    {
        IEnumerable<string> filteredURLs = All.Where(u => u != url);

        await File.WriteAllLinesAsync(path, filteredURLs);
    }

    public bool IsInQueue(string url) => All.Where(u => u == url).Any();
}
class Crawled
{
    private readonly string path;

    public Crawled(string path)
    {
        this.path = path;
        File.Create(path).Close();
    }

    public bool HasBeenCrawled(string url) => File.ReadAllLines(path).Any(c => c == url.ToCleanURL());

    public async Task Add(string url)
    {
        using StreamWriter file = new(path, append: true);

        await file.WriteLineAsync(url.ToCleanURL());
    }
}
static class StringExtensions
{
    public static string ToCleanURL(this string str) => str.Trim().ToLower();
}
class Crawl
{
    public readonly string url;
    private string webPage;
    public List<string> parsedURLs;

    public Crawl(string url)
    {
        this.url = url;
        webPage = null;
        parsedURLs = new List<string>();
    }

    public async Task Start()
    {
        await GetWebPage();

        if (!string.IsNullOrWhiteSpace(webPage))
        {
            ParseContent();
            ParseURLs();
        }
    }

    public async Task GetWebPage()
    {

        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => cert.Verify();

        // Pass the handler to httpclient(from you are calling api)
        HttpClient client = new HttpClient(clientHandler);

        client.Timeout = TimeSpan.FromSeconds(60);

        string responseBody = await client.GetStringAsync(url);

        if (!string.IsNullOrWhiteSpace(responseBody))
            webPage = responseBody;
        Console.WriteLine(webPage);


    }

    public void ParseURLs()
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        // Use this to manually input some HTML
        // Change this into a file location to yknow actually like :)))))
        htmlDoc.LoadHtml(webPage);

        foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
        {
            string hrefValue = link.GetAttributeValue("href", string.Empty);

            if (hrefValue.StartsWith("http"))
                parsedURLs.Add(hrefValue);
        }
    }

    public void ParseContent()
    {

    }
}

static class Processed
{

}
