using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HW03_SpanMemoryStackalloc.Models;

namespace HW03_SpanMemoryStackalloc
{
    public static class CommandParser
    {
        private static byte _splitValue = (byte)' ';

        public static DataStruct<byte> Parse(ReadOnlySpan<byte> roSpan)
        {
            ReadOnlySpan<byte> command = null;
            ReadOnlySpan<byte> key = null;
            ReadOnlySpan<byte> value = null;

            for (int i = 0; i < 3; i++)
            {
                var index = SkipWhiteSpace(roSpan, 0);
                if (index.isEnd)
                    break;

                roSpan = roSpan.Slice(index.idx);

                index.idx = roSpan.IndexOf(_splitValue);

                var part = index.idx != -1 ? roSpan.Slice(0, index.idx) : roSpan;
                switch (i)
                {
                    case 0:
                        command = part;
                        break;
                    case 1:
                        key = part;
                        break;
                    case 2:
                        value = part;
                        break;
                }

                index = SkipWhiteSpace(roSpan, index.idx + 1);
                if (index.isEnd)
                    break;

                roSpan = roSpan.Slice(index.idx);
            }

            return key.IsEmpty ?
                new DataStruct<byte>() :
                new DataStruct<byte>()
                {
                    Command = command,
                    Key = key,
                    Value = value
                };
        }

        private static (int idx, bool isEnd) SkipWhiteSpace(ReadOnlySpan<byte> roSpan, int idx)
        {
            while (true)
            {
                if (idx >= roSpan.Length)
                    return (idx, true);

                if (roSpan[idx] == _splitValue)
                    idx++;
                else
                    break;
            }
            return (idx, false);
        }
    }
}
