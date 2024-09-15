// <copyright file="MaomiSwaggerSchemaFilter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Maomi.Web.Core;

/// <summary>
/// Swagger 模型类过滤器.
/// </summary>
public class MaomiSwaggerSchemaFilter : ISchemaFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // 模型类的类型
        var type = context.Type;

        // 如果 API 参数不是对象
        if (type.IsPrimitive || TypeInfo.GetTypeCode(type) != TypeCode.Object)
        {
            return;
        }

        // 如果 API 参数是对象类型

        // 获取类型的所有属性
        PropertyInfo[] ps = context.Type.GetProperties();

        // 获取 swagger 文件显示的所有属性
        // 注意文档属性是已经已经生成的，这里进行后期转换，替换为需要显示的类型
        foreach (var property in schema.Properties)
        {
            var p = ps.FirstOrDefault(x => x.Name.ToLower() == property.Key.ToLower());
            if (p == null)
            {
                continue;
            }

            var t = property.Value.Type;
            var converter = p.GetCustomAttribute<JsonConverterAttribute>();
            if (converter == null || converter.ConverterType == null)
            {
                continue;
            }

            var targetType = TypeInfo.GetTypeCode(converter.ConverterType);

            // 如果是基元类型或 Decimal、DateTime
            if (targetType != TypeCode.Empty &&
                targetType != TypeCode.DBNull &&
                targetType != TypeCode.Object)
            {
                if (GetValueType(targetType, out var valueType))
                {
                    property.Value.Type = valueType;
                }
            }
        }

        static bool GetValueType(TypeCode targetType, out string? valueType)
        {
            valueType = null;
            switch (targetType)
            {
                case TypeCode.Boolean: valueType = "boolean"; break;
                case TypeCode.Char: valueType = "string"; break;
                case TypeCode.SByte: valueType = "integer"; break;
                case TypeCode.Byte: valueType = "integer"; break;
                case TypeCode.Int16: valueType = "integer"; break;
                case TypeCode.UInt16: valueType = "integer"; break;
                case TypeCode.Int32: valueType = "integer"; break;
                case TypeCode.UInt32: valueType = "integer"; break;
                case TypeCode.Int64: valueType = "integer"; break;
                case TypeCode.UInt64: valueType = "integer"; break;
                case TypeCode.Single: valueType = "number"; break;
                case TypeCode.Double: valueType = "number"; break;
                case TypeCode.Decimal: valueType = "number"; break;
                case TypeCode.DateTime: valueType = "string"; break;
                case TypeCode.String: valueType = "string"; break;

                // 一般不需要处理对象
                // case TypeCode.Object: valueType = p.PropertyType.Name; break;
                default: return false;
            }

            return true;
        }
    }
}