using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Herontech.Api.ODataControllers;

[Route("odata/[controller]")]
public sealed class ProductCategoriesController(AppDbContext db) : ODataController
{
    [EnableQuery(PageSize = 50)]
    public IQueryable<ProductCategory> Get()
        => db.ProductCategories.AsNoTracking();

    [EnableQuery]
    public SingleResult<ProductCategory> Get([FromRoute] Guid key)
        => SingleResult.Create(db.ProductCategories.AsNoTracking().Where(x => x.Id == key));
}