using System.Data.Common;
using api.Models;

namespace api.Services;

public interface IRepository
{
    public Result<(Tenant, User), RecordError> LookupUser(string emailAddress);
}

public interface IDbConnectionConfiguration
{
    public string ConnectionString { get; }
}

public class DbConnectionConfiguration
    : IDbConnectionConfiguration
{
    public string ConnectionString { get; init; }
}

public interface IDbConnectionFactory
{
    public Task<Result<DbConnection, Exception>> GetConnectionAsync();
}

public class DbConnectionFactory
    : IDbConnectionFactory
{
    private readonly DbProviderFactory _factory;
    private readonly IDbConnectionConfiguration _configuration;

    public DbConnectionFactory(DbProviderFactory providerFactory, IDbConnectionConfiguration connectionConfiguration)
    {
        _factory = providerFactory;
        _configuration = connectionConfiguration;
    }

    public async Task<Result<DbConnection, Exception>> GetConnectionAsync()
    {
        var connection = _factory.CreateConnection();
        connection.ConnectionString = _configuration.ConnectionString;

        try
        {
            await connection.OpenAsync();

            return connection;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}

public class Repository
    : IRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public Repository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }
    
    public async Task<Result<(Tenant, User), RecordError>> LookupUserAsync(string emailAddress)
    {
        // var result = await _connectionFactory.GetConnectionAsync();
        //
        // return result.Match(
        //     async (connection) =>
        //     {
        //         try
        //         {
        //             connection.ExecuteReaderAsync();
        //             var cmd = connection.CreateCommand();
        //         }
        //         finally
        //         {
        //             await connection.DisposeAsync();
        //         }
        //     },
        //     (error) =>
        //     {
        //         return RecordError.ConnectionFailed;
        //     }
        // );

        return (
            new Tenant(1, Guid.NewGuid(), string.Empty, true, DateTime.UtcNow, DateTime.UtcNow),
            new User(1, Guid.NewGuid(), string.Empty, 1, true, DateTime.UtcNow, DateTime.UtcNow)
        );
    }

    public Result<(Tenant, User), RecordError> LookupUser(string emailAddress)
    {
        throw new NotImplementedException();
    }
}