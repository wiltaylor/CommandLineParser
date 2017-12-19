using System.Collections.Generic;
using System.Linq;

namespace CommandLineParser
{
    /// <summary>
    /// Base class to define all Command handlers from.
    /// </summary>
    public abstract class CommandHandlerBase : ICommandHandler
    {
        /// <summary>
        /// Primary name to identify handler.
        /// Use this to determine which sub commands belong to this command.
        /// This should be a unique value.
        /// </summary>
        public abstract string PrimaryName { get; }

        /// <summary>
        /// Names command can be called by on the commandline.
        /// </summary>
        public abstract IEnumerable<string> Names { get; }

        /// <summary>
        /// Name of parent command. See PrimaryName for how to set parent of a command.
        /// </summary>
        public virtual string ParentName => "";

        /// <summary>
        /// An list of supported switches of this command.
        /// </summary>
        public virtual IEnumerable<SwitchInfo> Switches => new SwitchInfo[] { };

        /// <summary>
        /// Usage text help message for this command. Will show up when user types -? at the command line."
        /// </summary>
        public abstract string UsageText { get; }

        /// <summary>
        /// The priority the command will show up in the help text. The lower the value the higher it will show.
        /// </summary>
        public int UsagePriority => 100;

        /// <summary>
        /// Command Parser object which called this class. Use this to run sub commands.
        /// </summary>
        protected ICommandParser Parser;

        //Private variables.
        private readonly List<string> _returnText = new List<string>();
        private Dictionary<string, List<string>> _switchData = new Dictionary<string, List<string>>();

        /// <summary>
        /// This method is called by the command parser when running the command. You won't need to call this directly.
        /// </summary>
        /// <param name="parser">Parser instance that called this command.</param>
        /// <param name="args">Command arguments passed in from the command line.</param>
        /// <returns></returns>
        public IEnumerable<string> Process(ICommandParser parser, string[] args)
        {
            Parser = parser;
            ProcessCommand(args);

            foreach (var sd in _switchData)
            {
                var found = false;
                foreach (var sw in Switches)
                {
                    if (sw.Names.Contains(sd.Key.ToLower()))
                        found = true;

                    if (sw.ShortNames.Contains(sd.Key))
                        found = true;
                }

                if(found)
                    continue;

                throw new CommandParserException($"The switch {sd.Key} is not a valid switch for this command!");
            }

            return _returnText;
        }

        /// <summary>
        /// Used by command parser to add values to current switches passed in.
        /// You won't need to use this directly except in unit tests.
        /// </summary>
        /// <param name="name">Name of switch</param>
        /// <param name="value">Value to append to it.</param>
        public void AppendSwitch(string name, string value)
        {
            if (_switchData.ContainsKey(name))
                _switchData[name].Add(value);
            else
            {
                _switchData.Add(name, new List<string>());
                _switchData[name].Add(value);
            }
        }

        /// <summary>
        /// Used by command parser to set a switch.
        /// You won't need to use this directly except in unit tests.
        /// </summary>
        /// <param name="name">Name of switch to set</param>
        public void SetSwitch(string name)
        {
            if(!_switchData.ContainsKey(name))
                _switchData.Add(name, new List<string>());
        }

        /// <summary>
        /// Checks if a switch is set or not.
        /// </summary>
        /// <param name="name">Name of switch to check</param>
        /// <returns></returns>
        public bool IsSwitchSet(string name)
        {
            var key = Switches.FirstOrDefault(s => (s.Names.Contains(name.ToLower()) || name == "?") || s.ShortNames.Contains(name));

            if (key == null)
                return false;

            var data = _switchData.FirstOrDefault(d =>
                key.Names.Contains(d.Key.ToLower()) || key.ShortNames.Contains(d.Key));

            return !string.IsNullOrEmpty(data.Key);
        }

        /// <summary>
        /// Returns the values assigned to a switch from the command line.
        /// </summary>
        /// <param name="name">Name of switch</param>
        /// <returns>Will return an IEnumerable of strings or null if the switch doesn't exist.</returns>
        public IEnumerable<string> GetSwitch(string name)
        {
            var key = Switches.FirstOrDefault(s => s.Names.Contains(name.ToLower()) || s.ShortNames.Contains(name));

            if (key == null)
                return null;

            var data = _switchData.FirstOrDefault(d =>
                key.Names.Contains(d.Key.ToLower()) || key.ShortNames.Contains(d.Key));

            return string.IsNullOrEmpty(data.Key) ? null : data.Value;
        }

        /// <summary>
        /// Writes text to the return strings. Use this to write messages to the console.
        /// </summary>
        /// <param name="message">Message to write.</param>
        protected void WriteText(string message)
        {
            _returnText.Add(message);
        }

        /// <summary>
        /// Override this method to handle what happens when your command is called.
        /// </summary>
        /// <param name="args">Command line arguments passed to the command with switches stripped out.</param>
        public abstract void ProcessCommand(string[] args);

        /// <summary>
        /// Overide this method to handle usage manually.
        /// </summary>
        public virtual IEnumerable<string> Usage()
        {
            var result = new List<string>
            {
                $"Usage for {PrimaryName} :",
                UsageText, "", "Supported Switches: "
            };

            result.AddRange(from s in Switches.OrderBy(sw => sw.UsagePriority)
                            let names = string.Join(",", s.Names) + "," + string.Join(",", s.ShortNames)
                            select $"\t{names}\t - {s.UsageText}");
            return result;
        }
    }
}
