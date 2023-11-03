namespace Demo8.Mapper;


// 利用泛型缓存，提升访问速度，以及简化缓存结构
public static class TypeMembers<TTarget>
        where TTarget : class
{
    private static readonly MemberInfo[] MemberInfos;

    public static MemberInfo[] Members => MemberInfos;
    static TypeMembers()
    {
        MemberInfos = typeof(TTarget)
        .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(x => (x is FieldInfo) || (x is PropertyInfo)).ToArray();
    }
}

public class MapperBuilder<TSource, TTarget>
        where TSource : class
        where TTarget : class, new()
{
    // 生成的值映射委托
    private Delegate? MapDelegate;

    // 缓存用户自定义映射委托
    private readonly Dictionary<MemberInfo, Delegate> MapExpressions = new();

    public MapperBuilder<TSource, TTarget> Map<TValue, TField>(Func<TSource, TValue> valueFunc,
    Expression<Func<TTarget, TField>> field)
    {
        MemberInfo p = GetMember(field);
        MapExpressions[p] = valueFunc;
        return this;
    }

    // 从表达式中识别出对象的成员名称。
    // 如 (a => a.Value) ，解析出 Value
    private MemberInfo GetMember<TField>(Expression<Func<TTarget, TField>> field)
    {
        var body = field.Body;

        string name = "";

        // 提取 (a=> a.Value)
        if (body is MemberExpression memberExpression)
        {
            MemberInfo member = memberExpression.Member;
            name = member.Name;
        }
        // 提取 (a=> a.Value)
        else if (body is ParameterExpression parameterExpression)
        {
            name = parameterExpression.Name ?? "-";
        }
        // 提取 (a=> "Value") 字符串表达式
        else if (body is ConstantExpression constantExpression)
        {
            name = constantExpression.Value?.ToString() ?? "-";
        }
        else
        {
            throw new KeyNotFoundException($"{typeof(TTarget).Name} 中不存在名为 {body.ToString()} 的字段或属性，请检查表达式！");
        }

        var p = TypeMembers<TTarget>.Members.FirstOrDefault(x => x.Name == name);
        if (p == null)
        {
            throw new KeyNotFoundException($"{typeof(TTarget).Name} 中不存在名为 {body.ToString()} 的字段或属性，请检查表达式！");
        }

        return p;
    }


    public void Build()
    {
        List<Expression> exList = new List<Expression>();

        // TSource a;
        // TTarget b;
        ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "_a");
        ParameterExpression targetParameter = Expression.Parameter(typeof(TTarget), "_b");

        foreach (var item in TypeMembers<TTarget>.Members)
        {
            if (MapExpressions.TryGetValue(item, out var @delegate))
            {
                exList.Add(BuildAssign(sourceParameter, targetParameter, item, @delegate));
            }
            if (item is FieldInfo field)
            {
                // 忽略属性的私有字段
                if (item.Name.EndsWith(">k__BackingField")) continue;

                Expression assignDel = MapFieldOrProperty(sourceParameter, targetParameter, field);
                exList.Add(assignDel);
            }
            else if (item is PropertyInfo property)
            {
                if (!property.CanWrite) continue;
                Expression assignDel = MapFieldOrProperty(sourceParameter, targetParameter, property);
                exList.Add(assignDel);
            }
        }

        var block = Expression.Block(exList);
        var del = Expression.Lambda(block, sourceParameter, targetParameter).Compile();
        MapDelegate = del;
    }


    internal Expression BuildAssign(ParameterExpression sourceParameter, ParameterExpression targetParameter, MemberInfo memberInfo, Delegate @delegate)
    {
        // TSource a;
        // TTarget b;
        ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "a");
        ParameterExpression targetParameter = Expression.Parameter(typeof(TTarget), "b");

        // b.Value
        MemberExpression targetMember;
        if (memberInfo is FieldInfo field)
        {
            targetMember = Expression.Field(targetParameter, field);
        }
        else if (memberInfo is PropertyInfo property)
        {
            targetMember = Expression.Property(targetParameter, property);
        }
        else
        {
            throw new InvalidCastException($"{memberInfo.DeclaringType?.Name}.{memberInfo.Name} 不是字段或属性");
        }

        // 调用用户自定义委托
        var instance = Expression.Constant(@delegate.Target);
        MethodCallExpression delegateCall = Expression.Call(instance, @delegate.Method, sourceParameter);
        // b.Value = @delegate.DynamicInvoke(a);
        BinaryExpression assign = Expression.Assign(targetMember, delegateCall);
        return assign;
    }

    internal Expression MapFieldOrProperty(ParameterExpression sourceParameter, ParameterExpression targetParameter, MemberInfo targetField)
    {
        // b.Value
        MemberExpression targetMember;
        Type targetFieldType;
        {
            if (targetField is FieldInfo fieldInfo)
            {
                targetFieldType = fieldInfo.FieldType;
                targetMember = Expression.Field(targetParameter, fieldInfo);
            }
            else if (targetField is PropertyInfo propertyInfo)
            {
                targetFieldType = propertyInfo.PropertyType;
                targetMember = Expression.Property(targetParameter, propertyInfo);
            }
            else
            {
                throw new InvalidCastException(
                    $"框架处理出错，请提交 Issue！ {typeof(TTarget).Name}.{targetField.Name} 既不是字段也不是属性");
            }
        }

        var sourceField = typeof(TSource).GetMember(targetField.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();

        // 在 TSource 中搜索不到对应字段时，b.Value 使用默认值
        if (sourceField == null)
        {
            // 生成表达式 b.Value = default;
            return Expression.Assign(targetMember, Expression.Default(targetFieldType));
        }

        MemberExpression sourceMember;
        Type sourceFieldType;
        {
            if (sourceField is FieldInfo fieldInfo)
            {
                sourceFieldType = fieldInfo.FieldType;
                sourceMember = Expression.Field(sourceParameter, fieldInfo);
            }
            else if (sourceField is PropertyInfo propertyInfo)
            {
                sourceFieldType = propertyInfo.PropertyType;
                sourceMember = Expression.Property(sourceParameter, propertyInfo);
            }
            else
            {
                throw new InvalidCastException(
                    $"框架处理出错，请提交 Issue！ {typeof(TSource).Name}.{sourceField.Name} 既不是字段也不是属性");
            }
        }

        if (targetFieldType != sourceFieldType)
            throw new InvalidCastException(
                        $"类型不一致！ {typeof(TSource).Name}.{sourceField.Name} 与 {typeof(TTarget).Name}.{targetField.Name}");

        // 生成表达式 b.Value = a.Value
        return Expression.Assign(targetMember, sourceMember);
    }

    // 映射对象
    public TTarget Map<TSource>(TSource source)
    {
        TTarget target = new TTarget();
        return (TTarget)MapDelegate.DynamicInvoke(source, target);
    }
}