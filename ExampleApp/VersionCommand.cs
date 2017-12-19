using System.Collections.Generic;
using CommandLineParser;

namespace ExampleApp
{
    public class VersionCommand : CommandHandlerBase
    {
        public override string PrimaryName => "version";
        public override IEnumerable<string> Names => new[] { "version" };
        public override string UsageText => "Prints the version of the application.";
        public override void ProcessCommand(string[] args)
        {
            WriteText("Version: 1.0.0.0");

            if(IsSwitchSet("long"))
                WriteText("wooo");
        }

        public override IEnumerable<SwitchInfo> Switches => new[]
        {
            new SwitchInfo
            {
                Names = new [] {"long"},
                ShortNames = new [] {"l"},
                ArgumentCount = 0,
                Required = false
            } 
        };
    }
}
