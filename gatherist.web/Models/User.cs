namespace gatherist.web.Models;

public record User(int Id, Guid Key, string Email, int TenantId, DateTime CreatedAt, DateTime UpdatedAt, bool IsEnabled);