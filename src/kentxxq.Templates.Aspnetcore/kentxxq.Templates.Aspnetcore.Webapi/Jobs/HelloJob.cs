#if (EnableQuartz)
using Quartz;

namespace kentxxq.Templates.Aspnetcore.Webapi.Jobs
{
    /// <summary>
    /// 一个demo
    /// </summary>
    public class HelloJob : IJob
    {
        /// <summary>
        /// job的执行入口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now},Greetings from HelloJob!");
        }
    }
}
#endif