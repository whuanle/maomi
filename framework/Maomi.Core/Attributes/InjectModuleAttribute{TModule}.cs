// <copyright file="InjectModuleAttribute{TModule}.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi;

/// <summary>
/// 注册依赖的模块.
/// </summary>
/// <typeparam name="TModule">模块类.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InjectModuleAttribute<TModule> : InjectModuleAttribute
    where TModule : IModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InjectModuleAttribute{TModule}"/> class.
    /// </summary>
    public InjectModuleAttribute()
        : base(typeof(TModule))
    {
    }
}
