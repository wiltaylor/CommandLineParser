using System.Collections.Generic;

namespace CommandLineParser
{
    public interface ICommandParser
    {
        /// <summary>
        /// Process the input from the commandline.
        /// </summary>
        /// <param name="args">Parameters passed into the main function.</param>
        /// <returns></returns>
        IEnumerable<string> Process(string[] args);

        /// <summary>
        /// Process a sub command. Call this to call sub commands.
        /// </summary>
        /// <param name="command">Name of sub command to run.</param>
        /// <param name="args">Arguments to pass into sub command.</param>
        /// <returns></returns>
        IEnumerable<string> ProcessSubCommand(string command, string[] args);

        /// <summary>
        /// Gets a list of supported sub commands for a command. This is useful for when working with Dependancy Injected sub commands.
        /// </summary>
        /// <param name="parrentCommand">Name of parent command.</param>
        /// <returns>List of supported sub commands</returns>
        IEnumerable<string> GetSubCommands(string parrentCommand);
    }
}
