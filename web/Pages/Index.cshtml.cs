using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TheGatherist.Web.Pages;

public class IndexModel 
    : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    
    public int? CategoryId { get; set; }

    public void OnGet(int? categoryId)
    {
        CategoryId = categoryId;
    }
}