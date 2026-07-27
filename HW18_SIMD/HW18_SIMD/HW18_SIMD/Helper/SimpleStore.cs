using System.Text.Json;

namespace HW18_SIMD.Helper
{
    public class SimpleStore : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, byte[]> _dictionary = new Dictionary<string, byte[]>();

        private bool _disposed = false;
        private long _setCount = 0;
        private long _getCount = 0;
        private long _deleteCount = 0;

        public void Set(string key, UserProfile? profile)
        {
            try
            {
                CheckParamKey(key);
                CheckParamValue(profile);

                _lock.EnterWriteLock();

                _dictionary[key] = JsonSerializer.SerializeToUtf8Bytes(profile);

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
        public UserProfile? Get(string key)
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
                return JsonSerializer.Deserialize<UserProfile>(_dictionary[key]);
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
        private void CheckParamValue(UserProfile? profile)
        {
            if (profile == null)
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
