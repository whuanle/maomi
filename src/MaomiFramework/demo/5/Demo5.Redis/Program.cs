using FreeRedis;
using Maomi.I18n;
using Maomi.I18n.Redis;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;


public class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        RedisClient cli = new RedisClient("127.0.0.1:6379,defaultDatabase=0");
        builder.Services.AddI18n(defaultLanguage: "zh-CN");
        builder.Services.AddI18nResource(option =>
        {
            option.AddRedis(cli, "demo5.redis", TimeSpan.FromMinutes(10), 10);
            option.AddJson<Program>("i18n");
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseI18n();
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            var localizer = context.RequestServices.GetRequiredService<IStringLocalizer>();
            await context.Response.WriteAsync(localizer["���ﳵ:��Ʒ����"]);
            return;
        });


        app.Run();
    }
}