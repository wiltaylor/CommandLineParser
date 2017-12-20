using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLineParser
{
    public class CommandParser : ICommandParser
    {

        private readonly IEnumerable<ICommandHandler> _commandHandlers;

        public CommandParser(IEnumerable<ICommandHandler> commandHandlers)
        {
            _commandHandlers = commandHandlers;
        }


        public IEnumerable<string> Process(string[] args)
        {
            return ProcessSubCommand("", args);
        }

        public IEnumerable<string> ProcessSubCommand(string command, string[] args)
        {
            if (args.Length == 0 || args[0] == "-?")
                return Usage(command);

            if (args[0].StartsWith("-"))
                return new [] { "Unexpected - when command was expected. Please start with command." };
            
            var handler = _commandHandlers.FirstOrDefault(h => h.ParentName == command && h.Names.Contains(args[0])) ??
                          _commandHandlers.FirstOrDefault(h => h.ParentName == command && h.Names.Contains("default")); //If default handler is defined call it instead.

            if (handler == null)
                return Usage(command);

            try
            {
                var splitArgs = ExtractSwitches(handler, handler.Names.Contains("default") ? args : args.Skip(1)); //If using default handler pass the command in as first argument.

                if (handler.IsSwitchSet("?"))
                {
                    return handler.Usage();
                }

                var problemSwitches = handler.Switches.Where(s => s.Required && !handler.IsSwitchSet(s.Names.First())).ToArray();
                return problemSwitches.Length > 0 ? 
                    new []
                    {
                        "You need to use the following switches: " + 
                        string.Join(",", problemSwitches.Select(s => s.Names.First())) 
                        
                    } : 
                    handler.Process(this, splitArgs);
            }
            catch
            {
                return new[] {"Invalid switches passed in. Please use -? to list available switches."};
            }

        }

        public IEnumerable<string> GetSubCommands(string parrentCommand)
        {
            return _commandHandlers.Where(h => string.Equals(h.ParentName, parrentCommand, StringComparison.CurrentCultureIgnoreCase))
                .Select(h => h.PrimaryName);
        }

        private string[] ExtractSwitches(ICommandHandler handler, IEnumerable<string> args)
        {
            if (!handler.ProcessSwitches)
                return args.ToArray();

            var result = new List<string>();

            var skipArgs = 0;
            var lastSwitchName = default(string);

            foreach (var item in args)
            {
                if (skipArgs > 0)
                {
                    handler.AppendSwitch(lastSwitchName, item);
                    skipArgs--;
                    continue;
                }

                //Handle help switch.
                if (item.StartsWith("-?"))
                {
                    handler.SetSwitch("?");
                    return new string[] { };
                }


                if (item.StartsWith("--"))
                {
                    lastSwitchName = item.Substring(2).ToLower();
                    handler.SetSwitch(lastSwitchName);                  

                    var currentSwitch = handler.Switches.First(s => s.Names.Contains(lastSwitchName));
                    skipArgs = currentSwitch.ArgumentCount;
                }
                else if (item.StartsWith("-"))
                {
                    lastSwitchName = item.Substring(1);
                    handler.SetSwitch(lastSwitchName);

                    var currentSwitch = handler.Switches.First(s => s.ShortNames.Contains(lastSwitchName));
                    skipArgs = currentSwitch.ArgumentCount;
                }
                else
                {
                    result.Add(item);
                }
            }

            return result.ToArray();
        }

        private IEnumerable<string> Usage(string command)
        {
            var result = new List<string>{"Usage: "};

            result.AddRange(_commandHandlers
                .Where(c => c.ParentName == command)
                .OrderBy(c => c.UsagePriority)
                .Select(c => c.UsageText));

            return result;
        }
    }
}
