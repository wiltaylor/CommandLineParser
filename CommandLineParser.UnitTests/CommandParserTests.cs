using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using CommandLineParser.UnitTests.Helper;
using FakeItEasy;
using Ploeh.AutoFixture;
using Shouldly;
using Xunit;

namespace CommandLineParser.UnitTests
{
    public class CommandParserTests
    {
        [Fact]
        public void When_CallingCommandParserWithNonExistingCommand_Should_ReturnUsage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new []{"notanyvalidcommand"});

            //Assert
            result.Any(l => l.Contains("Usage")).ShouldBeTrue();
        }

        [Fact]
        public void When_CallingCommandParserWithoutAnyArguments_Should_ReturnUsage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new string[] { });

            //Assert
            result.Any(l => l.Contains("Usage")).ShouldBeTrue();
        }

        [Fact]
        public void When_CallingCommandWithSwitchAtStart_Should_ReturnErrorMessage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new[] {"-switchatthestart"});

            //Assert
            result.Any(l => l.Contains("Unexpected - when command was expected")).ShouldBeTrue();
        }

        [Fact]
        public void When_CallingAValidCommand_Should_CallItsHandler()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            var sut = fixture.Create<CommandParser>();

            //Act
            sut.Process(new string[] {$"{commandHandler.Names.First()}"});

            //Assert
            A.CallTo(() => commandHandler.Process(sut, A<string[]>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void When_CallingAValidCommandWithoutRequiredSwitch_Should_ReturnErrorMessage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.Switches).Returns(fixture.CreateMany<SwitchInfo>());
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new string[] { $"{commandHandler.Names.First()}" });

            //Assert
            result.Any(l => l.Contains("You need to use the following switches")).ShouldBeTrue();
        }

        [Fact]
        public void When_PassingInSwitchThatHasntBeenDefinedOnCommand_Should_ReturnErrorMessage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.Switches).Returns(fixture.CreateMany<SwitchInfo>());
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new string[] { $"{commandHandler.Names.First()}", "--badswitch" });

            //Assert
            result.Any(l => l.Contains("Invalid switches passed in. Please use -? to list available switches.")).ShouldBeTrue();
        }

        [Fact]
        public void When_PassingInCommandWithValidLongFormSwitch_Should_NotReturnError()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.Switches).Returns(new [] { new SwitchInfo{ Names = fixture.CreateMany<string>(), ArgumentCount = 0} });
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new [] { $"{commandHandler.Names.First()}", $"--{commandHandler.Switches.First().Names.First()}" });

            //Assert
           result.Count().ShouldBe(0);
        }

        [Fact]
        public void When_PassingInCommandWithShortFormSwitch_Should_NotReturnError()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.Switches).Returns(new[] { new SwitchInfo { ShortNames = new[]{"a", "b", "c"}, ArgumentCount = 0 } });
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new[] { $"{commandHandler.Names.First()}", $"-{commandHandler.Switches.First().ShortNames.First()}" });

            //Assert
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void When_PassingArgumentsWhichHaveParameters_Should_StoreParameters()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.Switches).Returns(new[] { new SwitchInfo { Names = fixture.CreateMany<string>(), ArgumentCount = 2 } });
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new[] { $"{commandHandler.Names.First()}", $"--{commandHandler.Switches.First().Names.First()}", "para1", "para2" });

            //Assert
            A.CallTo(() => commandHandler.AppendSwitch(commandHandler.Switches.First().Names.First(), "para1")).MustHaveHappened();
            A.CallTo(() => commandHandler.AppendSwitch(commandHandler.Switches.First().Names.First(), "para2")).MustHaveHappened();
        }

        [Fact]
        public void When_PassingInArgumentsToACommand_Should_BePassedToTheCommand()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            var sut = fixture.Create<CommandParser>();

            //Act
            sut.Process(new[] { $"{commandHandler.Names.First()}", "para1", "para2" });

            //Assert
            A.CallTo(() => commandHandler.Process(sut, A<string[]>.Ignored))
                .WhenArgumentsMatch(args => ((string[])args[1])[0] == "para1" && ((string[])args[1])[1] == "para2")
                .MustHaveHappened();                
        }

        [Fact]
        public void When_PassingHelpSwitch_Should_PrintUsage()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.Process(new[] {"-?"});

            //Assert
            result.Any(l => l.Contains("Usage")).ShouldBeTrue();
        }

        [Fact]
        public void When_PassingHelpSwitchToCommand_Should_CallUsageOnCommand()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.Names).Returns(fixture.CreateMany<string>());
            A.CallTo(() => commandHandler.IsSwitchSet(A<string>.Ignored)).Returns(true);
            var sut = fixture.Create<CommandParser>();

            //Act
            sut.Process(new[] { $"{commandHandler.Names.First()}", "-?" });

            //Assert
            A.CallTo(() => commandHandler.Usage()).MustHaveHappened();
        }

        [Fact]
        public void When_CallingGetSubCommands_Should_ReturnAListOfRegisteredSubCommands()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var commandHandler = fixture.Freeze<ICommandHandler>();
            A.CallTo(() => commandHandler.PrimaryName).Returns(fixture.Create<string>());
            A.CallTo(() => commandHandler.ParentName).Returns(fixture.Create<string>());
            var sut = fixture.Create<CommandParser>();

            //Act
            var result = sut.GetSubCommands(commandHandler.ParentName);

            //Assert
            result.ShouldContain(commandHandler.PrimaryName);

        }
    }
}
