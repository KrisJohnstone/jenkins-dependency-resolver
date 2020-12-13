using System.Collections.Generic;
using CommandLine;

namespace JenkinsDependencyResolver.Models
{
    public class Options
    {
        [Option('p', "plugins", Required = true, HelpText = "Pass list of plugins")]
        public IEnumerable<string> Plugins { get; set; }
            
        [Option('u', "update-center", Required = false, HelpText = "URL to Update Center JSON")]
        public string UpdateCenterURL { get; set; }
        
        [Option('r', "remote-location", Required = false, HelpText = "URL location for ")]
        public string PluginLocation { get; set; }

        [Option('f', "filename", Required = false, HelpText = "Sometimes jenkins puts out a malformed json document...")]
        public string File { get; set; }
    }
}