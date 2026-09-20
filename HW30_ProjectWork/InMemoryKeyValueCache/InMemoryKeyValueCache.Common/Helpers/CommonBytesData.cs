using System.Text;

namespace InMemoryKeyValueCache.Common.Helpers
{
    public static class CommonBytesData
    {
        public static byte SplitValue = (byte)' ';
        public static byte[] Ok = Encoding.UTF8.GetBytes("OK\r\n");
        public static byte[] Error = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
        public static byte[] Nil = Encoding.UTF8.GetBytes("(nil)\r\n");
    }
}
