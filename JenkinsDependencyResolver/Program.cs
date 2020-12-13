using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using JenkinsDependencyResolver.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
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

                        string updateCenterJson;
                        if (string.IsNullOrEmpty(o.File))
                        {
                            var updateCenterResponse = await GetUpdateCenter(o.UpdateCenterURL);
                            if (!updateCenterResponse.Success) return;
                            updateCenterJson = updateCenterResponse.Data.Substring(19).TrimEnd(new[] { ')', ';' });
                        }
                        else
                        {
                            updateCenterJson = await GetUpdateCenterFromFile(o.File);
                        }

                        var finalList = new List<string>();
                        try
                        {
                            var pluginListObject = await DeserializePluginList(updateCenterJson);
                            finalList = await GetPluginsList(o.Plugins, pluginListObject);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                        finalList.AddRange(o.Plugins);
                        await WriteOutPlugins(finalList.Distinct().ToArray());
                    }
                });
        }

        public static async ValueTask<List<string>> GetPluginsList(IEnumerable<string> plugins, JenkinsPlugins listOfPlugins)
        {
            var finalList = new List<string>();
            var tempList = new List<string>();
            foreach (var x in plugins)
            {
                listOfPlugins.Plugins.TryGetValue(x, out var p);
                if(p is null) continue;

                var listdeps = p.Dependencies;

                var requireDependencies = (from l in listdeps where l.Optional == false select l.Name).ToList();

                finalList.AddRange(requireDependencies);
                tempList.AddRange(requireDependencies);
            }

            for (var i = 0; i < tempList.Count; i++)
            {
                listOfPlugins.Plugins.TryGetValue(tempList.ToArray()[i], out var p);
                
                if (p is null) continue;

                var required = (from l in p.Dependencies where l.Optional == false select l.Name);
                foreach (var d in p.Dependencies)
                {
                    finalList.Add(d.Name);
                    tempList.Add(d.Name);
                }
            }

            finalList = finalList.OrderBy(q => q).ToList();
            return finalList;
        }

        public static async ValueTask<HttpCallResponse<string>> GetUpdateCenter(string updateCenter)
        {
            var result = await Http.Request(updateCenter)
                .ExpectString()
                .GetAsync();
            return result;
        }

        public static async ValueTask<JenkinsPlugins> DeserializePluginList(string data)
        {
            var plugins = JsonConvert.DeserializeObject<JenkinsPlugins>(data);
            return plugins;
        }

        public static async ValueTask<JsonElement> GetPluginListAsJson(string data, IEnumerable<string> inputPlugins)
        {
            JsonDocument.Parse(data).RootElement.TryGetProperty("plugins", out var plugins);
            return plugins;
        }

        public static async ValueTask WriteOutPlugins(string[] processed)
        {
            using (var outputFile = new StreamWriter(File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "plugins.txt"))))
            {
                foreach (var s in processed)
                    await outputFile.WriteLineAsync(s);
            }
        }

        public static async ValueTask<string> GetUpdateCenterFromFile(string file)
        {
            using (var inputFile = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), file)))
            {
                return await inputFile.ReadToEndAsync();
            }
        }
    }
}