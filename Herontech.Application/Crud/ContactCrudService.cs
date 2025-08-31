using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class ContactCrudService : AbstractCrudService<Contact>
{
    public ContactCrudService(AppDbContext db) : base(db)
    {
    }
}