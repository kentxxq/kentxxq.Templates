namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// jwt配置-拓展方法
/// </summary>
public static class MyJWTExtension
{
    /// <summary>
    /// 添加jwt配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddMyJWT(this IServiceCollection service)
    {
        service.AddAuthentication("Bearer") // controller没有配置方案的时候，使用Bearer认证
            .AddJwtBearer() // 等同于AddJwtBearer("Bearer")
            .AddJwtBearer("Test");

        service.AddAuthorization(options =>
        {
            // 角色不能满足，或多种条件组合的时候。采用自定义
            // dotnet user-jwts create  --claim is_allow=true --claim ken=ken_allow --role admin --role superadmin --audience dotnet-user-jwts  
            options.AddPolicy("is_allow", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("is_allow", "true");
                // 多种claim条件组合
                policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c =>
                        c.Type == "ken" && (c.Value == "ken_allow" || c.Value == "admin_allow"));
                });
            });
        });
        return service;
    }
}