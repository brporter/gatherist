using System.Data.Common;

namespace gatherist.web.Services;

public interface IDataConnectionFactory
{
    DbConnection CreateConnection(string connectionName);
}

public class DataConnectionFactory(DbProviderFactory factory, IConfiguration configuration)
    : IDataConnectionFactory
{
    private readonly DbProviderFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public DbConnection CreateConnection(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentNullException(nameof(connectionName));
        
        var connectionString = _configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentOutOfRangeException(nameof(connectionName));

        var connection = _factory.CreateConnection()!;
        connection.ConnectionString = connectionString;

        return connection;
    }
}