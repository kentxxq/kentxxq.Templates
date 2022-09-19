using kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi;
using WebApiClientCore;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services.Tools
{
    /// <inheritdoc/>
    public class IpService : IIpService
    {
        private readonly IIpApi _ipApi;
        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="ipApi"></param>
        public IpService(IIpApi ipApi)
        {
            _ipApi = ipApi;
        }

        /// <inheritdoc/>
        public async Task<IpServiceModel> GetIpInfo(string ip)
        {
            var data = await _ipApi.GetIpInfoAsync(ip).Retry(1);
            if (data.Status != "success")
            {
                throw new ApplicationException("查询失败");
            }
            var result = new IpServiceModel { Status = data.Status, Country = data.Country, RegionName = data.RegionName, Isp = data.Isp, City = data.City };
            return result;
        }
    }
}
