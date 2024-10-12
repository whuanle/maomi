// <copyright file="PageListRes{T}.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Net;

namespace Maomi.Web.Core;

/// <summary>
/// 分页结果模型类.
/// </summary>
/// <typeparam name="T">类型.</typeparam>
public partial class PageListRes<T> : Res<PageRes<IEnumerable<T>>>
{
}
