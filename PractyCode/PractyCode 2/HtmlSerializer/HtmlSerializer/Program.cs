// See https://aka.ms/new-console-template for more information

using HtmlSerializer;
using System.Text.RegularExpressions;

var html = await Load("https://hebrewbooks.org/beis");

var cleanHtml = new Regex("\\s+").Replace(html, " ");
var htmlLines = new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0);

List<string> htmlElements = htmlLines.ToList();

HtmlElement rootElement = BuildTree(htmlElements);


HtmlElement BuildTree(List<string> htmlLines)
{
    HtmlElement root = new HtmlElement();
    HtmlElement currentElement = root;

    foreach (var htmlLine in htmlLines)
    {
        if (htmlLine == "/html")
            break;
        if (htmlLine[0] == '/' && htmlLine[1] != '*')
            currentElement = currentElement.Parent;
        else
        {
            var tagName = htmlLine.Split(' ')[0];
            bool containsTagName = HtmlHelper.Instance.HtmlTags.Contains(tagName) && htmlLine[htmlLine.Length - 1] != '/';
            bool containsVoidTagName = HtmlHelper.Instance.HtmlVoidTags.Contains(tagName);
            if (containsTagName || containsVoidTagName)
            {
                HtmlElement newElement = createHtmlElement(tagName, htmlLine);
                currentElement.Children.Add(newElement);
                newElement.Parent = currentElement;
                if (containsTagName)
                    currentElement = newElement;
            }
            else
                currentElement.InnerHtml = htmlLine;
        }
    }
    return root.Children[0];
}

HtmlElement createHtmlElement(string tagName, string htmlLine)
{
    HtmlElement element = new HtmlElement();
    element.Name = tagName;
    var attributes = new Regex("\\b(\\w+)\\s*=\\s*\"([^\"]*)\"").Matches(htmlLine).Cast<Match>().ToList();
    var id = new Regex("id=\"(.*?)\"").Matches(htmlLine).ToList();
    if (id.Count() > 0)
        element.Id = id.ElementAt(0).Value.Substring(4, id.ElementAt(0).Value.Length - 5);
    foreach (var attribute in attributes)
    {
        string attributeName = attribute.Groups[1].Value;
        string attributeValue = attribute.Groups[2].Value;
        List<string> classes = new List<string>();
        if (attributeName.Equals("class", StringComparison.OrdinalIgnoreCase))
        {
            string[] wordsArr = attributeValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            classes.AddRange(wordsArr);
            element.Classes = classes;
        }
        element.Attributes.Add(attribute.ToString());
    }
    return element;
}

async Task<string> Load(string url)
{
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}

Console.WriteLine("1) head :");
HashSet<HtmlElement> matchingElements = rootElement.FindElement(Selector.ParseToQuerySelector("head"));
foreach (HtmlElement element in matchingElements)
    Console.WriteLine(element.ToString());
Console.WriteLine("------------------------------");
Console.WriteLine("2) button :");
matchingElements = rootElement.FindElement(Selector.ParseToQuerySelector("button"));
foreach (HtmlElement element in matchingElements)
    Console.WriteLine(element.ToString());
Console.WriteLine("------------------------------");
Console.WriteLine("3) div p :");
matchingElements = rootElement.FindElement(Selector.ParseToQuerySelector("div p"));
foreach (HtmlElement element in matchingElements)
    Console.WriteLine(element.ToString());

Console.ReadKey();