using Dapper;
using gatherist.web.Models;
using Microsoft.Identity.Client;

namespace gatherist.web.Services;

public interface IUserRepository
{
    public Task<User?> GetUserAsync(int id, CancellationToken token = default);
}

public class UserRepository(IDataConnectionFactory factory)
    : IUserRepository
{
    private const string ConnectionName = "gatherist";
    private readonly IDataConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        
    public async Task<User?> GetUserAsync(int id, CancellationToken token = default)
    {
        await using var connection = _factory.CreateConnection(ConnectionName);
        await connection.OpenAsync(token);
        
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            SqlStatements.GetUserById,
            new { Key = id });

        return user;
    }
}