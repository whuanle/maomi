// <copyright file="ModuleRecord.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Reflection;

namespace Maomi;

/// <summary>
/// 模块记录.
/// </summary>
public class ModuleRecord : IComparable<ModuleRecord>
{
    /// <summary>
    /// 模块所在的程序集.
    /// </summary>
    public Assembly Assembly { get; init; } = default!;

    /// <summary>
    /// 模块类.
    /// </summary>
    public Type Type { get; init; } = default!;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj != null && obj is ModuleRecord record)
        {
            return CompareTo(record) == 0;
        }

        return false;
    }

    /// <inheritdoc />
    public int CompareTo(ModuleRecord? other)
    {
        if (other == null)
        {
            return -1;
        }

        return other.Type == Type ? 0 : string.Compare(Type.FullName, other.Type.FullName);
    }
}