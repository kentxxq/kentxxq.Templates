using kentxxq.Templates.Aspnetcore.DB;
using SqlSugar;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        /// <summary>
        /// 依赖注入
        /// </summary>
        public UserService(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }

        /// <inheritdoc/>
        public async Task<List<Address>> GetUserAddressByUsername(string username)
        {
            var data = await _sqlSugarClient.Queryable<User>()
                .LeftJoin<Address>((u, a) => u.Id == a.Uid)
                .Select((u, a) => a)
                .ToListAsync();
            return data;
        }

        /// <inheritdoc/>
        public async Task<bool> Login(string username, string password)
        {
            var result = await _sqlSugarClient.Queryable<User>()
                .Where(u => u.Username == username && u.Password == password)
                .AnyAsync();
            return result;
        }
    }
}
