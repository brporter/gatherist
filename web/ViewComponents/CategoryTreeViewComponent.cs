using Microsoft.AspNetCore.Mvc;
using TheGatherist.Web.Services;
using TheGatherist.Web.Pages.Shared.Components.CategoryTree;

namespace TheGatherist.Web.ViewComponents;

public class CategoryTreeViewComponent(ICategoryRepository repository)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(int? categoryId)
    {
        var (root, categories) = await repository.GetCategoriesAsync(1, categoryId);

        return View(new CategoryTreeModel()
        {
            Root = root,
            Categories = categories
        });
    }
}