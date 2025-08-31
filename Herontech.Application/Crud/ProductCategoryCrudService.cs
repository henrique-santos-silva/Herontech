using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class ProductCategoryCrudService : AbstractCrudService<ProductCategory>
{
    public ProductCategoryCrudService(AppDbContext db) : base(db)
    {
    }
}