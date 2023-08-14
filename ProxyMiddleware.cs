using System.Text;

namespace Proxy;

public class ProxyMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _targetBaseUrl;

    public ProxyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _targetBaseUrl = configuration["TargetBaseUrl"]!;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;

        string targetUrl = request.Path + request.QueryString;
        string proxyUrl = _targetBaseUrl + targetUrl;

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(proxyUrl);
        var originalContent = await response.Content.ReadAsStringAsync();
        var modifiedContent = ModifyContent(originalContent);

        context.Response.StatusCode = (int)response.StatusCode;
        context.Response.ContentType = "text/html";

        await context.Response.WriteAsync(modifiedContent);
    }

    private string ModifyContent(string content)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(content);

        // Find the <body> element
        var bodyElement = doc.DocumentNode.SelectSingleNode("//body");

        if (bodyElement != null)
        {
            foreach (var textNode in bodyElement.DescendantsAndSelf().Where(n => n.NodeType == HtmlAgilityPack.HtmlNodeType.Text))
            {
                var parentTagName = textNode.ParentNode.Name.ToLower();

                if (parentTagName != "script")
                {
                    var newText = ReplaceWordsWithSymbol(textNode.InnerHtml, 6, "™");
                    textNode.InnerHtml = newText;
                }
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    private string ReplaceWordsWithSymbol(string input, int wordLength, string symbol)
    {
        var result = new StringBuilder();
        var words = input.Split(' ');

        foreach (var word in words)
        {
            string modifiedWord = word;

            if (word.Length == wordLength)
            {
                modifiedWord = word + symbol;
            }

            result.Append(modifiedWord + " ");
        }

        return result.ToString().TrimEnd();
    }
}

