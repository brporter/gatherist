namespace api.Models;

public record User(long Id, Guid Key, string Email, long TenantId, bool Enabled, DateTime Created, DateTime Modified);