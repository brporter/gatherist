namespace gatherist.web.Models;


public record Tenant(
    int Id,
    Guid Key,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsEnabled);