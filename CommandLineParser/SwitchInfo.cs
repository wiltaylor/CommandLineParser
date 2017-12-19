using System.Collections.Generic;

namespace CommandLineParser
{
    public class SwitchInfo
    {
        /// <summary>
        /// Long form names for this switch (i.e. --myswitch).
        /// These need to all be lower case.
        /// </summary>
        public IEnumerable<string> Names { get; set; }

        /// <summary>
        /// Short form names for this switch (i.e. -e).
        /// These are case sensetive so -E and -e are different switches.
        /// </summary>
        public IEnumerable<string> ShortNames { get; set; }

        /// <summary>
        /// Is this switch required for a command and it is not passed by the user and error is returned.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// This is the number of parameters that the switch will pull up from the command line.
        /// </summary>
        public int ArgumentCount { get; set; } = 0;

        /// <summary>
        /// Help text printed to the console when usage is displayed.
        /// </summary>
        public string UsageText { get; set; }

        /// <summary>
        /// Priority order of where this switch appears in the usage text. Lower the number the higher up the page it appears.
        /// </summary>
        public int UsagePriority { get; set; } = 100;
    }
}
