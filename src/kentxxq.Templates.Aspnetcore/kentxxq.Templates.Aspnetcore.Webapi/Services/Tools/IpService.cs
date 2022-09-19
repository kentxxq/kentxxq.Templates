using kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi;

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
            var data = await _ipApi.GetIpInfoAsync(ip);
            var result = new IpServiceModel { Status = data.Status, Country = data.Country, RegionName = data.RegionName, Isp = data.Isp, City = data.City };
            return result;
        }
    }
}
