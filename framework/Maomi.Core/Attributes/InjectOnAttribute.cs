// <copyright file="InjectOnAttribute.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Maomi;

/// <summary>
/// 将当前类型自动注册到容器中.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectOnAttribute : Attribute
{
    /// <summary>
    /// 要注册的服务类型.
    /// </summary>
    public Type[]? ServiceTypes { get; set; } = Array.Empty<Type>();

    /// <summary>
    /// 服务的生命周期.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// 服务注册模式.
    /// </summary>
    public InjectScheme Scheme { get; set; }

    /// <summary>
    /// 将自己也注册到容器中.
    /// </summary>
    public bool Own { get; set; } = false;

    /// <summary>
    /// ServiceKey.
    /// </summary>
    public object? ServiceKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectOnAttribute"/> class.
    /// </summary>
    /// <param name="lifetime">服务生命周期.</param>
    /// <param name="scheme">服务注册模式.</param>
    public InjectOnAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, InjectScheme scheme = InjectScheme.OnlyInterfaces)
    {
        Lifetime = lifetime;
        Scheme = scheme;
    }
}
