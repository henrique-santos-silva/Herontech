using Herontech.Api.AuthConfig;
using Herontech.Contracts;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.UriParser;

namespace Herontech.Api.ODataControllers;

[Authorize]
[Route("odata/[controller]")]
public abstract class AbstractBaseCrudODataController<TEntity, TPostDto, TPatchDto> : ODataController
    where TEntity : BaseEntity, new()
    where TPostDto  : IInto<TEntity>
    where TPatchDto : IIntoPatch<TEntity>
{
    protected readonly AppDbContext Db;
    protected readonly ICrudService<TEntity> Service;
    protected readonly IAuthorizationService Authz;

    protected AbstractBaseCrudODataController(AppDbContext db, ICrudService<TEntity> service, IAuthorizationService authz)
    {
        Db = db;
        Service = service;
        Authz = authz;
    }

    protected virtual string ReadPolicyForList  => AuthorizationPolicies.EmployeeOrHigher;
    protected virtual string ReadPolicyForItem  => AuthorizationPolicies.EmployeeOrHigher;
    protected virtual string CreatePolicy        => AuthorizationPolicies.EmployeeOrHigher;
    protected virtual string UpdatePolicy        => AuthorizationPolicies.EmployeeOrHigher;
    protected virtual string DeletePolicy       => AuthorizationPolicies.SysAdminOnly;

    protected virtual ODataValidationSettings ValidationForList => new()
    {
        AllowedQueryOptions = AllowedQueryOptions.Select
                              | AllowedQueryOptions.Filter
                              | AllowedQueryOptions.OrderBy
                              | AllowedQueryOptions.Expand
                              | AllowedQueryOptions.Skip
                              | AllowedQueryOptions.Top
                              | AllowedQueryOptions.Count,
        MaxTop = 100
    };

    protected virtual ODataValidationSettings ValidationForItem => new()
    {
        AllowedQueryOptions = AllowedQueryOptions.Select
                              | AllowedQueryOptions.Expand
                              | AllowedQueryOptions.Count
    };

    protected virtual ODataQuerySettings QuerySettingsForList => new() { PageSize = 50 };
    protected virtual ODataQuerySettings QuerySettingsForItem => new();

    protected virtual string[] AllowedExpandsForList => Array.Empty<string>();
    protected virtual string[] AllowedExpandsForItem => Array.Empty<string>();
    protected virtual int MaxExpandDepthList => 2;
    protected virtual int MaxExpandDepthItem => 3;

    protected virtual IQueryable<TEntity> QueryBase(IQueryable<TEntity> q) => q;

    public virtual async Task<IActionResult> Get(ODataQueryOptions<TEntity> options, CancellationToken ct)
    {
        var auth = await Authz.AuthorizeAsync(User, ReadPolicyForList);
        if (!auth.Succeeded) return Forbid();

        options.Validate(ValidationForList);

        var bad = ValidateExpands(options.SelectExpand, AllowedExpandsForList, MaxExpandDepthList, this);
        if (bad is not null) return bad;

        var q = QueryBase(Db.Set<TEntity>().AsNoTracking());
        var applied = options.ApplyTo(q, QuerySettingsForList);            // sem cast
        return Ok(applied);
    }


    public virtual async Task<IActionResult> Get([FromRoute] Guid key, ODataQueryOptions<TEntity> options, CancellationToken ct)
    {
        var auth = await Authz.AuthorizeAsync(User, ReadPolicyForItem);
        if (!auth.Succeeded) return Forbid();

        options.Validate(ValidationForItem);

        var bad = ValidateExpands(options.SelectExpand, AllowedExpandsForItem, MaxExpandDepthItem, this);
        if (bad is not null) return bad;

        var q = QueryBase(Db.Set<TEntity>().AsNoTracking()).Where(e => e.Id == key);

        if (HasProjection(options))
        {
            var applied = options.ApplyTo(q, QuerySettingsForItem); // wrapper
            var iq = (IQueryable)applied;
            var one = iq.Cast<object>().FirstOrDefault();           // materializa 1
            return one is null ? NotFound() : Ok(one);              // não usa SingleResult
        }

        // sem $select/$expand ainda é TEntity
        var filtered = q;
        if (options.Filter != null)   filtered = (IQueryable<TEntity>)options.Filter.ApplyTo(filtered, QuerySettingsForItem);
        if (options.OrderBy != null)  filtered = (IQueryable<TEntity>)options.OrderBy.ApplyTo(filtered, QuerySettingsForItem);
        if (options.Skip != null)     filtered = (IQueryable<TEntity>)options.Skip.ApplyTo(filtered, QuerySettingsForItem);
        if (options.Top != null)      filtered = (IQueryable<TEntity>)options.Top.ApplyTo(filtered, QuerySettingsForItem);

        var sr = SingleResult.Create(filtered);
        return sr.Queryable.Any() ? Ok(sr) : NotFound();
    }
    
    public virtual async Task<IActionResult> Post([FromBody] TPostDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest();

        var auth = await Authz.AuthorizeAsync(User, CreatePolicy);
        if (!auth.Succeeded) return Forbid();

        var result = await Service.Create(dto, ct);
        return ToActionResult(result);
    }

    public virtual async Task<IActionResult> Patch([FromRoute] Guid key, [FromBody] TPatchDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest();

        var auth = await Authz.AuthorizeAsync(User, UpdatePolicy);
        if (!auth.Succeeded) return Forbid();

        var result = await Service.Update(key, dto, ct);
        return ToActionResult(result);
    }

    [HttpDelete("{key:guid}")]
    public virtual async Task<IActionResult> Delete([FromRoute(Name = "key")] Guid key, CancellationToken ct)
    {
        var auth = await Authz.AuthorizeAsync(User, DeletePolicy);
        if (!auth.Succeeded) return Forbid();

        var result = await Service.Delete(key, ct);
        return ToActionResult(result);
    }
    
    protected IActionResult? ValidateExpands(SelectExpandQueryOption? selectExpand, string[] allowed, int maxDepth, ControllerBase c)
    {
        if (selectExpand is null) return null;

        var paths = GetExpandPaths(selectExpand.SelectExpandClause).ToList();

        if (paths.Any(p => p.Split('/').Length > maxDepth))
            return c.BadRequest($"$expand depth acima de {maxDepth}.");

        if (allowed.Length > 0)
        {
            var allowedSet = new HashSet<string>(allowed, StringComparer.Ordinal);
            var invalid = paths.Where(p => !allowedSet.Contains(p)).ToList();
            if (invalid.Count > 0)
                return c.BadRequest($"$expand não permitido: {string.Join(", ", invalid)}.");
        }

        return null;
    }

    private static IEnumerable<string> GetExpandPaths(SelectExpandClause? clause)
    {
        if (clause is null) yield break;

        foreach (var item in clause.SelectedItems)
        {
            if (item is ExpandedNavigationSelectItem en)
            {
                var navPath = string.Join("/",
                    en.PathToNavigationProperty
                        .OfType<NavigationPropertySegment>()
                        .Select(s => s.NavigationProperty.Name));

                if (!string.IsNullOrEmpty(navPath))
                    yield return navPath;

                foreach (var nested in GetExpandPaths(en.SelectAndExpand))
                    yield return string.IsNullOrEmpty(navPath) ? nested : $"{navPath}/{nested}";
            }
        }
    }
    
    
    private IActionResult ToActionResult<T>(ApiResultDto<T> r)
        => StatusCode((int)r.StatusCode, r.Success ? r.Data : r.Error);

    private IActionResult ToActionResult(ApiResultVoid r) => r.Success 
        ? StatusCode((int)r.StatusCode) 
        : StatusCode((int)r.StatusCode, r.Error);
    
    private static bool HasProjection<TEntity>(ODataQueryOptions<TEntity> o) => o.SelectExpand is not null;


}
