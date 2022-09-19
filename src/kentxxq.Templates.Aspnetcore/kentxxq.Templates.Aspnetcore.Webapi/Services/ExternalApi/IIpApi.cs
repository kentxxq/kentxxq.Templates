using WebApiClientCore.Attributes;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi
{
    /// <summary>
    /// ip-api的接口生成
    /// </summary>
    [HttpHost("http://ip-api.com/")]
    public interface IIpApi
    {
        /// <summary>
        /// 获取ip信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpGet("json/{ip}?lang=zh-CN")]
        Task<IpApiModels> GetIpInfoAsync(string ip);
    }
}
