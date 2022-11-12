using kentxxq.Templates.Aspnetcore.Webapi.Common;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Demo;
using Microsoft.AspNetCore.Mvc;
#if (EnableDB)
using kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo;
#endif
#if (EnableQuartz)
using Quartz;
using Quartz.Impl.Matchers;
#endif
#if (EnableNacos)
using Nacos.AspNetCore.V2;
using Nacos.V2;
using Microsoft.Extensions.Options;
#endif
using Microsoft.AspNetCore.Authorization;

namespace kentxxq.Templates.Aspnetcore.Webapi.Controllers;

/// <summary>
/// demo的api
/// </summary>
[ApiExplorerSettings(GroupName = "Examples")]
[Route("api/[controller]/[action]")]
[ApiController]
public class DemoController : ControllerBase
{
    private readonly IDemoService _demoService;
    private readonly JWTService _jwtService;
    private readonly IIpService _ipService;
#if (EnableQuartz)
    private readonly ISchedulerFactory _schedulerFactory;
#endif
#if (EnableDB)
    private readonly IUserService _userService;
#endif
#if (EnableNacos)
    private readonly NacosSettings _appSettings;
    private readonly NacosSettings _mAppSettings;
    private readonly NacosAspNetOptions _nacosAspNetOptions;
    private readonly INacosNamingService _nacosNamingService;
    private readonly NacosSettings _sAppSettings;
#endif


    /// <inheritdoc />
    public DemoController(
        IDemoService demoService
        ,JWTService jwtService
        , IIpService ipService
#if (EnableQuartz)
        , ISchedulerFactory schedulerFactory
#endif
#if (EnableDB)
        , IUserService userService
#endif
#if (EnableNacos)
        , INacosNamingService nacosNamingService
        , IOptions<NacosAspNetOptions> nacosAspNetOptions
        , IOptions<NacosSettings> appSetting
        , IOptionsSnapshot<NacosSettings> sAppSettings
        , IOptionsMonitor<NacosSettings> mAppSettings
#endif
    )
    {
        _demoService = demoService;
        _jwtService = jwtService;
        _ipService = ipService;
#if (EnableQuartz)
        _schedulerFactory = schedulerFactory;
#endif
#if (EnableDB)
        _userService = userService;
#endif
#if (EnableNacos)
        _appSettings = appSetting.Value;
        _sAppSettings = sAppSettings.Value;
        _mAppSettings = mAppSettings.CurrentValue;
        _nacosAspNetOptions = nacosAspNetOptions.Value;
        _nacosNamingService = nacosNamingService;
#endif
    }

    /// <summary>
    /// 获取demo数据
    /// </summary>
    /// <returns>demo数据</returns>
    [HttpGet]
    public async Task<ResultModel<string>> GetData()
    {
        return ResultModel<string>.Ok(await _demoService.GetData());
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
    public async Task<ResultModel<string>> Login(string username, string password)
    {
        var user = await _userService.Login(username, password);
        if (user is not null)
        {
            var token = _jwtService.GetToken(user.Id, user.Username, new List<string>{ "admin" });
            return ResultModel<string>.Ok(token);
        }
        else
        {
            return ResultModel<string>.Error("登录失败，用户名或密码错误","");
        }
        
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


#if (EnableNacos)
    /// <summary>
    /// 获取注册上去的主机连接信息
    /// </summary>
    /// <returns>ip:port</returns>
    [HttpGet]
    public ResultModel<string> GetHost()
    {
        var instance = _nacosNamingService.SelectOneHealthyInstance("kentxxq.Templates.Aspnetcore", "dev_demo_group").GetAwaiter()
            .GetResult();
        var host = $"{instance.Ip}:{instance.Port}";
        return ResultModel<string>.Ok(host);
    }

    /// <summary>
    /// 获取自己的配置信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ResultModel<List<NacosSettings>> GetAppSettings()
    {
        var data = new List<NacosSettings> { _appSettings, _sAppSettings, _mAppSettings };
        return ResultModel<List<NacosSettings>>.Ok(data);
    }

    /// <summary>
    /// 获取注册上去的实例信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ResultModel<NacosAspNetOptions> GetNacosAspNetOptions()
    {
        return ResultModel<NacosAspNetOptions>.Ok(_nacosAspNetOptions);
    }
#endif


    /// <summary>
    /// 客户端缓存10s，不会发送新的请求过来
    /// location.client代表只能在浏览器存储，不能由cdn，nginx等中间层缓存. any代表中间层也可以缓存。none可以理解为代表no-cache，仅中间件缓存。no-store单独设置bool值，直接不缓存
    /// https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/response?view=aspnetcore-6.0#http-based-response-caching
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Client)]
    public async Task<string> TestClientCache()
    {
        await Task.Delay(2000);
        return "1";
    }

    /// <summary>
    /// 测试内存缓存
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> TestLocalCacheData()
    {
        var data = await _demoService.GetLocalCacheData();
        return data.ToString();
    }

#if (EnableRedis)
    /// <summary>
    /// 测试redis缓存
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> TestRedisCacheData()
    {
        var data = await _demoService.GetRedisCacheData();
        return data.ToString();
    }
#endif

    /// <summary>
    /// 不需要授权，只要token正确即可
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public string GetAuthData()
    {
        return "AuthData";
    }

    /// <summary>
    /// 角色是admin或者superadmin
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes ="Bearer",Roles ="admin,superadmin")]
    [HttpGet]
    public string GetAdminData()
    {
        return "AdminData";
    }
    
    /// <summary>
    /// 指定is_allow策略，自定义策略
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes ="Bearer",Policy ="is_allow")]
    [HttpGet]
    public string GetAllowData()
    {
        return "AllowData";
    }
}