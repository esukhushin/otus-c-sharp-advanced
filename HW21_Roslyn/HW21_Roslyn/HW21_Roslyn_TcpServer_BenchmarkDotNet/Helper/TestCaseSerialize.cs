using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using HW21_Roslyn_TcpServer.Helper;
using System.Text.Json;


namespace HW21_Roslyn_TcpServer_BenchmarkDotNet.Helper
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class TestCaseSerialize
    {
        private UserProfile _userProfile;

        [GlobalSetup]
        public void Setup()
        {
            _userProfile = new UserProfile()
            {
                Id = 1,
                Username = $@"Test{Guid.NewGuid()}",
                CreatedAt = DateTime.Now
            };
        }

        [Benchmark]
        public void SerializeRoslyn()
        {
            using (var stream = new MemoryStream())
            {
                _userProfile.SerializeToBinary(stream);
                var result = stream.ToArray();
            }
        }

        [Benchmark]
        public void SerializeJson()
        {
            var result = JsonSerializer.SerializeToUtf8Bytes(_userProfile);
        }

        public static void Execute()
        {
            BenchmarkRunner.Run<TestCaseSerialize>();
        }
    }
}
