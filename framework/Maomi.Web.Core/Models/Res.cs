// <copyright file="Res.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;

namespace Maomi.Web.Core;

/// <summary>
/// 响应模型类.
/// </summary>
/// <typeparam name="T">类型.</typeparam>
public class Res<T>
{
    /// <summary>
    /// 当前请求是否有错误.
    /// </summary>
    public virtual bool IsSuccess => Code == 200;

    /// <summary>
    /// 业务代码.
    /// </summary>
    public virtual int Code { get; set; }

    /// <summary>
    /// 响应消息.
    /// </summary>
    public virtual string Msg { get; set; } = default!;

    /// <summary>
    /// 返回数据.
    /// </summary>
    public virtual T? Data { get; set; }
}

/// <summary>
/// 响应模型类.
/// </summary>
public partial class Res : Res<object?>
{
    /// <summary>
    /// 创建 <see cref="Res"/>.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="code">响应代码.</param>
    /// <param name="message">响应信息.</param>
    /// <param name="data">响应内容.</param>
    /// <returns><see cref="Res"/>.</returns>
    public static Res<T> Create<T>(int code, string message, T data)
    {
        return new Res<T>
        {
            Code = code,
            Msg = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建 <see cref="Res"/>.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="code">响应代码.</param>
    /// <param name="message">响应信息.</param>
    /// <returns><see cref="Res"/>.</returns>
    public static Res Create<T>(int code, string message)
    {
        return new Res
        {
            Code = code,
            Msg = message
        };
    }

    /// <summary>
    /// 创建 <see cref="Res"/>.
    /// </summary>
    /// <param name="code">响应代码.</param>
    /// <param name="message">响应信息.</param>
    /// <returns><see cref="Res"/>.</returns>
    public static Res Create(int code, string message)
    {
        return new Res
        {
            Code = code,
            Msg = message
        };
    }

    /// <summary>
    /// 创建 <see cref="Res"/>.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="code">响应代码.</param>
    /// <param name="message">响应信息.</param>
    /// <param name="data">响应内容.</param>
    /// <returns><see cref="Res"/>.</returns>
    public static Res<T> Create<T>(HttpStatusCode code, string message, T data) => Create((int)code, message, data);
}