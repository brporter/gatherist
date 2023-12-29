using TheGatherist.Web.Models;

namespace TheGatherist.Web.Pages.Shared.Components.CategoryTree;

public class CategoryTreeModel
{
    public Category? Root { get; set; }
    public IEnumerable<Category>? Categories { get; set; }
}