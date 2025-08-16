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
public sealed class ProductsController(AppDbContext db) : ODataController
{
    [EnableQuery(PageSize = 50)]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]

    public IQueryable<Product> Get()
        => db.Products.AsNoTracking();

    [EnableQuery]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public SingleResult<Product> Get([FromRoute] Guid key)
        => SingleResult.Create(
            db.Products.AsNoTracking().Where(p => p.Id == key)
        );
    
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Post([FromBody] Product input, CancellationToken ct)
    {
        if (input is null) return BadRequest();
        if (input.Id != Guid.Empty) return BadRequest("Id deve ser vazio no POST.");

        input.Id = Guid.NewGuid();
        input.CreatedAt = default;
        input.LastUpdatedAt = default;
        input.CreatorId = default;
        input.LastUpdaterId = default;
        
        bool exists = await db.ProductCategories.AnyAsync(c => c.Id == input.ParentProductCategoryId, ct);
        if (!exists) return BadRequest("Categoria inválida.");
        
        db.Products.Add(input);
        await db.SaveChangesAsync(ct);
        return Created(input);
    }

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] Delta<Product> delta, CancellationToken ct)
    {
        if (delta is null) return BadRequest();

        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();
        var backup = entity.BackupBaseEntity();
        delta.Patch(entity);
        backup.RestoreBaseEntityBackup(entity);
        
        // valida categoria se veio no patch
        if (delta.TryGetPropertyValue(nameof(Product.ParentProductCategoryId), out var catObj) && catObj is Guid catId)
        {
            bool exists = await db.ProductCategories.AnyAsync(c => c.Id == catId, ct);
            if (!exists) return BadRequest("Categoria inválida.");
        }

        await db.SaveChangesAsync(ct);
        return Updated(entity);
    }

    [Authorize(Policy = AuthorizationPolicies.SysAdminOnly)]
    public async Task<IActionResult> Delete([FromRoute] Guid key, CancellationToken ct)
    {
        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        db.Products.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}