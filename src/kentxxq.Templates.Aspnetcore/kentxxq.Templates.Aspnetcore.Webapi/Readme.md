﻿## 使用文档

### 基础配置
- 默认`不启用https`端口，因为前端肯定挂nginx/yarp做负载均衡或ssl
- 启用了`metrics`，`健康检查，健康检查UI`，`swagger`
- 返回的请求头中包含有`TraceId`

### 日志配置
日志配置在`program.cs`中的第一段，采用serilog

### quartz定时任务
默认使用内存模式，建议使用数据库存储。可以在`appsettings.json`中查看配置

### webapi声明式调用
自定义token[参考文档](https://github.com/dotnetcore/WebApiClient#%E8%87%AA%E5%AE%9A%E4%B9%89tokenprovider)




## TODO
1. 启动的时候输出关键的路由信息，例如swagger入口，metrics等
2. net7发布的时候，加上`JWT`！参考https://stackoverflow.com/questions/38751616/asp-net-core-identity-get-current-user,还有https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes?view=net-6.0
3. 生成一个数据库？
4. net7发布的时候，grpc?https://devblogs.microsoft.com/dotnet/asp-net-core-updates-in-dotnet-7-rc-1/#experimental-openapi-support-for-grpc-json-transcoding
5. net7发布的时候，限速api?https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/