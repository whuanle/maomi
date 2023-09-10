using Demo9.ApiDataAnnotations;
using Maomi;
using Maomi.I18n;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
// ע��ģ�黯���񣬲����� ApiModule Ϊ���
builder.Services.AddModule<ApiModule>();

var app = builder.Build();

app.UseI18n();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
