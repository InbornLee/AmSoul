# AmSoul
.Net7 Dynamic Restful Webapi

[![license](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)

## ���

[AmSoul](https://github.com/InbornLee/AmSoul) ��һ������.Net7�Ķ�̬WebApiģ�棬��ʹ����Panda.DynamicWebApi������֧�ֶ������ݿ����ã��нϸߵĴ���淶��ʹ�÷�ʽ�򵥷��㣬����JWTʵ�ֶ�̬Ȩ��·�ɣ����伴�õĺ�̨���������Ҳ������ѧϰ�ο���

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
builder.Services.AddJwtSwaggerGen("��̬ Webapi", "v1", "Webapi ����");
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

## ��װʹ��

- 环境配置
  本地环境需要安装.Net7 SDK 和 Git

- ��¡����

```bash
git clone https://github.com/InbornLee/AmSoul.git
```
