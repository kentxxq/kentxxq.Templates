## 使用文档

### 基础配置
- 默认`不启用https`端口，因为前端肯定挂nginx/yarp做负载均衡或ssl
- 启用了`metrics`，`健康检查，健康检查UI`，`swagger`，`mapper的静态拓展方法`
- 返回的请求头中包含有`TraceId`

### 日志配置
日志配置在`program.cs`中的第一段，采用serilog

### quartz定时任务
默认使用内存模式，建议使用数据库存储。可以在`appsettings.json`中查看配置

### webapi声明式调用
自定义token[参考文档](https://github.com/dotnetcore/WebApiClient#%E8%87%AA%E5%AE%9A%E4%B9%89tokenprovider)


## TODO
- ~~启动的时候输出关键的路由信息，例如swagger入口，metrics等~~
- net7发布的时候，加上`JWT`！参考https://stackoverflow.com/questions/38751616/asp-net-core-identity-get-current-user,还有https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes?view=net-6.0。还可以使用user-jwts工具！
- ~~生成一个数据库？~~
- net7发布的时候，grpc?https://devblogs.microsoft.com/dotnet/asp-net-core-updates-in-dotnet-7-rc-1/#experimental-openapi-support-for-grpc-json-transcoding
- net7发布的时候，限速api?https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/
- source-generator做策略模式？！
- ~~startup健康检查？~~
- net7发布的时候，通过eventSource计算启动时间，同时更新startup健康检查。参考https://github.com/dotnet/aspnetcore/blob/v7.0.0-preview.5.22303.8/src/Hosting/Hosting/src/Internal/HostingEventSource.cs#L75-L79，https://github.com/dotnet/aspnetcore/blob/v7.0.0-preview.5.22303.8/src/Hosting/Hosting/src/Internal/HostingEventSource.cs#L75-L79，https://learn.microsoft.com/zh-cn/dotnet/core/diagnostics/eventsource-collect-and-view-traces#eventlistener，https://learn.microsoft.com/zh-cn/dotnet/core/diagnostics/diagnostics-client-library#parse-events-in-real-time