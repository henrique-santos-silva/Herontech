using System.Net;
using Herontech.Contracts;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class ClientCrudService : AbstractCrudService<Client>
{
    public ClientCrudService(AppDbContext db) : base(db)
    {
        
    }
}