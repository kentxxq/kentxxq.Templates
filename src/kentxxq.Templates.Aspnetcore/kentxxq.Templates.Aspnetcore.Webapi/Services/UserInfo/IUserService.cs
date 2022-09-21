using kentxxq.Templates.Aspnetcore.DB;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo
{
    /// <summary>
    /// 用户接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        Task<bool> Login(string username, string password);

        /// <summary>
        /// 获取用户的地址列表
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        Task<List<Address>> GetUserAddressByUsername(string username);
    }
}
