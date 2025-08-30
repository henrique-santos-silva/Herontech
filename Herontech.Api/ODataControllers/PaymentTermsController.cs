using Herontech.Api.AuthConfig;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Herontech.Api.ODataControllers;

[Route("odata/[controller]")]
public sealed class PaymentTermsController(AppDbContext db) : ODataController
{
    public AppDbContext Db => db;
    
    [EnableQuery(PageSize = 50)]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public IQueryable<PaymentTerm> Get() => db.Set<PaymentTerm>().AsNoTracking();

    [EnableQuery]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public SingleResult<PaymentTerm> Get([FromRoute] Guid key) => SingleResult.Create(
        db.Set<PaymentTerm>().AsNoTracking().Where(c => c.Id == key)
    );

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Post([FromBody] PaymentTerm input, CancellationToken ct)
    {
        if (input is null) return BadRequest();
        input.BaseEntityDefaults();
        db.Set<PaymentTerm>().Add(input);
        await db.SaveChangesAsync(ct);
        return Created(input);
    }
    
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] Delta<PaymentTerm> delta, CancellationToken ct)
    {
        if (delta is null) return BadRequest();

        var entity = await db.Set<PaymentTerm>().FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        var backup = entity.BackupBaseEntity();
        delta.Patch(entity);
        backup.RestoreBaseEntityBackup(entity);
        
        await db.SaveChangesAsync(ct);
        return Updated(entity);
    }
    
    [Authorize(Policy = AuthorizationPolicies.SysAdminOnly)]
    public async Task<IActionResult> Delete([FromRoute] Guid key, CancellationToken ct)
    {
        var entity = await db.Set<PaymentTerm>().FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        db.Set<PaymentTerm>().Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}