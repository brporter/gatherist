namespace gatherist.web.Services;

using Dapper;
using gatherist.web.Models;

public interface ITenantRepository
{
    Task<Tenant?> GetTenantAsync(Guid key, CancellationToken token = default);
    Task<Tenant?> GetTenantAsync(int id, CancellationToken token = default);
}

public class TenantRepository(IDataConnectionFactory factory) 
    : ITenantRepository
{
    private const string ConnectionName = "gatherist";
    private readonly IDataConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    
    private async Task<Tenant?> GetTenantInternalAsync<T>(T key, string sql, CancellationToken token) 
        where T : struct
    {
        await using var connection = _factory.CreateConnection(ConnectionName);
        await connection.OpenAsync(token);
        
        var tenant = await connection.QueryFirstOrDefaultAsync<Tenant>(
            sql,
            new { Key = key });

        return tenant;
    }

    public Task<Tenant?> GetTenantAsync(Guid key, CancellationToken token = default)
    {
        return GetTenantInternalAsync(key, SqlStatements.GetTenantByKey, token);
    }

    public Task<Tenant?> GetTenantAsync(int id, CancellationToken token = default)
    {
        return GetTenantInternalAsync(id, SqlStatements.GetTenantById, token);
    }
}