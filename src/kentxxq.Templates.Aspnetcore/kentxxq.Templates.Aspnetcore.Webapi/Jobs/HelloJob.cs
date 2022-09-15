#if (EnableQuartz)
using Quartz;

namespace kentxxq.Templates.Aspnetcore.Webapi.Jobs
{
    public class HelloJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now},Greetings from HelloJob!");
        }
    }
}
#endif
