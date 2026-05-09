using E_Library.Database;

namespace E_Library.Services.Implementations.Categories;

public class GuidCategoryService : BaseCategoryService<Guid>
{
    public GuidCategoryService(AppDbContext context) : base(context)
    {
    }
}