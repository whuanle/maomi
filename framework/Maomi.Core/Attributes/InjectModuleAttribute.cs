// <copyright file="InjectModuleAttribute.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi;

/// <summary>
/// 注册依赖的模块.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class InjectModuleAttribute : Attribute
{
    /// <summary>
    /// 依赖的模块.
    /// </summary>
    public Type ModuleType { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectModuleAttribute"/> class.
    /// </summary>
    /// <param name="type"></param>
    public InjectModuleAttribute(Type type)
    {
        ModuleType = type;
    }
}