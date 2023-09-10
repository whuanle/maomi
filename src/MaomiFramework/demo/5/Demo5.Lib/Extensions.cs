using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo5.Lib
{
    public class Test { }
    public static class Extensions
    {
        public static void AddLib(this IServiceCollection services)
        {
            services.AddI18nResource(options =>
            {
                options.AddJson<Test>("i18n");
            });
        }
    }
}
