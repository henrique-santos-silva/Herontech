using Herontech.Api.AuthConfig;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public IQueryable<User> Get() => db.Set<User>().AsNoTracking();

    [EnableQuery]
    public SingleResult<User> Get([FromRoute] Guid key)
        => SingleResult.Create(db.Set<User>().AsNoTracking().Where(u => u.Id == key));
}