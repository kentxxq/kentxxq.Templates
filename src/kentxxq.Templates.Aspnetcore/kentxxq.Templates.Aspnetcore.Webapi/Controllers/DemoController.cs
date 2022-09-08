using kentxxq.Templates.Aspnetcore.Webapi.Services;
using Microsoft.AspNetCore.Mvc;

namespace kentxxq.Templates.Webapi.Controllers
{
    /// <summary>
    /// demo的api
    /// </summary>
    [ApiExplorerSettings(GroupName = "V1")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IDemoService _demoService;

        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="demoService"></param>
        public DemoController(IDemoService demoService)
        {
            _demoService = demoService;
        }

        /// <summary>
        /// 获取demo数据
        /// </summary>
        /// <returns>demo数据</returns>
        [HttpGet]
        public string GetData()
        {
            return _demoService.GetData();
        }
    }
}
