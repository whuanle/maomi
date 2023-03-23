using System.Reflection.Emit;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Linq;
using ConsoleApp3;
using System.Threading;

public class Program
{
    static void Main()
    {
        AopHelper.Build();
        var assembly = Assembly.LoadFrom("AopTmp.dll");
        var newType = assembly.GetType("Aop.UserServiceAop");
        var o = (IUserService)Activator.CreateInstance(newType);
        o.Login("工良", "123456");
    }

    public interface IUserService
    {
        bool Login(string name, string passeword);
    }

    public class UserService : IUserService
    {
        public bool Login(string name, string passeword)
        {
            return true;
        }
    }

    public class UserServiceAop : IUserService
    {
        private readonly UserService _service;
        public UserServiceAop()
        {
            _service = new UserService();
        }
        public bool Login(string name, string passeword)
        {
            Console.WriteLine($"用户开始登录：{name}");
            var result = _service.Login(name, passeword);
            Console.WriteLine($"用户 {name} 登录结果: {result}");
            return result;
        }
    }

    public static class AopHelper
    {
        public static void Build()
        {
            // .NET Framework 跟 .NET Core 有区别，如果想保存程序集到文件，需要使用 AppDomain 创建程序集；
            // 构建运行时程序集
            AppDomain myDomain = AppDomain.CurrentDomain;
            AssemblyName assemblyName = new AssemblyName("AopTmp");
            assemblyName.SetPublicKeyToken(new Guid().ToByteArray());
            AssemblyBuilder assBuilder = myDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);

            // 构建模块
            ModuleBuilder moduleBuilder = assBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");
            //// 构建类型，命名空间+类名
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Aop.UserServiceAop",
                TypeAttributes.Public, parent: null, interfaces: typeof(UserService).GetInterfaces());
            // 构建字段
            // field private initonly class Program/UserService _service
            var fieldBuilder = typeBuilder.DefineField("_service", typeof(UserService), FieldAttributes.Private | FieldAttributes.InitOnly);
            
            BuildCtor(typeBuilder, fieldBuilder);
            BuildMethod(typeBuilder, fieldBuilder);
            var type = typeBuilder.CreateType();
            assBuilder.Save(assemblyName.Name+".dll");
        }

        private static void BuildCtor(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            // 构造函数
            // .method public hidebysig specialname rtspecialname 
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);
            var il = ctorBuilder.GetILGenerator();
            //  _service = new UserService();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, typeof(UserService).GetConstructors()[0]);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);
        }

        private static void BuildMethod(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var baseMethod = typeof(UserService).GetMethod("Login");
            // .method public final hidebysig newslot virtual 
            var methodBuilder = typeBuilder.DefineMethod(baseMethod.Name,
                baseMethod.Attributes,
                baseMethod.CallingConvention,
                // 返回值和参数
                baseMethod.ReturnType, baseMethod.GetParameters().Select(x => x.ParameterType).ToArray());
            var sType = typeof(DefaultInterpolatedStringHandler);

            ILGenerator il = methodBuilder.GetILGenerator();

            // 定义本地变量
            il.DeclareLocal(typeof(bool));
            il.DeclareLocal(sType);

            // Console.WriteLine("用户开始登录：" + name);
            il.Emit(OpCodes.Ldstr, "用户开始登录: ");
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(String).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
            il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));

            // bool result = _service.Login(name, passeword);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Callvirt, baseMethod);
            il.Emit(OpCodes.Stloc_0);

            // Console.WriteLine($"用户 {name} 登录结果 {result}");
            // DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Ldc_I4_S, 9);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Call, sType.GetConstructor(new Type[] { typeof(int), typeof(int) }));
            // defaultInterpolatedStringHandler.AppendLiteral("用户 ");
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Ldstr, "用户: ");
            il.Emit(OpCodes.Call, sType.GetMethod("AppendLiteral", new Type[] { typeof(string) }));
            // defaultInterpolatedStringHandler.AppendFormatted(name);
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, sType.GetMethod("AppendFormatted", new Type[] { typeof(string) }));
            // defaultInterpolatedStringHandler.AppendLiteral(" 登录结果 ");
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Ldstr, "登录结果: ");
            il.Emit(OpCodes.Call, sType.GetMethod("AppendLiteral", new Type[] { typeof(string) }));
            // defaultInterpolatedStringHandler.AppendFormatted(result); AppendFormatted<bool>(result)
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, sType.GetMethods()
                .FirstOrDefault(x => x.Name == "AppendFormatted" && x.IsGenericMethod && x.GetParameters().Length == 1).MakeGenericMethod(typeof(bool)));
            // Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Call, sType.GetMethod("ToStringAndClear", Type.EmptyTypes));
            il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            // return result;
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }
    }
}

namespace ConsoleApp3
{
    public class DefaultInterpolatedStringHandler
    {
        public DefaultInterpolatedStringHandler(int a, int b) { }
        public void AppendLiteral(string value) { }
        public void AppendFormatted(string value) { }
        public void AppendFormatted<T>(T value) { }
        public string ToStringAndClear() { return "1"; }
    }
}
