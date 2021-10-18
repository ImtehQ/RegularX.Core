using RegularX.Core.Expressions;
using Xunit;

namespace RegularX.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string input = "Test if this works";
            string pattern = "!s!t";

            string result = input.Exp(pattern);

            Assert.Equal("e if hi work", result);
        }
    }
}
