namespace kentxxq.Templates.Aspnetcore.Webapi.Services.Tools
{
    /// <summary>
    /// ip服务接口
    /// </summary>
    public interface IIpService
    {
        /// <summary>
        /// 获取ip信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<IpServiceModel> GetIpInfo(string ip);
    }
}
