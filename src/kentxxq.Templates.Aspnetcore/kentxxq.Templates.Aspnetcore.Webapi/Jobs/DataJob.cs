#if (EnableQuartz)
using Quartz;

namespace kentxxq.Templates.Aspnetcore.Webapi.Jobs
{
    /// <summary>
    /// 传递数据的job
    /// </summary>
    public class DataJob : IJob
    {
        private readonly ILogger<DataJob> _logger;

        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="logger"></param>
        public DataJob(ILogger<DataJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// job的执行入口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Execute(IJobExecutionContext context)
        {
            var data = context.MergedJobDataMap.GetString("data");
            _logger.LogInformation($"{data},output from {nameof(DataJob)}!");
            await Task.Yield();
        }
    }
}
#endif