using InMemoryKeyValueCache.Common.Helpers;
using InMemoryKeyValueCache.Common.Models;
using InMemoryKeyValueCache.TcpServer.NBomber.Helpers;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System.Text;

namespace InMemoryKeyValueCache.TcpServer.NBomber.Tests
{
    public class InMemoryCacheNbomberTest
    {
        private readonly Random _rnd = new Random(10000);
        
        public async Task Execute()
        {
            var scenario = Scenario.Create("tcp_client_test_scenario", async context =>
            {
                return await Step.Run("tcp_client_test", context, async () =>
                {
                    try
                    {
                        var _tcpServerClient = new InMemoryCacheTcpServerClient();

                        var key = $@"user:{Guid.NewGuid()}";
                        
                        var value = new UserProfile()
                        {
                            Id = _rnd.Next(),
                            Username = $@"User:{Guid.NewGuid()}",
                            CreatedAt = DateTime.Now
                        };

                        var responseSet = await _tcpServerClient.SetAsync(key, value);

                        if (Encoding.UTF8.GetString(responseSet) != Encoding.UTF8.GetString(CommonBytesData.Ok))
                            return Response.Fail();

                        var responseGet = await _tcpServerClient.GetAsync(key);
                        if (responseGet == null)
                            return Response.Fail();

                        return Response.Ok();
                    }
                    catch (Exception)
                    {
                        return Response.Fail();
                    }
                });
            })
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)))
                .WithRestartIterationOnFail(false);

            NBomberRunner.RegisterScenarios(scenario)
                .WithReportFormats(ReportFormat.Txt)
                .WithReportFolder(Path.Combine(AppContext.BaseDirectory, "Reports"))
                .WithReportFileName($@"Report_{Guid.NewGuid()}")
                .Run();
        }
    }
}
