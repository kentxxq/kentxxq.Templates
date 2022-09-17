#if (EnableQuartz)
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Jobs;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;

namespace kentxxq.Templates.Aspnetcore.Webapi.Controllers
{
    /// <summary>
    /// 调度api
    /// </summary>
    [ApiExplorerSettings(GroupName = "V1")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="schedulerFactory"></param>
        public JobController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        /// <summary>
        /// 获取scheduler状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultModel<SchedulerStatusSO>> GetStatus()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobGroups = await scheduler.GetJobGroupNames();
            var triggerGroups = await scheduler.GetTriggerGroupNames();
            var runningJobs = await scheduler.GetCurrentlyExecutingJobs();
            var metaData = await scheduler.GetMetaData();
            var jobs = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var triggers = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            var data = new SchedulerStatusSO
            {
                SchedulerId = metaData.SchedulerInstanceId,
                SchedulerName = metaData.SchedulerName,
                Status = scheduler.IsStarted ? "running" : "stopped",
                SchedulerType = metaData.SchedulerType.ToString(),
                StartTime = metaData.RunningSince,
                NumberOfJobsExec = metaData.NumberOfJobsExecuted,
                JobStoreType = metaData.JobStoreType.ToString(),
                JobStorePersistent = metaData.JobStoreSupportsPersistence,
                JobStoreClustered = metaData.JobStoreClustered,
                ThreadPoolType = metaData.ThreadPoolType.ToString(),
                ThreadPoolSize = metaData.ThreadPoolSize,
                Version = metaData.Version,
                RunningJobs = runningJobs.Select(key => key.ToString() ?? "").ToList(),
                Jobs = jobs.Select(key => key.ToString()).ToList(),
                Triggers = triggers.Select(key => key.ToString()).ToList()
            };
            return ResultModel<SchedulerStatusSO>.Ok(data);
        }
    }
}
#endif