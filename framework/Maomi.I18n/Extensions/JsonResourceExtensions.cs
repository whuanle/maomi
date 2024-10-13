// <copyright file="JsonResourceExtensions.cs" company="Maomi">
// Copyright (c) Maomi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Github link: https://github.com/whuanle/maomi
// </copyright>

using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Maomi.I18n;

/// <summary>
/// Json 多语言文件资源.
/// </summary>
public static class JsonResourceExtensions
{
    /// <summary>
    /// 扫描目录下的所有子目录，自动区配对应的项目/程序集下，json 文件名称会被动作语言名称.
    /// </summary>
    /// <param name="resourceFactory"></param>
    /// <param name="basePath"></param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory ParseDirectory(
        this I18nResourceFactory resourceFactory,
        string basePath)
    {
        var basePathDirectoryInfo = new DirectoryInfo(basePath);
        Queue<DirectoryInfo> directoryInfos = new Queue<DirectoryInfo>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var basePathFullName = basePathDirectoryInfo.FullName;

        directoryInfos.Enqueue(basePathDirectoryInfo);

        while (directoryInfos.Count > 0)
        {
            var curDirectory = directoryInfos.Dequeue();
            var lanDir = curDirectory.GetDirectories();
            foreach (var lan in lanDir)
            {
                directoryInfos.Enqueue(lan);
            }

            var files = curDirectory.GetFiles().Where(x => x.Name.EndsWith(".json")).ToArray();
            if (files.Length == 0)
            {
                continue;
            }

            // 移除路径的前部分
            var curPath = curDirectory.FullName[basePathFullName.Length..].Trim('/', '\\');

            var assembly = assemblies.FirstOrDefault(x => string.Equals(curPath, x.GetName().Name, StringComparison.CurrentCultureIgnoreCase));
            if (assembly == null)
            {
                continue;
            }

            foreach (var file in files)
            {
                var language = Path.GetFileNameWithoutExtension(file.Name);
                var text = File.ReadAllText(file.FullName);
                var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });

                DictionaryResource jsonResource = (Activator.CreateInstance(
                    typeof(DictionaryResource<>).MakeGenericType(assembly.GetTypes()[0]),
                    new object[] { new CultureInfo(language), dic, assembly }) as DictionaryResource)!;
                resourceFactory.Add(jsonResource);
            }
        }

        return resourceFactory;
    }

    /// <summary>
    /// 添加 json 文件资源，json 文件名称会被当作语言名称.
    /// </summary>
    /// <param name="resourceFactory"></param>
    /// <param name="basePath">基础路径.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory AddJsonDirectory(
        this I18nResourceFactory resourceFactory,
        string basePath)
    {
        var rootDir = new DirectoryInfo(basePath);

        var files = rootDir.GetFiles().Where(x => x.Name.EndsWith(".json"));
        foreach (var file in files)
        {
            var language = Path.GetFileNameWithoutExtension(file.Name);
            var text = File.ReadAllText(file.FullName);
            var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });

            DictionaryResource jsonResource = new DictionaryResource(new CultureInfo(language), dic);
            resourceFactory.Add(jsonResource);
        }

        return resourceFactory;
    }

    /// <summary>
    /// 添加 json 文件资源，将目录下的所有 json 文件都归类到此程序集下，json 文件名称会被当作语言名称.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="resourceFactory"></param>
    /// <param name="basePath">基础路径.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory AddJsonDirectory<T>(
        this I18nResourceFactory resourceFactory,
        string basePath)
        where T : class
    {
        var rootDir = new DirectoryInfo(basePath);

        var files = rootDir.GetFiles().Where(x => x.Name.EndsWith(".json"));
        foreach (var file in files)
        {
            var language = Path.GetFileNameWithoutExtension(file.Name);
            var text = File.ReadAllText(file.FullName);
            var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });

            DictionaryResource<T> jsonResource = new DictionaryResource<T>(new CultureInfo(language), dic, typeof(T).Assembly);
            resourceFactory.Add(jsonResource);
        }

        return resourceFactory;
    }

    /// <summary>
    /// 添加 json 文件资源.
    /// </summary>
    /// <param name="resourceFactory"></param>
    /// <param name="language">语言.</param>
    /// <param name="jsonFile">json 文件路径.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory AddJsonFile(this I18nResourceFactory resourceFactory, string language, string jsonFile)
    {
        string s = File.ReadAllText(jsonFile);
        Dictionary<string, object> kvs = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(s)), new JsonReaderOptions
        {
            AllowTrailingCommas = true
        });

        DictionaryResource resource = new DictionaryResource(new CultureInfo(language), kvs);
        resourceFactory.Add(resource);
        return resourceFactory;
    }

    /// <summary>
    /// 添加 json 文件资源.
    /// </summary>
    /// <typeparam name="T">类型.</typeparam>
    /// <param name="resourceFactory"></param>
    /// <param name="language">语言.</param>
    /// <param name="jsonFile">json 文件路径.</param>
    /// <returns><see cref="I18nResourceFactory"/>.</returns>
    public static I18nResourceFactory AddJsonFile<T>(this I18nResourceFactory resourceFactory, string language, string jsonFile)
        where T : class
    {
        string s = File.ReadAllText(jsonFile);
        Dictionary<string, object> kvs = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(s)), new JsonReaderOptions
        {
            AllowTrailingCommas = true
        });

        DictionaryResource<T> resource = new DictionaryResource<T>(new CultureInfo(language), kvs, typeof(T).Assembly);
        resourceFactory.Add(resource);
        return resourceFactory;
    }
}
