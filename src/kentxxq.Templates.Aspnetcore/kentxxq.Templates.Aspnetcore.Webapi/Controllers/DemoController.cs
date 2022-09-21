using kentxxq.Templates.Aspnetcore.Webapi.Common;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Jobs;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Tools;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;

namespace kentxxq.Templates.Webapi.Controllers;

/// <summary>
/// demo的api
/// </summary>
[ApiExplorerSettings(GroupName = "Examples")]
[Route("api/[controller]/[action]")]
[ApiController]
public class DemoController : ControllerBase
{
    private readonly IDemoService _demoService;
    private readonly IIpService _ipService;
    private readonly ISchedulerFactory _schedulerFactory;

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="demoService"></param>
    /// <param name="ipService"></param>
    /// <param name="schedulerFactory"></param>
    public DemoController(IDemoService demoService, IIpService ipService, ISchedulerFactory schedulerFactory)
    {
        _demoService = demoService;
        _ipService = ipService;
        _schedulerFactory = schedulerFactory;
    }

    /// <summary>
    /// 获取demo数据
    /// </summary>
    /// <returns>demo数据</returns>
    [HttpGet]
    public ResultModel<string> GetData()
    {
        return ResultModel<string>.Ok(_demoService.GetData());
    }


    /// <summary>
    /// 获取ip数据
    /// </summary>
    /// <returns>demo数据</returns>
    [HttpGet]
    public async Task<ResultModel<IpSO>> GetIpInfo(string ip)
    {
        var data = await _ipService.GetIpInfo(ip);
        var result = Mapper.IpServiceModelToIpSO(data);
        return ResultModel<IpSO>.Ok(result);
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
