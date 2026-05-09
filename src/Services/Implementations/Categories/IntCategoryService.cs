using E_Library.Database;

namespace E_Library.Services.Implementations.Categories;

public class IntCategoryService : BaseCategoryService<int>
{
    public IntCategoryService(AppDbContext context) : base(context)
    {
    }
}