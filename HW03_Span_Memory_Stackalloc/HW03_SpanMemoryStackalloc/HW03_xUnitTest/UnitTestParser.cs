using HW03_SpanMemoryStackalloc;
using HW03_SpanMemoryStackalloc.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Text;


namespace HW03_xUnitTest
{
    public class UnitTestParser
    {
        [Fact]
        public void TestParser3Args()
        {
            var expected = GetExpected("SET", "User:1", "True");
            var parsed = CommandParser.Parse(Encoding.UTF8.GetBytes("    SET   User:1    True").AsSpan());

            Assert.Equal(parsed.Command, expected.Command);
            Assert.Equal(parsed.Key, expected.Key);
            Assert.Equal(parsed.Value, expected.Value);
        }

        [Fact]
        public void TestParser2Args()
        {
            var expected = GetExpected("SET", "User:1", null);
            var parsed = CommandParser.Parse(Encoding.UTF8.GetBytes("SET   User:1 ").AsSpan());

            Assert.Equal(parsed.Command, expected.Command);
            Assert.Equal(parsed.Key, expected.Key);
            Assert.Equal(parsed.Value, expected.Value);
        }

        [Fact]
        public void TestParserWithoutKey()
        {
            var expected = GetExpected(null, null, null);
            var parsed = CommandParser.Parse(Encoding.UTF8.GetBytes("  SET   ").AsSpan());

            Assert.Equal(parsed.Command, expected.Command);
            Assert.Equal(parsed.Key, expected.Key);
            Assert.Equal(parsed.Value, expected.Value);
        }

        private DataStruct<byte> GetExpected(string command, string key, string value)
        {
            return new DataStruct<byte>()
            {
                Command = !string.IsNullOrEmpty(command) ?
                    (ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(command).AsSpan() :
                    (ReadOnlySpan<byte>)new byte[0],
                Key = !string.IsNullOrEmpty(key) ?
                    (ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(key).AsSpan() :
                    (ReadOnlySpan<byte>)new byte[0],
                Value = !string.IsNullOrEmpty(value) ? 
                    (ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(value).AsSpan() :
                    (ReadOnlySpan<byte>)new byte[0]
            };
        }
    }
}
