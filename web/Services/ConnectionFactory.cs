using System.Data;
using System.Data.Common;

namespace TheGatherist.Web.Services;

public enum ResultStatus
 {
     Ok,
     Error
 }
 
 public class Result<TValue, TError>
 {
     private readonly TValue? _value;
     private readonly TError? _error;
 
     private Result(TValue value)
     {
         _value = value ?? throw new ArgumentException(nameof(value));
     }
 
     private Result(TError error)
     {
         _error = error ?? throw new ArgumentException(nameof(error));
     }
 
     public bool IsOk => _value != null && _error == null;
 
     public bool IsError => _error != null && _value == null;
 
     public TValue Ok()
     {
         if (_value is not null)
             return _value;
 
         if (_error is Exception except)
         {
             throw except;
         }
 
         throw new InvalidOperationException();
     }
 
     public TError Error() => _error ?? throw new InvalidOperationException();
 
     public static implicit operator TValue(Result<TValue, TError> r) => r.Ok();
 
     public static implicit operator TError(Result<TValue, TError> r) => r.Error();
 
     public static implicit operator Result<TValue, TError>(TValue value) => new(value);
     
     public static implicit operator Result<TValue, TError>(TError error) => new(error);
 
     public static implicit operator ResultStatus(Result<TValue, TError> result)
     {
         if (result.IsError) return ResultStatus.Error;
         if (result.IsOk) return ResultStatus.Ok;
 
         throw new InvalidOperationException();
     }
 }
 
 public enum CommonError
 {
     None,
     ItemNotFound
 }

 public interface IConnectionStringManager
 {
     public Result<String, CommonError> GetConnectionString(string name);
 }

 public class ConnectionStringManager
    : IConnectionStringManager
{
    private readonly Dictionary<string, string> _connectionStrings = new();
    
    public ConnectionStringManager(IConfiguration configuration)
    {
        var connectionStrings = configuration?.GetSection("ConnectionStrings");

        if (connectionStrings == null) 
            return;
        
        foreach (var connectionString in connectionStrings.GetChildren())
        {
            var value = connectionString.Value;
            var key = connectionString.Key;

            if (value != null)
            {
                _connectionStrings.Add(key, value);
            }
        }
    }
    
    public Result<String, CommonError> GetConnectionString(string name)
    {
        if (_connectionStrings.TryGetValue(name, out var s))
            return s;

        return CommonError.ItemNotFound;
    }
}

public interface IConnectionFactory
{
    IDbConnection GetConnection(string name);
}

public class ConnectionFactory(DbProviderFactory factory, IConnectionStringManager connectionStringManager)
    : IConnectionFactory
{
    public IDbConnection GetConnection(string name)
    {
        var connectionString = connectionStringManager.GetConnectionString(name);
        
        if (CommonError.ItemNotFound == connectionString)
        {
            throw new ArgumentOutOfRangeException(nameof(name), name, "No such connection string could be found.");
        }
        
        var connection = factory.CreateConnection()!;
        connection.ConnectionString = connectionString;

        return connection;
    }
}