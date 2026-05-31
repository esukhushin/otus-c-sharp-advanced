using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace HW03_SpanMemoryStackalloc
{
    public class SimpleStore
    {
        private readonly Dictionary<string, byte[]> _dictionary;

        public SimpleStore() 
        {
            _dictionary = new Dictionary<string, byte[]>();
        }

        public void Set(string key, byte[] value)
        {
            _dictionary[key] = value;
        }

        public byte[]? Get(string key)
        {
            if (!_dictionary.Keys.Contains(key))
                return null;

            return _dictionary[key];
        }

        public void Delete(string key)
        {
            if(_dictionary.Keys.Contains(key))
                _dictionary.Remove(key);
        }
    }
}
