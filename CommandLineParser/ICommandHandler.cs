using System.Collections.Generic;

namespace CommandLineParser
{
    public interface ICommandHandler
    {
        string PrimaryName { get; }
        IEnumerable<string> Names { get; }
        string ParentName { get; }
        IEnumerable<SwitchInfo> Switches { get; }

        /// <summary>
        /// Help text printed to the console when usage is displayed.
        /// </summary>
        string UsageText { get; }

        /// <summary>
        /// Priority order of where this switch appears in the usage text. Lower the number the higher up the page it appears.
        /// </summary>
        int UsagePriority { get; }

        IEnumerable<string> Process(ICommandParser parser, string[] args);

        void AppendSwitch(string name, string value);
        void SetSwitch(string name);
        bool IsSwitchSet(string name);
        IEnumerable<string> GetSwitch(string name);

        IEnumerable<string> Usage();

    }
}
