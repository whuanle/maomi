using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Maomi.I18n
{
    public static partial class DataAnnotationsExtensions
    {
        /// <summary>
        /// 为 API 模型验证注入 i18n 服务
        /// </summary>
        /// <param name="builder"></param>
        public static IMvcBuilder AddI18nDataAnnotation(this IMvcBuilder builder)
        {
            builder.Services.AddTransient<IStringLocalizerFactory, I18nStringLocalizerFactory>();
            builder
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (modelType, stringLocalizerFactory) =>
                    stringLocalizerFactory.Create(modelType);
                });
            return builder;
        }
    }
}
