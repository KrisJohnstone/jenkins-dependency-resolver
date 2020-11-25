using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using StackExchange.Utils;

namespace JenkinsDependencyResolver
{
    public class Program
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

                        var updateCenterJSON = await GetUpdateCenter(o.UpdateCenterURL);

                        if (!updateCenterJSON.Success) return;
                        
                        var processed = await GetPluginList(updateCenterJSON.Data, o.Plugins);
                        await WriteOutPlugins(string.Join(" ",processed));
                    }
                });
        }

        public static async Task<HttpCallResponse<string>> GetUpdateCenter(string updateCenter)
        {
            var result = await Http.Request(updateCenter)
                .ExpectString()
                .GetAsync();
            return result;
        }

        public static async Task<string[]> GetPluginList(string data, IEnumerable<string> inputPlugins)
        {
            var split = data.Substring(19).TrimEnd(new char[] {')',';'});
            JsonDocument.Parse(split).RootElement.TryGetProperty("plugins", out var plugins);
            var pluginsList = new List<string>();
            foreach (var plugin in inputPlugins)
            {
                var p = plugins.GetProperty(plugin);
                //plugins.TryGetProperty(plugin, out var p);
                p.TryGetProperty("dependencies", out var d);
                pluginsList.Add(plugin);
                pluginsList.AddRange(d.EnumerateArray().Select(x => x.GetProperty("name").GetString()));
            }
            return pluginsList.ToArray();
        }

        public static async Task WriteOutPlugins(string processed)
        {
            using (var outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "plugins.txt")))
            {
                await outputFile.WriteAsync(processed);
            }
        }
    }
}