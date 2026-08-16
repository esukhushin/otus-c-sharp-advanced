using HW21_Roslyn_TcpServer.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn_TcpServer_BenchmarkDotNet.Interface
{
    public interface ITcpServerClient
    {
        public Task<byte[]> SetAsync(string key, UserProfile value);
        public Task<UserProfile?> GetAsync(string key);
    }
}
