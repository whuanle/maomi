namespace Maomi.Module
{
    /// <summary>
    /// 模块节点
    /// </summary>
    internal class ModuleNode
    {
        /// <summary>
        /// 当前模块类型
        /// </summary>
        public Type ModuleType { get; set; } = null!;

        /// <summary>
        /// 链表，指向父模块节点，用于循环引用检测
        /// </summary>
        public ModuleNode? ParentModule { get; set; }

        /// <summary>
        /// 依赖的其它模块
        /// </summary>
        public HashSet<ModuleNode>? Childs { get; set; }

        /// <summary>
        /// 一直向父节点搜索，如果存在此模块，说明是循环引用
        /// </summary>
        /// <param name="childModule"></param>
        /// <returns></returns>
        public bool ContainsTree(ModuleNode childModule)
        {
            if (childModule.ModuleType == ModuleType) return true;
            if (this.ParentModule == null) return false;
            // 如果当前模块找不到记录，则向上查找
            return this.ParentModule.ContainsTree(childModule);
        }

        public override int GetHashCode()
        {
            return ModuleType.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is ModuleNode module)
            {
                return GetHashCode() == module.GetHashCode();
            }
            return false;
        }
    }
}
