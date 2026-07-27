using HW18_SIMD.Helper;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System.Text;

namespace HW18_SIMD_Test
{
    public class TestCase
    {
        private readonly Random _rnd = new Random(10000);
        private const string OK = "OK\r\n";
        public async Task StartTest()
        {
            var scenario = Scenario.Create("tcp_client_test_scenario", async context =>
            {
                return await Step.Run("tcp_client_test", context, async () =>
                {
                    try
                    {
                        var _tcpServerClient = new TcpServerClient();

                        var key = $@"user:{Guid.NewGuid()}";
                        var value = new UserProfile()
                        {
                            Id = _rnd.Next(),
                            Username = $@"User:{Guid.NewGuid()}",
                            CreatedAt = DateTime.Now
                        };

                        var response = await _tcpServerClient.SetAsync(key, value);

                        if (Encoding.UTF8.GetString(response).ToUpper() == OK)
                            return Response.Ok();

                        return Response.Fail();
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
