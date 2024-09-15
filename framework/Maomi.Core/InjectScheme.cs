// <copyright file="InjectScheme.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 服务注册模式.
/// </summary>
public enum InjectScheme
{
    /// <summary>
    /// <see cref="ServiceDescriptor.ServiceType"/> 包括父类、接口.
    /// <code>
    /// public class C: CBase, IC {} <br />
    /// services.AddScoped&lt;CBase, C&gt;(); <br />
    /// services.AddScoped&lt;IC, C&gt;(); <br />
    /// </code>
    /// </summary>
    Any,

    /// <summary>
    /// 自定义 <see cref="InjectOnAttribute.ServiceTypes"/> 服务列表.
    /// </summary>
    Some,

    /// <summary>
    /// 只注入父类.
    /// </summary>
    OnlyBaseClass,

    /// <summary>
    /// 只注册接口.
    /// </summary>
    OnlyInterfaces,

    /// <summary>
    /// 此服务不会被注入到容器中.
    /// </summary>
    None
}
