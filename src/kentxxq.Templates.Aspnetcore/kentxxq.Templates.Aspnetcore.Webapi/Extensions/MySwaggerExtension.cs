using Microsoft.OpenApi.Models;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// swagger-拓展方法
/// </summary>
public static class MySwaggerExtension
{
    /// <summary>
    /// 添加swagger配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddMySwagger(this IServiceCollection service)
    {
        
        service.AddEndpointsApiExplorer();
        service.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("Examples", new OpenApiInfo { Title = "Examples", Version = "v1" });

            // JWT
            s.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                // http是Header带 Authorization: Bearer ZGVtbzpwQDU1dzByZA==
                // apikey 是下面3中方式
                // 参数带 /something?api_key=abcdef12345
                // header带 X-API-Key: abcdef12345
                // cookie带 Cookie: X-API-KEY=abcdef12345
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });
            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    Array.Empty<string>()
                }
            });

            // xmlDoc
            var filePath = Path.Combine(AppContext.BaseDirectory, "MyApi.xml");
            s.IncludeXmlComments(filePath);
        });

        return service;
    }
}