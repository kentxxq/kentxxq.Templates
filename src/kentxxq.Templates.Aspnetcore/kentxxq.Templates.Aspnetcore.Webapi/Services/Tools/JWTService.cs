using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services.Tools
{
    /// <summary>
    /// JWT工具
    /// </summary>
    public class JWTService
    {
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="username">用户名</param>
        /// <param name="roles">角色</param>
        /// <param name="schemaName">验证方案名称，默认Bearer</param>
        /// <returns></returns>
        public string GetToken(int uid, string username, IEnumerable<string> roles,string schemaName="Bearer")
        {
            var secret = "";
            var issuer = _configuration.GetValue<string>($"Authentication:Schemes:{schemaName}:ValidIssuer");
            var signingKey = _configuration.GetSection($"Authentication:Schemes:{schemaName}:SigningKeys")
                .GetChildren()
                .SingleOrDefault(key=>key["Issuer"] == issuer);
            if (signingKey?["Value"] is string keyValue)
            {
                secret = keyValue;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity("ken-subject"),
                Claims = new Dictionary<string, object>()
                {
                    {JwtClaimTypes.Id,uid},
                    {JwtClaimTypes.NickName,username},
                    {JwtClaimTypes.Role,string.Join(",",roles)}
                },
                // 签证机构的名称
                Issuer = issuer,
                // 受众。签证机构把认证给了ken
                Audience = issuer,
                // 签发时间
                IssuedAt = DateTime.Now,
                // 在这之前不可用.作用是1点签发token，允许2点开始生效，生效1小时到3点
                NotBefore = DateTime.Now,
                // 2小时后过期
                Expires = DateTime.Now.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(secret)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var stringToken = tokenHandler.WriteToken(token);

            return stringToken;
        }
    }
}
