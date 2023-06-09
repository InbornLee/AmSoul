using AmSoul.Core;
using AmSoul.MongoDB;
using AmSoul.SQL;
using AmSoul.Identity.MongoDB;
using Panda.DynamicWebApi;
using Sample.Models;

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
