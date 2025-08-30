using System.Net;
using Herontech.Api.AuthConfig;
using Herontech.Contracts;
using Herontech.Contracts.Dtos;
using Herontech.Contracts.Interfaces;
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
    public IQueryable<Client> Get() => db.Set<Client>().AsNoTracking();

    [EnableQuery]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public SingleResult<Client> Get([FromRoute] Guid key) => SingleResult.Create(
        db.Set<Client>().AsNoTracking().Where(c => c.Id == key)
    );

    
    
    

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Post(
        [FromBody] PostClientDto _input,
        [FromServices] ICrudService<Client> service,
        CancellationToken ct)
    {
        
        if (_input is null) return BadRequest();
        var result = await service.Create(_input);
        return result.Success 
            ? StatusCode((int)result.StatusCode, result.Error) 
            : StatusCode((int)result.StatusCode, result.Data);

        throw new NotImplementedException();
        var input = new Client();
        
        if (input.Id != Guid.Empty) return BadRequest("Id deve ser vazio no POST.");

        if (input.CompanyHeadQuartersId is { } hqId)
        {
            if (hqId == input.Id) return BadRequest("CompanyHeadQuartersId não pode apontar para o próprio registro.");
            bool hqExists = await db.Set<Client>().AnyAsync(c => c.Id == hqId, ct);
            if (!hqExists) return BadRequest("CompanyHeadQuartersId inválido.");
        }

        input.Id = Guid.NewGuid();
        input.CreatedAt = default;
        input.LastUpdatedAt = default;
        input.CreatorId = default;
        input.LastUpdaterId = default;

        db.Set<Client>().Add(input);
        await db.SaveChangesAsync(ct);
        return Created(input);
    }

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] Delta<Client> delta, CancellationToken ct)
    {
        if (delta is null) return BadRequest();

        var entity = await db.Set<Client>().FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        var backup = entity.BackupBaseEntity();
        delta.
            Patch(entity);
        backup.RestoreBaseEntityBackup(entity);

        if (delta.TryGetPropertyValue(nameof(Client.CompanyHeadQuartersId), out var hqObj))
        {
            if (hqObj is Guid hqId)
            {
                if (hqId == entity.Id) return BadRequest("CompanyHeadQuartersId não pode apontar para o próprio registro.");
                bool exists = await db.Set<Client>().AnyAsync(c => c.Id == hqId, ct);
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
        var entity = await db.Set<Client>().FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        db.Set<Client>().Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
