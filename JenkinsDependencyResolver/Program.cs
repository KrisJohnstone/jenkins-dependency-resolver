using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using StackExchange.Utils;

namespace JenkinsDependencyResolver
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync<Options>(async o =>
                {
                    if (o.Plugins.Any())
                    {
                        if (string.IsNullOrEmpty(o.UpdateCenterURL))
                            o.UpdateCenterURL = "https://updates.jenkins.io/current/update-center.json";

                        var updateCenterJson = await GetUpdateCenter(o.UpdateCenterURL);

                        if (!updateCenterJson.Success) return;

                        var pluginListJson = await GetPluginList(updateCenterJson.Data, o.Plugins);

                        var finalList = await ListPlugins(o.Plugins, pluginListJson);
                        finalList.AddRange(o.Plugins);
                        var listOfPlugins = string.Join(" ", finalList.Distinct().ToArray());
                        
                        await WriteOutPlugins(string.Join(" ",listOfPlugins));
                    }
                });
        }

        public static async ValueTask<List<string>> ListPlugins(IEnumerable<string> plugins, JsonElement listOfPlugins)
        {
            var finalList = new List<string>();
            var tempList = new List<string>();
            foreach (var x in plugins)
            {
                var y = await ReturnDependency(x, listOfPlugins);
                finalList.AddRange(y);
                tempList.AddRange(y);
            }

            foreach (var x in tempList)
            {
                if (!finalList.Contains(x))
                {
                    finalList.AddRange(await ReturnDependency(x, listOfPlugins));
                }
                tempList.Remove(x);
            }

            return finalList;
        }
        public static async ValueTask<HttpCallResponse<string>> GetUpdateCenter(string updateCenter)
        {
            var result = await Http.Request(updateCenter)
                .ExpectString()
                .GetAsync();
            return result;
        }

        public static async ValueTask<JsonElement> GetPluginList(string data, IEnumerable<string> inputPlugins)
        {
            var split = data.Substring(19).TrimEnd(new char[] {')',';'});
            JsonDocument.Parse(split).RootElement.TryGetProperty("plugins", out var plugins);
            return plugins;
        }

        public static async ValueTask<IEnumerable<string>> ReturnDependency(string plugin, JsonElement data)
        {
            var p = data.GetProperty(plugin);
            p.TryGetProperty("dependencies", out var d);
            return d.EnumerateArray().Select(x => x.GetProperty("name").GetString());
        }
        
        public static async ValueTask WriteOutPlugins(string processed)
        {
            using (var outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "plugins.txt")))
            {
                await outputFile.WriteAsync(processed);
            }
        }
    }
}