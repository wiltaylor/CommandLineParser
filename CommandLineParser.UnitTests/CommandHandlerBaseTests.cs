using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLineParser.UnitTests.Helper;
using Xunit;
using FakeItEasy;
using Ploeh.AutoFixture;
using Shouldly;

namespace CommandLineParser.UnitTests
{

    public class FakeCommandHandler : CommandHandlerBase
    {
        private readonly Action<string[]> _processCommand;
        public FakeCommandHandler(string primaryName, IEnumerable<string> names, string usageText, IEnumerable<SwitchInfo> switches)
        {
            PrimaryName = primaryName;
            Names = names;
            UsageText = usageText;
            Switches = switches;
        }

        public void WriteTextFromTest(string text)
        {
            WriteText(text);
        }

        public override IEnumerable<SwitchInfo> Switches { get; }

        public override string PrimaryName { get; }
        public override IEnumerable<string> Names { get; }
        public override string UsageText { get; }
        public override void ProcessCommand(string[] args)
        {

        }
    }

    public class CommandHandlerBaseTests
    {
        [Fact]
        public void When_PassingInSwitchesThatCommandDoesntHandle_Should_Throw()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sut = fixture.Create<FakeCommandHandler>();
            sut.SetSwitch("NonExisting");

            //Act & Assert
            Assert.Throws<CommandParserException>(() =>
            {
                sut.Process(parser, new string[] { });
            });
        }

        [Fact]
        public void When_PassingInNoSwitches_Should_NotThrow()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sut = fixture.Create<FakeCommandHandler>();

            //Act & Assert
            sut.Process(parser, new string[]{});
            //No Exception Raised
        }

        [Fact]
        public void When_PassingInSupportedLongNameSwitch_Should_BeFound()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sw = fixture.Freeze<SwitchInfo>();
            sw.Names = fixture.CreateMany<string>();
            var sut = fixture.Create<FakeCommandHandler>();
            sut.SetSwitch(sw.Names.First());

            //Act & Assert
            sut.Process(parser, new string[] { });
            //No Exception raised.

        }

        [Fact]
        public void When_PassingInSupportedShortNameSwitch_Should_BeFound()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sw = fixture.Freeze<SwitchInfo>();
            sw.ShortNames = fixture.CreateMany<string>();
            var sut = fixture.Create<FakeCommandHandler>();
            sut.SetSwitch(sw.ShortNames.First());

            //Act & Assert
            sut.Process(parser, new string[] { });
            //No Exception raised.
        }

        [Fact]
        public void When_CallingAppendSwitch_Should_StoreDataInternally()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sw = fixture.Freeze<SwitchInfo>();
            sw.Names = fixture.CreateMany<string>();
            var sut = fixture.Create<FakeCommandHandler>();
            var value = fixture.Create<string>();

            //Act
            sut.AppendSwitch(sw.Names.First(), value);

            //Assert
            var result = sut.GetSwitch(sw.Names.First()).ToArray();
            result.Length.ShouldBe(1);
            result[0].ShouldBe(value);
        }

        [Fact]
        public void When_CallingAppendSwitchOnExistingSwitch_Should_StoreDataInternally()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sw = fixture.Freeze<SwitchInfo>();
            sw.Names = fixture.CreateMany<string>();
            var sut = fixture.Create<FakeCommandHandler>();
            var value = fixture.Create<string>();

            //Act
            sut.SetSwitch(sw.Names.First());
            sut.AppendSwitch(sw.Names.First(), value);

            //Assert
            var result = sut.GetSwitch(sw.Names.First()).ToArray();
            result.Length.ShouldBe(1);
            result[0].ShouldBe(value);
        }

        [Fact]
        public void When_CallingWriteText_Should_BeInReturnStrings()
        {
            //Arrange
            var fixture = TestHelper.NewFixture();
            var parser = fixture.Freeze<ICommandParser>();
            var sut = fixture.Create<FakeCommandHandler>();
            var value = fixture.Create<string>();

            //Act
            sut.WriteTextFromTest(value);

            //Assert
            var result = sut.Process(parser, new string[] { });
            result.Any(l => l.Contains(value)).ShouldBeTrue();
        }
    }
}
