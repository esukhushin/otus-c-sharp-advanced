using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace HW09_Channels
{
    public class SimpleStore : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, byte[]> _dictionary = new Dictionary<string, byte[]>();

        private bool _disposed = false;
        private long _setCount = 0;
        private long _getCount = 0;
        private long _deleteCount = 0;

        public SimpleStore()
        {

        }
        ~SimpleStore()
        {
            Dispose(false);
        }

        public void Set(string key, byte[] value)
        {
            try
            {
                CheckParamKey(key);
                CheckParamValue(value);

                _lock.EnterWriteLock();

                _dictionary[key] = value;

                Interlocked.Increment(ref _setCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"SetError - '{ex.Message}'");
            }
            finally
            {
                _lock.ExitWriteLock();
            }


        }
        public byte[]? Get(string key)
        {
            try
            {
                CheckParamKey(key);

                _lock.EnterReadLock();

                if (!_dictionary.Keys.Contains(key))
                {
                    Interlocked.Increment(ref _getCount);
                    return null;
                }
                
                Interlocked.Increment(ref _getCount);
                return _dictionary[key];
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"GetError - '{ex.Message}'");
                return null;
            }
            finally
            {
                _lock.ExitReadLock();
            }

        }
        public void Delete(string key)
        {
            try
            {
                CheckParamKey(key);

                _lock.EnterWriteLock();

                if (_dictionary.Keys.Contains(key))
                {
                    _dictionary.Remove(key);
                    Interlocked.Increment(ref _deleteCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"DeleteError - '{ex.Message}'");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public (long SetCount, long GetCount, long DeleteCount) GetStatistics()
        {
            return (Interlocked.Read(ref _setCount), Interlocked.Read(ref _getCount), Interlocked.Read(ref _deleteCount));
        }
        
        private void CheckParamKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Param 'key' is null");
        }
        private void CheckParamValue(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("Param 'value' is null");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _lock.Dispose();
            }

            _disposed = true;
        }
    }
}