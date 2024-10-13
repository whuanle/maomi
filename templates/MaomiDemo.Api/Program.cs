using Maomi;
using Maomi.I18n;
using Maomi.Web.Core;

namespace MaomiDemo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddI18nAspNetCore("i18n");

        // 注册模块化服务，并设置 ApiModule 为入口
        builder.Services.AddModule<ApiModule>();
        //  swagger 服务
        builder.Services.AddMaomiSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            // swagger 中间件
            app.UseMaomiSwagger();
        }

        app.UseI18n();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}