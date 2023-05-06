using AmSoul.Core.Extensions;
using AmSoul.Core.Models;
using AmSoul.Extension.Sql.Extensions;
using AmSoul.Identity.MongoDB.Extensions;
using AmSoul.Identity.MongoDB.Models;
using Microsoft.OpenApi.Models;
using Panda.DynamicWebApi;
using Sample.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddMongoDb<MongoDbDatabaseSetting2>(builder.Configuration);

builder.Services.AddMySql(builder.Configuration);
builder.Services.AddOracle(builder.Configuration);


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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDynamicWebApi();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddJwtSwaggerGen("¶¯Ì¬ Webapi", "v1", "Webapi ²âÊÔ");
builder.Services.AddSwaggerGen(options =>
{
    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var baseXmlPath = Path.Combine(baseDirectory, "AmSoul.Extension.Sql.xml");

    options.IncludeXmlComments(baseXmlPath);
});

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
