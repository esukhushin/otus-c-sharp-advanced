using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using HW21_Rolsyn_BenchmarkDotNet.Models;
using System.Buffers;
using System.Text.Json;

namespace HW21_Rolsyn_BenchmarkDotNet.Helper
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class TestCaseSerialize
    {
        private readonly Random _rnd = new Random(10);
        private UserProfile _userProfile;
        private MemoryStream _stream;
        private byte[]? _arrayByte;
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        [IterationSetup]
        public void Setup()
        {
            _userProfile = new UserProfile()
            {
                Id = _rnd.Next(),
                Username = $@"Test:{Guid.NewGuid()}",
                CreatedAt = DateTime.Now
            };

            _arrayByte = _pool.Rent(1024);
            _stream = new MemoryStream(_arrayByte);
        }

        [IterationCleanup]
        public void Cleanup()
        {
            if (_arrayByte != null)
                _pool.Return(_arrayByte, true);
            _stream?.Dispose();
        }

        [Benchmark]
        public void SerializeRoslyn()
        {
            _stream.Position = 0;
            _userProfile.SerializeToBinary(_stream);
        }

        [Benchmark]
        public void SerializeJson()
        {
            var result = JsonSerializer.SerializeToUtf8Bytes(_userProfile);
        }

        public static void Execute()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithOption(ConfigOptions.StopOnFirstError, true)
            .AddJob(Job.Default
            .WithWarmupCount(500)
            .WithIterationCount(3000));

            BenchmarkRunner.Run<TestCaseSerialize>(config);
        }
    }
}
