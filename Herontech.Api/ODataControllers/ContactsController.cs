using Herontech.Api.AuthConfig;
using Herontech.Contracts.Dtos;
using Herontech.Contracts.Interfaces;
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


// public sealed class ContactsController 
//     : CrudODataController<Contact, PostClientDto, PatchClientDto>
// {
//     public ContactsController(AppDbContext db, ICrudService<Contact> svc, IAuthorizationService authz) 
//         : base(db, svc, authz) {}
//
// }