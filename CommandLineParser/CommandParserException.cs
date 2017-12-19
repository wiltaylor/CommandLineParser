using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    public class CommandParserException : Exception
    {
        public CommandParserException(string message) : base(message)
        {
            
        }
    }
}
