using System;
using System.Collections.Generic;
using System.Text;
using HW21_Roslyn_TcpServer.Helper;

namespace HW21_Roslyn_TcpServer_NBomber.Interface
{
    public interface ITcpServerClient
    {
        public Task<byte[]> SetAsync(string key, UserProfile value);
        public Task<UserProfile?> GetAsync(string key);
    }
}
