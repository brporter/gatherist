using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var sampleTodos = new Todo[]
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

var tenantApi = app.MapGroup("/tenants");
tenantApi.MapGet("/", async () => {
    using var connection = new Microsoft.Data.SqlClient.SqlConnection();
    connection.ConnectionString = "Data Source=localhost,1433;Initial Catalog=gatherist.db;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=true";

    await connection.OpenAsync();

    var cmd = connection.CreateCommand();
    cmd.CommandType = System.Data.CommandType.Text;
    cmd.CommandText = "SELECT * FROM [dbo].[tenant]";

    using var reader = await cmd.ExecuteReaderAsync();

    var results = new List<Tenant>();

    while (await reader.ReadAsync())
    {
        results.Add(
            new Tenant(reader.GetInt32(0), reader.GetGuid(1), reader.GetString(2), reader.GetDateTime(3), reader.GetDateTime(4), reader.GetBoolean(5))
        );
    }

    return results;
});

tenantApi.MapGet("/{id}", async (int id) => {
    using var connection = new Microsoft.Data.SqlClient.SqlConnection();
    connection.ConnectionString = "Data Source=localhost,1433;Initial Catalog=gatherist.db;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=true";

    await connection.OpenAsync();

    var cmd = connection.CreateCommand();
    cmd.CommandType = System.Data.CommandType.Text;
    cmd.CommandText = "SELECT * FROM [dbo].[user] WHERE [tenant_id] = @tenant_id";
    cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tenant_id", System.Data.SqlDbType.Int) { Value = id });

    using var reader = await cmd.ExecuteReaderAsync();
    var results = new List<User>();

    if (await reader.ReadAsync())
    {
        results.Add(new User(reader.GetInt32(0), reader.GetGuid(1), reader.GetString(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetDateTime(5), reader.GetBoolean(6)));
    }

    return results;
});

// Set up an API endpoint that queries the database running on localhost and exposes CRUD metods
// for the table tenant in the userdata schema. The query should used Dapper and be compatible with
// AOT.


app.Run();

public record User(int Id, Guid Key, string Email, int TenantId, DateTime CreatedOn, DateTime UpdateAt, bool IsEnabled);

public record Tenant(int Id, Guid Key, string Name, DateTime CreatedOn, DateTime UpdateAt, bool IsEnabled);

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}