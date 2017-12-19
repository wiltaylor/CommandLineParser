# Command Line Parser
This is a simple command line parser I created for a bunch of command line tools I am working on.

The reason I created this rather than using an existing one is I had the following requirements:

* I needed to support commands and sub commands.
* I wanted to use Dependency Injection and have the ability to create more handlers easily.

# Installing
To install the package you can search for it in the NuGet Package Manager or install from the package console with: 

```
Install-Package WilTaylor.CommandLineParser
```

# Usage:
First create a class to hold your command handler and inherit from CommandHandlerBase then implement all the abstract methods.

```
    public class FooHandler : CommandHandlerBase
    {
        public override void ProcessCommand(string[] args)
        {
            //Put code you want to run when command is run here.

            var message = "foobar";

            //You can check if switches are set or not with IsSwitchSet.
            if (IsSwitchSet("Test"))
                message = "Hello " + message;

            //Use this to write text to the console instead of Console objects.
            //This is to handle different hosts that don't write to console windows if you want.
            WriteText(message);
        }

        public override string PrimaryName => "foo"; //Primary name this command will be referenced by
        public override IEnumerable<string> Names => new[] { "foo"}; //Names that can be used to get this command if you want multiple. You must put the primary command in here though.

        public override string UsageText => "Writes foobar to the console"; //Will be shown when user uses -?

        public override IEnumerable<SwitchInfo> Switches => new[] //Here you can define all of the valid switches for a command.
        {
            new SwitchInfo
            {
                Names = new []{ "test"}, //Names passed with --test (not case senstive but you must put it lowercase here)
                ShortNames = new [] {"t"}, //Short names are passed with -t (case sensetive)
                UsageText = "Appends test to message", //Usage text that is shown when -? is passed.
                ArgumentCount = 0 // How many arguments after the switch will it capture.
            }
        };
    }
```

Now you can call it with the following:

```
static void Main(string[] args)
{
    //This is normally handled by a CI Container which will inject all handlers you create.
    var commandline = new CommandLineParser.CommandParser(new[] {new FooHandler()});

    //Execute the command with results from the command line by passing them to process.
    var result = commandline.Process(args);

    //It will return a IEnumerable of strings you can write to the console or log file etc.
    foreach (var line in result)
        Console.WriteLine(line);

}
```

