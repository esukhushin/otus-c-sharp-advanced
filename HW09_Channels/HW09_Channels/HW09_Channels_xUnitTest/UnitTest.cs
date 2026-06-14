using HW09_Channels;
using System.Text;

namespace HW09_Channels_xUnitTest
{
    public class UnitTest
    {
        private const int _count = 1000;

        [Fact]
        public async Task TestTasks()
        {
            var store = new SimpleStore();

            var tstTasks = new List<Task>();

            for (int i = 1; i <= _count; i++)
            {
                var id = i;

                tstTasks.Add(Task.Run(() =>
                {
                    store.Set($@"Key_{id}", Encoding.UTF8.GetBytes($@"Test_{id}"));
                }));
                tstTasks.Add(Task.Run(() =>
                {
                    var result = store.Get($@"Key_{id}");
                }));
            }

            await Task.WhenAll(tstTasks);

            var statistics = store.GetStatistics();

            Assert.Equal(_count, statistics.SetCount);
            Assert.Equal(_count, statistics.GetCount);
        }
    }
}
