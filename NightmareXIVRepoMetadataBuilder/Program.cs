namespace NightmareXIVRepoMetadataBuilder;

internal class Program
{
    static void Main(string[] args)
    {
        foreach(var param in ParseParams())
        {
            Console.WriteLine($"{param.Key}={param.Value}");
        }
    }

    static string GetURL(string param, string type)
    {
        return $"https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/meta/{param}/{type}.md";
    }

    static Dictionary<string, string> ParseParams()
    {
        var file = File.ReadAllText("meta/config.ini").Split("\n");
        var ret = new Dictionary<string, string>();
        foreach(var line in file)
        {
            var s = line.Trim().Split("=");
            if(s.Length == 2)
            {
                ret.Add(s[0], s[1]);
            }
        }
        return ret;
    }
}
