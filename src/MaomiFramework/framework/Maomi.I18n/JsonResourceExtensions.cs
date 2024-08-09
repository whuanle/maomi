using System.Buffers;
using System.Text.Json;
using System.Text;

namespace Maomi.I18n
{
    public static class JsonResourceExtensions
    {
        /// <summary>
        /// 添加 json 文件资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceFactory"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static I18nResourceFactory AddJson<T>(this I18nResourceFactory resourceFactory,
            string basePath)
            where T : class
        {
            var dirName = typeof(T).Assembly.GetName().Name;

            // 非递归法遍历所有目录，读取 json 文件，生成语言支持

            var rootDir = new DirectoryInfo(Path.Combine(Directory.GetParent(typeof(T).Assembly.Location).FullName, basePath));
            var lanDir = rootDir.GetDirectories().FirstOrDefault(x => x.Name == dirName);

            ArgumentNullException.ThrowIfNull(lanDir);

            var files = lanDir.GetFiles().Where(x => x.Name.EndsWith(".json"));
            foreach (var file in files)
            {
                var language = Path.GetFileNameWithoutExtension(file.Name);
                var text = File.ReadAllText(file.FullName);
                var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });

                JsonResource<T> jsonResource = new JsonResource<T>(language, dic);
                resourceFactory.Add(jsonResource);
            }

            return resourceFactory;
        }

        /// <summary>
        /// 添加 json 文件资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="language">语言</param>
        /// <param name="resourceFactory"></param>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static I18nResourceFactory AddJson<T>(this I18nResourceFactory resourceFactory,
            string language,
            string jsonFile)
            where T : class
        {
            var text = File.ReadAllText(jsonFile);
            var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(text)), new JsonReaderOptions { AllowTrailingCommas = true });

            JsonResource<T> jsonResource = new JsonResource<T>(language, dic);
            resourceFactory.Add(jsonResource);
            return resourceFactory;
        }
    }
}
