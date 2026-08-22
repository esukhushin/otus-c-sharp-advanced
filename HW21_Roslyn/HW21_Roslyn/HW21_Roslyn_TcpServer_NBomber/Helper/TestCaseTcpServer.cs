using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System.Text;
using HW21_Roslyn_TcpServer.Helper;
using NBomber.Contracts;
using HW21_Roslyn_TcpServer_NBomber.Interface;

namespace HW21_Roslyn_TcpServer_NBomber.Helper
{
    public class TestCaseTcpServer
    {
        private readonly Random _rnd = new Random(10);
        private const string OK = "OK\r\n";
        public void Execute()
        {
            NBomberRunner.RegisterScenarios(GetScenario().ToArray())
                .WithReportFormats(ReportFormat.Txt)
                .WithReportFolder(Path.Combine(AppContext.BaseDirectory, "Reports"))
                .WithReportFileName($@"Report_{Guid.NewGuid()}")
                .Run();
        }

        private List<ScenarioProps> GetScenario()
        {
            var scenario = new List<ScenarioProps>();
            foreach (var item in new[] { "roslyn", "json" })
            {
                scenario.Add(Scenario.Create($@"tcp_client_test_{item}_scenario", async context =>
                {
                    return await Step.Run($@"tcp_client_{item}_roslyn", context, async () =>
                    {
                        switch (item)
                        {
                            case "roslyn":
                                return await ExecuteScenario(new TcpServerRoslynClient());
                            case "json":
                                return await ExecuteScenario(new TcpServerJsonClient());
                            default:
                                return Response.Fail();
                        }
                    });
                })
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)))
                .WithRestartIterationOnFail(false));
            }
            return scenario;
        }
        private async Task<Response<object>> ExecuteScenario(ITcpServerClient client)
        {
            try
            {
                var key = $@"user:{Guid.NewGuid()}";
                var value = new UserProfile()
                {
                    Id = _rnd.Next(),
                    Username = $@"User:{Guid.NewGuid()}",
                    CreatedAt = DateTime.Now
                };

                var response = await client.SetAsync(key, value);

                if (Encoding.UTF8.GetString(response).ToUpper() == OK)
                    return Response.Ok();

                return Response.Fail();
            }
            catch (Exception)
            {
                return Response.Fail();
            }
        }
    }
}
