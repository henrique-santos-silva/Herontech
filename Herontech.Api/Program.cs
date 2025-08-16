using System.Text;
using Asp.Versioning;
using Herontech.Api.AuthConfig;
using Herontech.Api.Routes;
using Herontech.Application;
using Herontech.Application.Security;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

static IEdmModel GetEdmModel()
{
    var b = new ODataConventionModelBuilder();
    b.Namespace = "Herontech";
    b.ContainerName = "DefaultContainer";

    // EntitySets
    b.EntitySet<MeasurementUnit>("MeasurementUnits");
    b.EntitySet<User>("Users");
    b.EntitySet<Product>("Products");
    b.EntitySet<ProductCategory>("ProductCategories");
    b.EntitySet<Client>("Clients");
    b.EntitySet<Contact>("Contacts");
    return b.GetEdmModel();
}
builder.Services.AddControllers().AddOData(opt =>
{
    opt.EnableQueryFeatures(maxTopValue: 100).AddRouteComponents("odata", GetEdmModel());
});

builder.Services.AddJwtConfig(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddCors(opt => {
    opt.AddPolicy("Public", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddRateLimiter(opt => {
    opt.AddFixedWindowLimiter("fixed", o => {
        o.Window = TimeSpan.FromSeconds(10);
        o.PermitLimit = 30;
        o.QueueLimit = 0;
    });
});


builder.Services.AddDbContext<AppDbContext>(opt =>
{
    string cs = builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("ConnectionStrings:Default ausente");
    opt.UseMySql(
        cs,
        ServerVersion.AutoDetect(cs),
        mySqlOpt =>
        {
            mySqlOpt.MigrationsAssembly("Herontech.Infrastructure");
            mySqlOpt.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore);
        });
});
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddScoped<ITokenService,TokenService>()
    .AddScoped<IRefreshTokenService,RefreshTokenService>()
    .AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAuthorization();

WebApplication app = builder.Build();
app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapApi();


//app.UseHttpsRedirection();

app.Run();

