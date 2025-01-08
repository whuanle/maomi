using Maomi.Web.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Demo9.MaomiSwagger
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			// 1，这里注入
			builder.Services.AddMaomiSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				// 2，这里配置中间件
				app.UseMaomiSwagger(setupAction: setup =>
				{
					setup.PreSerializeFilters.Add((swagger, httpReq) =>
					{
						swagger.Servers = new List<OpenApiServer>
						{
							new  (){ Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/mya" },
							new  (){ Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/myb" }
						};
					});
				});
			}

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
