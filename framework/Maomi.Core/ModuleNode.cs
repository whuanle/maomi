// <copyright file="ModuleNode.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

namespace Maomi;

/// <summary>
/// 模块节点.
/// </summary>
public class ModuleNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleNode"/> class.
    /// </summary>
    /// <param name="moduleType"></param>
    public ModuleNode(Type moduleType)
    {
        ModuleType = moduleType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleNode"/> class.
    /// </summary>
    /// <param name="moduleType"></param>
    /// <param name="parentModuleNode"></param>
    public ModuleNode(Type moduleType, ModuleNode parentModuleNode)
    {
        ModuleType = moduleType;
        Parent = parentModuleNode;
    }

    /// <summary>
    /// 模块类型.
    /// </summary>
    public Type ModuleType { get; set; } = null!;

    /// <summary>
    /// 链表，指向父模块节点，用于循环引用检测.
    /// </summary>
    public ModuleNode? Parent { get; set; }

    /// <summary>
    /// 依赖的其它模块.
    /// </summary>
    public HashSet<ModuleNode> Childs { get; private set; } = new();

    /// <summary>
    /// 向上搜索，当前模块的上层是否已存在此模块.
    /// </summary>
    /// <remarks>如果存在此模块，说明是循环引用.</remarks>
    /// <param name="childModule"></param>
    /// <returns>搜索结果.</returns>
    public bool ContainsTree(ModuleNode childModule)
    {
        if (childModule.ModuleType == ModuleType)
        {
            return true;
        }

        if (this.Parent == null)
        {
            return false;
        }

        // 如果当前模块找不到记录，则继续从父模块向上查找
        return this.Parent.ContainsTree(childModule);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return ModuleType.GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is ModuleNode module)
        {
            return GetHashCode() == module.GetHashCode();
        }

        return false;
    }
}
