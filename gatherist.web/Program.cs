using System.Data.Common;
using System.Text.Json.Serialization;
using gatherist.web.Models;
using gatherist.web.Services;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateSlimBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<IDataConnectionFactory>(new DataConnectionFactory(SqlClientFactory.Instance, builder.Configuration));
builder.Services.AddSingleton<ITenantRepository, TenantRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
