using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Module
{
    /// <summary>
    /// 模块注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class InjectModuleAttribute : Attribute
    {
        /// <summary>
        /// 依赖的模块
        /// </summary>
        public Type ModuleType { get; private init; }

        /// <summary>
        /// 注入需要使用的模块
        /// </summary>
        /// <param name="type"></param>
        public InjectModuleAttribute(Type type)
        {
            ModuleType = type;
        }
    }

    /// <summary>
    /// 模块注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InjectModuleAttribute<TModule> : InjectModuleAttribute
        where TModule : IModule
    {

        public InjectModuleAttribute() : base(typeof(TModule))
        {
        }
    }
}
