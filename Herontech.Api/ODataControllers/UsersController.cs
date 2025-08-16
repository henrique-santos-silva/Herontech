using Microsoft.AspNetCore.OData.Results;

namespace Herontech.Api.ODataControllers;

using Domain;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

[Route("odata/[controller]")]
public sealed class UsersController(AppDbContext db) : ODataController
{
    [EnableQuery(PageSize = 50)]
    // [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public IQueryable<User> Get() => db.Users.AsNoTracking();

    [EnableQuery]
    public SingleResult<User> Get([FromRoute] Guid key)
        => SingleResult.Create(db.Users.AsNoTracking().Where(u => u.Id == key));
}