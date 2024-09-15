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
public class InjectOnSingletonAttribute : InjectOnAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InjectOnSingletonAttribute"/> class.
    /// </summary>
    /// <param name="scheme">服务注册模式.</param>
    public InjectOnSingletonAttribute(InjectScheme scheme = InjectScheme.OnlyInterfaces)
        : base(ServiceLifetime.Singleton, scheme)
    {
    }
}
