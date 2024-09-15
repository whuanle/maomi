// <copyright file="Res.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;

namespace Maomi.Web.Core;

/// <summary>
/// 分页结果模型类.
/// </summary>
/// <typeparam name="T">类型.</typeparam>
public class PageRes<T>
{
    /// <summary>
    /// 当前页.
    /// </summary>
    public virtual int PageNo { get; set; }

    /// <summary>
    /// 页大小.
    /// </summary>
    public virtual int PageSize { get; set; }

    /// <summary>
    /// 列表.
    /// </summary>
    public virtual T? List { get; set; }
}
