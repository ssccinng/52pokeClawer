// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;

HttpClient httpClient = new HttpClient();
string ItemListURL = "https://wiki.52poke.com/wiki/%E9%81%93%E5%85%B7%E5%88%97%E8%A1%A8";

string itemHtml = await (await httpClient.GetAsync(ItemListURL)).Content.ReadAsStringAsync();

// Console.WriteLine(itemHtml);
Regex regex = new Regex(@"href=""(?<url>/wiki/.+?)"".+?title=""(?<title>.+?)"">(?<whatthis>.+?)<");
Regex regeximg = new Regex(@"data-url=""(?<url>//media.52poke.com.[^""]+?Sprite.png)""");

List<(string Url, string Title)> itemList = new();

MatchCollection matches = regex.Matches(itemHtml);
foreach (Match match in matches)
{
    GroupCollection groups = match.Groups;
    if (groups["title"].Value.Contains("（道具）"))
        itemList.Add((groups["url"].Value, groups["title"].Value[..^4]));
    // Console.WriteLine($"url = {groups["url"]}, title = {groups["title"]}");
}

if (!Directory.Exists("ItemImg"))
{
    Directory.CreateDirectory("ItemImg");
}

int idx = 0;
List<Task> tasks = new();
foreach (var item in itemList)
{
    // if (idx > 2) break;
    tasks.Add(DownItem(item));
    // Console.WriteLine($"url = {item.Url}, title = {item.Title}");
    idx++;
}
foreach (var item in tasks)
{
    await item;
}
async Task DownItem((string Url, string Title) item)
{
    try
    {
    
        string text = await (await httpClient.GetAsync($"https://wiki.52poke.com{item.Url}")).Content.ReadAsStringAsync();
        MatchCollection matches = regeximg.Matches(text);
        int idx = 0;
        foreach (Match match in matches)
        {
            GroupCollection groups = match.Groups;
            if(idx == 1 && !groups["url"].Value.Contains("Dream")) continue;
            // Console.WriteLine();
            await File.WriteAllBytesAsync($"ItemImg/{item.Title}-{idx}.png", await (await httpClient.GetAsync($"https:{groups["url"]}")).Content.ReadAsByteArrayAsync());

            idx++;
            if (idx == 2) break;

            // Console.WriteLine($"url = {groups["url"]}, title = {groups["title"]}");
        }
        Console.WriteLine($"{item.Title} 完成");
    }
    catch (System.Exception ex)
    {
        Console.WriteLine($"{item.Title} 线程炸裂！！！！！");
    }
}