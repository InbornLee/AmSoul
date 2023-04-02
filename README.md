# AmSoul
.Net7 Dynamic Restful Webapi

```C#
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
