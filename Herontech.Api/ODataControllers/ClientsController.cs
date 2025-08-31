using Herontech.Api.AuthConfig;
using Herontech.Contracts;
using Herontech.Contracts.Dtos;
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

namespace Herontech.Api.ODataControllers;

[Route("odata/[controller]")]
public sealed class ClientsController 
    : AbstractBaseCrudODataController<Client, PostClientDto, PatchClientDto>
{
    public ClientsController(AppDbContext db, ICrudService<Client> svc, IAuthorizationService authz) 
        : base(db, svc, authz) {}

    // Leitura de lista pode expandir apenas PrimaryContact
    protected override string[] AllowedExpandsForList => ["Creator","PrimaryContact"];
    protected override int MaxExpandDepthList => 1;

    // Leitura item detalhado pode expandir Orders, Orders/Lines e PrimaryContact
    protected override string[] AllowedExpandsForItem => ["Creator","Orders", "Orders/Lines", "PrimaryContact"];
    protected override int MaxExpandDepthItem => 2;

    protected override ODataValidationSettings ValidationForItem => new()
    {
        AllowedQueryOptions = AllowedQueryOptions.Select
                              | AllowedQueryOptions.Expand
                              | AllowedQueryOptions.Filter
                              | AllowedQueryOptions.Count,
        MaxTop = 1
    };
}
