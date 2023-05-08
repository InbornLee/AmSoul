# AmSoul
.Net7 Dynamic Restful Webapi

[![license](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)

## 简介

[AmSoul](https://github.com/InbornLee/AmSoul) 是一个基于.Net7的动态WebApi模版，它使用了Panda.DynamicWebApi，内置支持多种数据库配置，有较高的代码规范，使用方式简单方便，基于JWT实现动态权限路由，开箱即用的后台解决方案，也可用于学习参考。

## 配置说明
### Program.cs
```CSharp
using AmSoul.Core;
using AmSoul.MongoDB;
using AmSoul.SQL;
using AmSoul.Identity.MongoDB;
using Panda.DynamicWebApi;
using Sample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
//注册MongoDB服务
builder.Services.AddMongoDb(builder.Configuration);
//注册额外的MongoDB服务
builder.Services.AddMongoDb<MongoDbDatabaseSetting2>(builder.Configuration);
//注册MySql服务
builder.Services.AddMySql(builder.Configuration);
//注册Oracle服务
builder.Services.AddOracle(builder.Configuration);
//注册Identity服务（MongoDB）
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
//注册动态WebApi服务
builder.Services.AddDynamicWebApi();
//注册Jwt验证服务
builder.Services.AddJwtAuthentication(builder.Configuration);
//注册Swagger(带Jwt验证)
builder.Services.AddJwtSwaggerGen("动态 Webapi", "v1", "Webapi 测试");
//注册跨域
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

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
```
### appsettings.json
```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  //MongoDB数据源配置
  "MongoDbDatabaseSetting": {
    "Server": "xxx.xxx.xxx.xxx",
    "Port": 27017,
    "DatabaseName": "XXXXX",
    "Username": "XXX",
    "Password": "XXXXXX"
  },
  //MongoDB2数据源配置
  "MongoDbDatabaseSetting2": {
    "Server": "localhost",
    "Port": 27018,
    "DatabaseName": "XXXXX",
    "Username": "XXX",
    "Password": "XXXXXX"
  },
  //MySql数据源配置
  "MySqlDatabaseSetting": {
    "Server": "localhost",
    "Port": 3306,
    "DatabaseName": "XXXXX",
    "Username": "XXX",
    "Password": "XXXXXX"
  },
  //Oracle数据源配置
  "OracleDatabaseSetting": {
    "Server": "localhost",
    "Port": 1521,
    "DatabaseName": "XXXXX",
    "Username": "XXX",
    "Password": "XXXXXX"
  },
  "JwtTokenOptions": {
    "Audience": "http://localhost",
    "Issuer": "http://localhost",
    "SecurityKey": "XXX"
  }
}
```
## 安装使用

- 环境配置
  本地环境需要安装.Net7 SDK 和 Git

- 克隆代码

```bash
git clone https://github.com/InbornLee/AmSoul.git
```
