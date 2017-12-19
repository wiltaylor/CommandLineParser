using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLineParser;

namespace ExampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new CommandParser(new[] { new VersionCommand()});

            foreach(var line in handler.Process(new[] {"version", "--long", "--notdefined"}))
                Console.WriteLine(line);

            Console.Read();
        }
    }
}
