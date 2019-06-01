using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Xunit;

public class UnitTest
{
    static readonly string contestName = "";
    static readonly string questionName = "";

    [Theory]
    [MemberData(nameof(GetArgs))]
    public void ProgramTest(string input, string expected)
    {
        using (var inReader = new StringReader(input))
        using (var outWriter = new StringWriter())
        {
            Console.SetIn(inReader);
            Console.SetOut(outWriter);

            Program.Main();

            var actual = outWriter.ToString();

            Assert.Equal(
                expected.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries),
                actual.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public static IEnumerable<object[]> GetArgs()
    {
        var userProfile = Environment.GetEnvironmentVariable("UserProfile");
        var dirPath = Path.Combine(userProfile, "Downloads", "AtCoderCache");
        Directory.CreateDirectory(dirPath);

        var cacheFilePath = Path.Combine(dirPath, $"{questionName}.json");
        var serializer = new JsonSerializer();
        InOut[] inOutArray;
        if (File.Exists(cacheFilePath))
        {
            using (var textReader = File.OpenText(cacheFilePath))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                inOutArray = serializer.Deserialize<InOut[]>(jsonReader);
            }
        }
        else
        {
            inOutArray = CreateInOutFromWeb().Result;
            using (var writer = File.CreateText(cacheFilePath))
            {
                serializer.Serialize(writer, inOutArray);
            }
        }

        return inOutArray.Select(x => x.ToObjArray());
    }

    public static async Task<InOut[]> CreateInOutFromWeb()
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri($"https://atcoder.jp/contests/{contestName}/tasks/");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            using (var stream = await client.GetStreamAsync(questionName))
            {
                var parser = new HtmlParser();
                var dom = await parser.ParseDocumentAsync(stream, default(CancellationToken));
                var h3Array = dom.QuerySelectorAll("h3");

                var inputs = h3Array
                    .Where(x => x.InnerHtml?.StartsWith("入力例") == true)
                    .Select(x => x.NextElementSibling.InnerHtml.TrimEnd());
                var outputs = h3Array
                    .Where(x => x.InnerHtml?.StartsWith("出力例") == true)
                    .Select(x => x.NextElementSibling.InnerHtml.TrimEnd());

                return inputs
                    .Zip(outputs, (i, o) => new InOut { In = i, Out = o })
                    .ToArray();
            }
        }
    }

    public class InOut
    {
        public string In { get; set; }
        public string Out { get; set; }
        public object[] ToObjArray() => new object[] { In, Out };
    }
}
