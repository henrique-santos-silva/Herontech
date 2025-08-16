using Herontech.Api.AuthConfig;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Herontech.Api.ODataControllers;

[Route("odata/[controller]")]
public sealed class ClientsController(AppDbContext db) : ODataController
{
    [EnableQuery(PageSize = 50)]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public IQueryable<Client> Get() => db.Clients.AsNoTracking();

    [EnableQuery]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public SingleResult<Client> Get([FromRoute] Guid key) => SingleResult.Create(
        db.Clients.AsNoTracking().Where(c => c.Id == key)
    );

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Post([FromBody] Client input, CancellationToken ct)
    {
        if (input is null) return BadRequest();
        if (input.Id != Guid.Empty) return BadRequest("Id deve ser vazio no POST.");

        if (input.CompanyHeadQuartersId is { } hqId)
        {
            if (hqId == input.Id) return BadRequest("CompanyHeadQuartersId não pode apontar para o próprio registro.");
            bool hqExists = await db.Clients.AnyAsync(c => c.Id == hqId, ct);
            if (!hqExists) return BadRequest("CompanyHeadQuartersId inválido.");
        }

        input.Id = Guid.NewGuid();
        input.CreatedAt = default;
        input.LastUpdatedAt = default;
        input.CreatorId = default;
        input.LastUpdaterId = default;

        db.Clients.Add(input);
        await db.SaveChangesAsync(ct);
        return Created(input);
    }

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] Delta<Client> delta, CancellationToken ct)
    {
        if (delta is null) return BadRequest();

        var entity = await db.Clients.FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        var backup = entity.BackupBaseEntity();
        delta.Patch(entity);
        backup.RestoreBaseEntityBackup(entity);

        if (delta.TryGetPropertyValue(nameof(Client.CompanyHeadQuartersId), out var hqObj))
        {
            if (hqObj is Guid hqId)
            {
                if (hqId == entity.Id) return BadRequest("CompanyHeadQuartersId não pode apontar para o próprio registro.");
                bool exists = await db.Clients.AnyAsync(c => c.Id == hqId, ct);
                if (!exists) return BadRequest("CompanyHeadQuartersId inválido.");
            }
            else if (hqObj is not null)
            {
                return BadRequest("CompanyHeadQuartersId inválido.");
            }
        }

        await db.SaveChangesAsync(ct);
        return Updated(entity);
    }

    // DELETE /odata/Clients(<guid>)
    [Authorize(Policy = AuthorizationPolicies.SysAdminOnly)]
    public async Task<IActionResult> Delete([FromRoute] Guid key, CancellationToken ct)
    {
        var entity = await db.Clients.FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        db.Clients.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
