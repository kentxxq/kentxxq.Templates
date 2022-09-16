namespace kentxxq.Templates.Aspnetcore.Webapi.Common;

/// <summary>
/// 统一返回模型
/// </summary>
/// <typeparam name="T"></typeparam>
public class ResultModel<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; } = 20000;

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    /// <param name="data">返回数据</param>
    /// <returns></returns>
    public static ResultModel<T> Ok(T data)
    {
        return new ResultModel<T> { Data = data };
    }

    /// <summary>
    /// 失败
    /// </summary>
    /// <param name="errorStr">错误信息</param>
    /// <param name="code">状态码</param>
    /// <returns></returns>
    public static ResultModel<T> Error(T errorStr, int code = 50000)
    {
        return new ResultModel<T> { Data = errorStr, Code = code };
    }
}