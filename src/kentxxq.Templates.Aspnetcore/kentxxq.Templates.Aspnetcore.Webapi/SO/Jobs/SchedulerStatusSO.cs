namespace kentxxq.Templates.Aspnetcore.Webapi.SO.Jobs;

/// <summary>
/// 调度器状态SO
/// </summary>
public class SchedulerStatusSO
{
    /// <summary>
    /// 调度器id
    /// </summary>
    public string SchedulerId { get; set; } = string.Empty;

    /// <summary>
    /// 调度器名称
    /// </summary>
    public string SchedulerName { get; set; } = string.Empty;

    /// <summary>
    /// 运行状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 调度器类型
    /// </summary>
    public string SchedulerType { get; set; } = string.Empty;

    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// 运行job的次数
    /// </summary>
    public int NumberOfJobsExec { get; set; }

    /// <summary>
    /// 存储类型
    /// </summary>
    public string JobStoreType { get; set; } = string.Empty;

    /// <summary>
    /// 永久存储
    /// </summary>
    public bool JobStorePersistent { get; set; }

    /// <summary>
    /// 是否集群
    /// </summary>
    public bool JobStoreClustered { get; set; }

    /// <summary>
    /// 线程池类型
    /// </summary>
    public string ThreadPoolType { get; set; } = string.Empty;

    /// <summary>
    /// 线程池大小
    /// </summary>
    public int ThreadPoolSize { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 运行中的job
    /// </summary>
    public List<string> RunningJobs { get; set; } = new();

    /// <summary>
    /// 所有的job
    /// </summary>
    public List<string> Jobs { get; set; } = new();

    /// <summary>
    /// 所有的触发器
    /// </summary>
    public List<string> Triggers { get; set; } = new();
}