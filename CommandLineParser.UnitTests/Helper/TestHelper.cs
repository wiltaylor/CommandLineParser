using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CommandLineParser.UnitTests.Helper
{
    public static class TestHelper
    {
        public static IFixture NewFixture()
        {
            return new Fixture()
                .Customize(new AutoFakeItEasyCustomization());
        }
    }
}
