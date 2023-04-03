# AmSoul
.Net7 Dynamic Restful Webapi

[![license](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)

## 简介

[AmSoul](https://github.com/InbornLee/AmSoul) 是一个基于.Net7的动态WebApi模版，它使用了Panda.DynamicWebApi，内置多数据库配置，有较高的代码规范，使用方式简单方面，基于JWT实现动态权限路由，开箱即用的后台解决方案，也可用于学习参考。

```CSharp
using AmSoul.Core.Extensions;
using AmSoul.Core.Models;
using AmSoul.Identity.MongoDB.Extensions;
using AmSoul.Identity.MongoDB.Models;
using Panda.DynamicWebApi;
using Sample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddMongoDb<MongoDbDatabaseSetting2>(builder.Configuration);

builder.Services.AddMongoDbIdentityStores<BaseUser, BaseRole>(
    identityOptions =>
    {
        identityOptions.Password.RequireDigit = false;
        identityOptions.Password.RequireNonAlphanumeric = false;
        identityOptions.Password.RequiredLength = 6;
        identityOptions.Password.RequireLowercase = false;
        identityOptions.Password.RequireUppercase = false;
    },
    builder.Configuration.GetSection(nameof(MongoDbDatabaseSetting)).Get<MongoDbDatabaseSetting>()
);

builder.Services.AddDynamicWebApi();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddJwtSwaggerGen("动态 Webapi", "v1", "Webapi 测试");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
```