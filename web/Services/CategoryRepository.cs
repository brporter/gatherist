using System.Data;
using System.Text;
using Dapper;
using Microsoft.Extensions.Primitives;
using TheGatherist.Web.Models;
using TheGatherist.Web.Pages;

namespace TheGatherist.Web.Services;

public interface ICategoryRepository
{
    public Task<(Category?, IEnumerable<Category>)> GetCategoriesAsync(int tenantId, int? categoryId);
}

public class CategoryRepository(IConnectionFactory connectionFactory)
    : ICategoryRepository
{
    private const string CategoryConnection = "categories";

    public async Task<(Category?, IEnumerable<Category>)> GetCategoriesAsync(int tenantId, int? categoryId)
    {
        using var connection = connectionFactory.GetConnection(CategoryConnection);

        IDataReader results = null;

        if (categoryId is not null)
        {
            results = await connection.ExecuteReaderAsync(
                "select categoryid, tenantid, name, parentcategoryid from dbo.category where categoryid = @CategoryId or parentcategoryid = @CategoryId and tenantId = @TenantId",
                new { TenantId = tenantId, CategoryId = categoryId }
            );
        }
        else
        {
            results = await connection.ExecuteReaderAsync(
                "select categoryid, tenantid, name, parentcategoryid from dbo.category where parentcategoryid is null and tenantId = @TenantId",
                new { TenantId = tenantId }
            );
        }

        Category? root = null;
        List<Category> children = new List<Category>();

        while (results.Read())
        {
            var catId = results.GetInt32(0);
            if (categoryId == catId)
            {
                root = new Category(results.GetInt32(0),
                    results.GetInt32(1),
                    results.GetString(2),
                    results.IsDBNull(3) ? null : results.GetInt32(3));
            }
            else
            {
                children.Add(
                    new Category(results.GetInt32(0),
                    results.GetInt32(1),
                    results.GetString(2),
                    results.IsDBNull(3) ? null : results.GetInt32(3))
                );
            }
        }

        return (root, children);
    }
}