using HW24_OpenTelemetry.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW24_OpenTelemetry_NBomber.Interface
{
    public interface ITcpServerClient
    {
        public Task<byte[]> SetAsync(string key, UserProfile value);
        public Task<UserProfile?> GetAsync(string key);
    }
}
