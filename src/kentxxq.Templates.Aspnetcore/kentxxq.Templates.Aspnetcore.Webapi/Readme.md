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