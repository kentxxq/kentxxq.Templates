using kentxxq.Templates.Aspnetcore.Webapi.Common;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
#if (EnableDB)
using kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo;
#endif
using kentxxq.Templates.Aspnetcore.Webapi.SO.Demo;
using Microsoft.AspNetCore.Mvc;
#if (EnableQuartz)
using Quartz;
using Quartz.Impl.Matchers;
#endif

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
#if (EnableQuartz)
    private readonly ISchedulerFactory _schedulerFactory;
#endif
#if (EnableDB)
    private readonly IUserService _userService;
#endif

    /// <inheritdoc/>
    public DemoController(
        IDemoService demoService
        , IIpService ipService
#if (EnableQuartz)
        , ISchedulerFactory schedulerFactory
#endif
#if (EnableDB)
        , IUserService userService
#endif
        )
    {
        _demoService = demoService;
        _ipService = ipService;
#if (EnableQuartz)
        _schedulerFactory = schedulerFactory;
#endif
#if (EnableDB)
        _userService = userService;
#endif
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

#if (EnableQuartz)
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
#endif

#if (EnableDB)
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<bool>> Login(string username, string password)
    {
        var result = await _userService.Login(username, password);
        return ResultModel<bool>.Ok(result);
    }

    /// <summary>
    /// 通过用户名查询用户地址信息
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultModel<IEnumerable<AddressSO>>> GetUserAddressByUsername(string username)
    {
        var data = await _userService.GetUserAddressByUsername(username);
        var result = data.Select(a => Mapper.AddressToAddressSO(a));
        return ResultModel<IEnumerable<AddressSO>>.Ok(result);
    }
#endif
}
