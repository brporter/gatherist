namespace api.Models;

public record Tenant(long Id, Guid Key, string Name, bool Enabled, DateTime Created, DateTime Modified);