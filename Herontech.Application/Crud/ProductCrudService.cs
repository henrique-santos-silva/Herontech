using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class ProductCrudService : AbstractCrudService<Product>
{
    public ProductCrudService(AppDbContext db) : base(db)
    {
    }
}