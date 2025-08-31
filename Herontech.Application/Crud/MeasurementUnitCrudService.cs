using Herontech.Domain;
using Herontech.Infrastructure.Persistence;

namespace Herontech.Application.Crud;

public class MeasurementUnitCrudService : AbstractCrudService<MeasurementUnit>
{
    public MeasurementUnitCrudService(AppDbContext db) : base(db)
    {
    }
}