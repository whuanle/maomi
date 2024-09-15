// <copyright file="Res.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;

namespace Maomi.Web.Core;

/// <summary>
/// 分页结果模型类.
/// </summary>
/// <typeparam name="T">类型.</typeparam>
public partial class PageArrayRes<T> : Res<PageRes<T[]>>
{
}
