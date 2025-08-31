using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class PaymentTermCrudService : AbstractCrudService<PaymentTerm>
{
    public PaymentTermCrudService(AppDbContext db) : base(db)
    {
    }
}