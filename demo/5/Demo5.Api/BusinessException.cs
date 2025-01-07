namespace Demo5.Api;

/// <summary>
/// 业务异常类型.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 异常代码.
    /// </summary>
    public int? Code { get; set; }

    /// <summary>
    /// 异常描述.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// 参数.
    /// </summary>
    public object[]? Paramters { get; set; }

    /// <summary>
    /// 异常级别.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Error;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="paramters"></param>
    public BusinessException(
    int? code = null,
    string? message = null,
    params object[] paramters)
    : base(message)
    {
        Code = code;
        Paramters = paramters;
    }

    /// <summary>
    /// 记录额外的异常信息.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns><see cref="BusinessException"/>.</returns>
    public BusinessException WithData(string name, object value)
    {
        Data[name] = value;
        return this;
    }
}