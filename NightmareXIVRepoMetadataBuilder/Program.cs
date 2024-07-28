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
            Console.WriteLine($"{param[0]}={param[1]}");
        }

        var currentRepo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY").Split("/");

        var roCredentials = new InMemoryCredentialStore(new(Key, AuthenticationType.Bearer));
        var github = new GitHubClient(new ProductHeaderValue("NightmareXIVMetadataUpdater"), roCredentials);
        var repo = github.Repository.Get(currentRepo[0], currentRepo[1]).Result;

        StringBuilder builder = new();
        builder.Append($"# {repo.Name}\n");

        if(pars.Any(x => x[0] == "description" && x[1] == "false"))
        {
            //
        }
        else
        {
            builder.Append($"{repo.Description}\n");
        }

        foreach(var param in pars)
        {
            string text;
            if(param[0] == "file")
            {
                text = File.ReadAllText($"meta/{param[1]}.md");
            }
            else
            {
                text = github.Connection.GetHtml(new(GetURL(param[0], param[1]))).Result.Body
                    .Replace("$plugin", repo.Name);
            }
            builder.Append(text);
            if(!text.EndsWith("\n"))
            {
                builder.Append("\n");
            }
        }

        File.WriteAllText("README.md", builder.ToString(), Encoding.UTF8);
        var funding = github.Connection.GetHtml(new("https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/.github/FUNDING.yml")).Result.Body;
        File.WriteAllText(".github/FUNDING.yml", funding);
    }

    static string GetURL(string param, string type)
    {
        return $"https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/meta/{param}/{type}.md";
    }

    static List<string[]> ParseParams()
    {
        var file = File.ReadAllText("meta/config.ini").Split("\n");
        var ret = new List<string[]>();
        foreach(var line in file)
        {
            var s = line.Trim().Split("=", StringSplitOptions.TrimEntries);
            if(s.Length == 2)
            {
                ret.Add(s);
            }
            else if(s.Length == 1)
            {
                if(s[0] != "")
                {
                    ret.Add(["file", s[0]]);
                }
            }
        }
        return ret;
    }
}
