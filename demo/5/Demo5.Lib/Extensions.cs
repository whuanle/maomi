using Maomi.I18n;
using Microsoft.Extensions.DependencyInjection;

namespace Demo5.Lib;

public class Test { }
public static class Extensions
{
    public static void AddLib(this IServiceCollection services)
    {
        services.AddI18nResource(options =>
        {
            options.ParseDirectory("i18n");
        });
    }
}
