using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using JenkinsDependencyResolver;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Program = JenkinsDependencyResolver.Program;

namespace UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public async Task GetValidURL()
        {
            var x = await Program.GetUpdateCenter
                ("https://updates.jenkins.io/current/update-center.json");
            
            Assert.AreEqual(x.Success,true);
        }
        
        [Test]
        public async Task GetInvalidURL()
        {
            var x = await Program.GetUpdateCenter("https://google.com/update-center.json");
            Assert.AreEqual(x.Success,false);
        }
        
        [Test]
        public async Task CheckDependencies()
        {
            var pluginList = new List<string>()
            {
                "kubernetes", "workflow-api", "workflow-cps", "workflow-step-api", "authentication-tokens", "cloudbees-folder", "credentials", "durable-task", "jackson2-api", "kubernetes-client-api", "metrics", "plain-credentials", "structs", "variant", "kubernetes-credentials", "pipeline-model-extensions"
            };
            
            var x = await Program.GetUpdateCenter
                ("https://updates.jenkins.io/current/update-center.json");
            var process = await Program.GetPluginList(x.Data, new List<string>(){"kubernetes"});
            
            var finalList = await Program.ListPlugins(new[] { "kubernetes" }, process);
            
            Assert.AreEqual(pluginList, finalList);

        }
        
        [Test]
        public async Task ListOfPlugins_Parsed()
        {
            var sut = new Parser();
            var options = new Options()
            {
                Plugins = new []{"kubernetes","openshift-client"}
            };
            
            var parsed = sut.ParseArguments<Options>(new[]{"-p","kubernetes","openshift-client"});
            var result = ((Parsed<Options>) parsed).Value;
            Assert.AreEqual(options.Plugins, result.Plugins);
        }
    }
}