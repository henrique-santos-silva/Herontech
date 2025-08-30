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
public sealed class QuotesController(AppDbContext db) : ODataController
{
    public AppDbContext Db => db;
    
    [EnableQuery(PageSize = 50)]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public IQueryable<Quote> Get() => db.Set<Quote>().AsNoTracking();

    [EnableQuery]
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public SingleResult<Quote> Get([FromRoute] Guid key) => SingleResult.Create(
        db.Set<Quote>().AsNoTracking().Where(c => c.Id == key)
    );

    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Post([FromBody] Quote input, CancellationToken ct)
    {
        if (input is null) return BadRequest();
        input.BaseEntityDefaults();
        
        var today = DateTime.UtcNow;


        foreach (var quoteRevision in input.QuoteRevisions)
        {
            foreach (var quoteItem in quoteRevision.Items)
            {
                foreach (QuoteProduct quoteProduct in quoteItem.QuoteProducts)
                {
                    quoteProduct.Total0 = quoteProduct.Quantity * quoteProduct.UnitPrice;
                    quoteProduct.Total1AfterMarkup = quoteProduct.Total0 / quoteProduct.MarkupPercentage;
                    quoteProduct.Total2AfterDiscount  = quoteProduct.Total1AfterMarkup * (1 - quoteRevision.DiscountPercentage);
                    quoteProduct.Total3AfterComission = quoteProduct.Total2AfterDiscount / (1 - quoteRevision.SalesPersonCommissionPercentage);
                    quoteRevision.SalesPersonCommission += (quoteProduct.Total3AfterComission * quoteRevision.SalesPersonCommissionPercentage);
                    quoteProduct.Total4AfterTaxes = quoteProduct.Total3AfterComission * (1 + quoteRevision.TaxPercentage);
                    
                    quoteItem.TotalBeforeTaxes += quoteProduct.Total3AfterComission;
                    quoteItem.TotalAfterTaxes  += quoteProduct.Total4AfterTaxes;
                    
                    quoteRevision.TotalBeforeTaxes += quoteProduct.Total3AfterComission;
                    quoteRevision.TotalAfterTaxes  += quoteProduct.Total4AfterTaxes;
                }
                
            }

        }
        
        
        db.Set<Quote>().Add(input);
        await db.SaveChangesAsync(ct);
        return Created(input);
    }
    
    [Authorize(Policy = AuthorizationPolicies.EmployeeOrHigher)]
    public async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] Delta<Quote> delta, CancellationToken ct)
    {
        if (delta is null) return BadRequest();

        var entity = await db.Set<Quote>().FirstOrDefaultAsync(p => p.Id == key, ct);
        if (entity is null) return NotFound();

        var backup = entity.BackupBaseEntity();
        delta.Patch(entity);
        backup.RestoreBaseEntityBackup(entity);
        
        await db.SaveChangesAsync(ct);
        return Updated(entity);
    }
    
    
}