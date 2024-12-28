// <copyright file="ModuleOptions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Reflection;

namespace Maomi;

/// <summary>
/// 初始化配置.
/// </summary>
public class ModuleOptions
{
    /// <summary>
    /// 注册服务时要过滤的类型或接口，这些类型不会被注册到容器中.
    /// </summary>
    public ICollection<Type> FilterServiceTypes { get; private set; } = new List<Type>()
    {
        typeof(IDisposable),
        typeof(ICloneable),
        typeof(IComparable),
        typeof(object)
    };

    /// <summary>
    /// 自定义要注册的程序集.
    /// </summary>
    public ICollection<Assembly> CustomAssembies { get; private set; } = new List<Assembly>();
}
