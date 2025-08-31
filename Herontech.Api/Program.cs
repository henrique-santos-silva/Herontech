using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Herontech.Api.AuthConfig;
using Herontech.Api.DependencyInjection;
using Herontech.Api.Routes;
using Herontech.Application;
using Herontech.Application.Security;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using TriStateNullable;

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
builder.Services
    .AddControllers()
    .AddOData(opt =>
    {
        opt.EnableQueryFeatures(maxTopValue: 100).AddRouteComponents("odata", GetEdmModel());
    }).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new TriStateNullableJsonConverterFactory());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    });

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new TriStateNullableJsonConverterFactory());
});
builder.Services.AddJwtConfig(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddCors(opt => {
    string? originDev  = builder.Configuration["Cors:Frontend:Dev"];
    string? originProd = builder.Configuration["Cors:Frontend:Prod"];

    opt.AddPolicy("Frontend", p =>
    {
        string[] origins = new[] { originDev, originProd }
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(x => x!)
            .ToArray();

        p.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
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
    .AddScoped<ITokenService, TokenService>()
    .AddScoped<IRefreshTokenService, RefreshTokenService>()
    .AddScoped<ICurrentUserService, CurrentUserService>()
    .AddCrudServices();

builder.Services.AddAuthorization();

WebApplication app = builder.Build();


app.UseCors("Frontend");    
app.MapOpenApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireCors("Frontend");
app.MapApi();


//app.UseHttpsRedirection();

app.Run();

