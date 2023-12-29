namespace TheGatherist.Web.Models;

public record Category(int CategoryId, int TenantId, string Name, int? ParentCategoryId);