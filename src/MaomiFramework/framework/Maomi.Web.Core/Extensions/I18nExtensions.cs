using Maomi.I18n;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;

namespace Maomi.Web.Core
{
    public static class I18nExtensions
    {
        private static readonly DataAnnotationsI18nStringLocalizer Localizar = new(typeof(I18nExtensions));
        public static void AddI18nDataAnnotation(this IMvcBuilder builder)
        {
            //builder.Services.Configure<ApiBehaviorOptions>(options =>
            //{
            //    // 这段代码只有在模型验证失败时执行
            //    options.InvalidModelStateResponseFactory = (context) =>
            //    {
            //        Dictionary<string, List<string>> errors = new();
            //        foreach (var item in context.ModelState)
            //        {
            //            List<string> list = new();
            //            foreach (var error in item.Value.Errors)
            //            {
            //                list.Add(error.ErrorMessage);
            //            }
            //            errors.Add(item.Key, list);
            //        }
            //        return new JsonResult(R.C(400, Localizar["404"], errors))
            //        {
            //            StatusCode = 400
            //        };
            //    };
            //});
            builder
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (modelType, stringLocalizerFactory) =>
                    new DataAnnotationsI18nStringLocalizer(modelType);
                });
        }

        internal class DataAnnotationsI18nStringLocalizer : IStringLocalizer
        {
            private readonly string _path;
            public DataAnnotationsI18nStringLocalizer(Type resourceType)
            {
                _path = resourceType.Assembly.GetName().Name;
            }
            public LocalizedString this[string name] => I18nStringLocalizerFactory.Get($"{_path}.{CultureInfo.CurrentCulture.Name}")[name];

            public LocalizedString this[string name, params object[] arguments] => I18nStringLocalizerFactory.Get($"{_path}.{CultureInfo.CurrentCulture.Name}")[name, arguments];

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => I18nStringLocalizerFactory.Get($"{_path}.{CultureInfo.CurrentCulture.Name}").GetAllStrings(includeParentCultures);
        }
    }
}
