using kentxxq.Templates.Aspnetcore.Webapi.Common;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Tools;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="demoService"></param>
    /// <param name="ipService"></param>
    public DemoController(IDemoService demoService, IIpService ipService)
    {
        _demoService = demoService;
        _ipService = ipService;
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
}