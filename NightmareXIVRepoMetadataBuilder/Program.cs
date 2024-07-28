using Octokit.Internal;
using Octokit;
using System.Text;

namespace NightmareXIVRepoMetadataBuilder;

#nullable disable

internal class Program
{
    static string? Key = null;
    static void Main(string[] args)
    {
        var envvars = (object[])[.. Environment.GetEnvironmentVariables().Keys];
        Console.WriteLine($"Existing envvars: {string.Join(", ", envvars.Select(x => x.ToString()))}");
        Key = Environment.GetEnvironmentVariable("GH_TOKEN") ?? throw new NullReferenceException("Token is null");
        var pars = ParseParams();
        foreach(var param in pars)
        {
            Console.WriteLine($"{param.Key}={param.Value}");
        }

        var currentRepo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY").Split("/");

        var roCredentials = new InMemoryCredentialStore(new(Key, AuthenticationType.Bearer));
        var github = new GitHubClient(new ProductHeaderValue("NightmareXIVMetadataUpdater"), roCredentials);
        var repo = github.Repository.Get(currentRepo[0], currentRepo[1]).Result;

        StringBuilder builder = new();
        builder.Append($"# {repo.Name}\n");

        if(pars.TryGetValue("description", out var v) && v == "false")
        {
            //
        }
        else
        {
            builder.Append($"{repo.Description}\n");
        }

        foreach(var param in pars)
        {
            var text = github.Connection.GetHtml(new(GetURL(param.Key, param.Value)), new Dictionary<string, string>()).Result.Body
                .Replace("$plugin", repo.Name);
            builder.Append(text);
            if(!text.EndsWith("\n"))
            {
                builder.Append("\n");
            }
        }

        File.WriteAllText("README.md", builder.ToString(), Encoding.UTF8);
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
